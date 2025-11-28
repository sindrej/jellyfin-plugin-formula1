using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.TheSportsDB.API.Models;
using Jellyfin.Plugin.TheSportsDB.Logger;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TheSportsDB.API;

/// <summary>
/// Client for interacting with TheSportsDB API.
/// </summary>
public class TheSportsDBClient : IDisposable
{
    private const string BaseUrl = "https://www.thesportsdb.com/api/v1/json";

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PrefixedLogger<TheSportsDBClient> _logger;
    private readonly SemaphoreSlim _rateLimitSemaphore = new(1, 1);
    private readonly Queue<DateTime> _requestTimestamps = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="TheSportsDBClient"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger.</param>
    public TheSportsDBClient(IHttpClientFactory httpClientFactory, ILogger<TheSportsDBClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = new PrefixedLogger<TheSportsDBClient>(logger);
    }

    /// <summary>
    /// Gets the API key from plugin configuration.
    /// </summary>
    private static string ApiKey => Plugin.Instance?.Configuration?.ApiKey ?? "123";

    /// <summary>
    /// Gets the maximum requests per minute from plugin configuration.
    /// </summary>
    private static int MaxRequestsPerMinute => Plugin.Instance?.Configuration?.MaxRequestsPerMinute ?? 30;

    /// <summary>
    /// Enforces rate limiting before making API requests.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task EnforceRateLimitAsync(CancellationToken cancellationToken)
    {
        await _rateLimitSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var now = DateTime.UtcNow;
            var oneMinuteAgo = now.AddMinutes(-1);

            // Remove timestamps older than 1 minute
            while (_requestTimestamps.Count > 0 && _requestTimestamps.Peek() < oneMinuteAgo)
            {
                _requestTimestamps.Dequeue();
            }

            // If we've hit the limit, wait until we can make another request
            if (_requestTimestamps.Count >= MaxRequestsPerMinute)
            {
                var oldestRequest = _requestTimestamps.Peek();
                var waitTime = oldestRequest.AddMinutes(1) - now;
                if (waitTime > TimeSpan.Zero)
                {
                    _logger.LogWarning("Rate limit reached. Waiting {Seconds} seconds before next request.", waitTime.TotalSeconds);
                    await Task.Delay(waitTime, cancellationToken).ConfigureAwait(false);
                }

                _requestTimestamps.Dequeue();
            }

            _requestTimestamps.Enqueue(now);
        }
        finally
        {
            _rateLimitSemaphore.Release();
        }
    }

    /// <summary>
    /// Makes an HTTP GET request to the API with retry logic.
    /// </summary>
    /// <typeparam name="T">The response type.</typeparam>
    /// <param name="endpoint">The API endpoint.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    private async Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken)
    {
        await EnforceRateLimitAsync(cancellationToken).ConfigureAwait(false);

        var url = $"{BaseUrl}/{ApiKey}/{endpoint}";
        _logger.LogDebug("Requesting: {Url}", url);

        var httpClient = _httpClientFactory.CreateClient();
        const int MaxRetries = 3;
        var retryDelay = TimeSpan.FromSeconds(2);

        for (var attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                var response = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("Rate limit exceeded (429). Waiting 60 seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken).ConfigureAwait(false);
                    continue;
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                var result = JsonSerializer.Deserialize<T>(content, _jsonOptions);

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed (attempt {Attempt}/{MaxRetries})", attempt + 1, MaxRetries);

                if (attempt < MaxRetries - 1)
                {
                    await Task.Delay(retryDelay * (attempt + 1), cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    throw;
                }
            }
        }

        return default;
    }

    /// <summary>
    /// Gets all events for a specific league and season.
    /// </summary>
    /// <param name="leagueId">The league ID.</param>
    /// <param name="season">The season year (e.g., 2024).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of events.</returns>
    public async Task<IReadOnlyList<Event>> GetEventsForSeasonAsync(string leagueId, int season, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching events for league {LeagueId}, season: {Season}", leagueId, season);
        var endpoint = $"eventsseason.php?id={leagueId}&s={season}";
        var response = await GetAsync<EventsResponse>(endpoint, cancellationToken).ConfigureAwait(false);
        var events = response?.Events ?? Array.Empty<Event>();
        _logger.LogDebug("Found {Count} events for league {LeagueId}, season {Season}", events.Count, leagueId, season);
        if (events.Count > 0)
        {
            _logger.LogDebug(
                "Sample event: {EventName} (Round {Round}, ID: {EventId})",
                events[0].Name,
                events[0].Round,
                events[0].Id);
        }

        return events;
    }

    /// <summary>
    /// Gets a specific event by ID.
    /// </summary>
    /// <param name="eventId">The event ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The event details.</returns>
    public async Task<Event?> GetEventByIdAsync(string eventId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching event by ID: {EventId}", eventId);
        var endpoint = $"lookupevent.php?id={eventId}";
        var response = await GetAsync<EventsResponse>(endpoint, cancellationToken).ConfigureAwait(false);
        var evt = response?.Events is { Count: > 0 } events ? events[0] : null;
        if (evt != null)
        {
            _logger.LogDebug(
                "Found event: {EventName} (Round {Round}, Season {Season})",
                evt.Name,
                evt.Round,
                evt.Season);
        }
        else
        {
            _logger.LogWarning("No event found with ID: {EventId}", eventId);
        }

        return evt;
    }

    /// <summary>
    /// Gets all available leagues from TheSportsDB.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of leagues.</returns>
    public async Task<IReadOnlyList<League>> GetAllLeaguesAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching all leagues");
        var endpoint = "all_leagues.php";
        var response = await GetAsync<LeaguesResponse>(endpoint, cancellationToken).ConfigureAwait(false);
        var leagues = response?.Leagues ?? Array.Empty<League>();
        _logger.LogDebug("Found {Count} leagues", leagues.Count);
        return leagues;
    }

    /// <summary>
    /// Gets detailed information about a specific league.
    /// </summary>
    /// <param name="leagueId">The league ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The league details.</returns>
    public async Task<League?> GetLeagueByIdAsync(string leagueId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching league details for ID: {LeagueId}", leagueId);
        var endpoint = $"lookupleague.php?id={leagueId}";
        var response = await GetAsync<LeagueDetailsResponse>(endpoint, cancellationToken).ConfigureAwait(false);
        var league = response?.Leagues is { Count: > 0 } leagues ? leagues[0] : null;
        if (league != null)
        {
            _logger.LogDebug("Found league: {LeagueName} (ID: {LeagueId})", league.Name, league.Id);
        }
        else
        {
            _logger.LogWarning("No league found with ID: {LeagueId}", leagueId);
        }

        return league;
    }

    /// <summary>
    /// Gets all seasons for a specific league.
    /// </summary>
    /// <param name="leagueId">The league ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of seasons.</returns>
    public async Task<IReadOnlyList<Season>> GetLeagueSeasonsAsync(string leagueId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching seasons for league ID: {LeagueId}", leagueId);
        var endpoint = $"search_all_seasons.php?id={leagueId}&poster=1";
        var response = await GetAsync<SeasonsResponse>(endpoint, cancellationToken).ConfigureAwait(false);
        var seasons = response?.Seasons ?? Array.Empty<Season>();
        _logger.LogDebug("Found {Count} seasons for league {LeagueId}", seasons.Count, leagueId);
        return seasons;
    }

    /// <summary>
    /// Searches for events by name.
    /// </summary>
    /// <param name="eventName">The event name to search for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of matching events.</returns>
    public async Task<IReadOnlyList<Event>> SearchEventsAsync(string eventName, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Searching for events with name: {EventName}", eventName);
        var endpoint = $"searchevents.php?e={Uri.EscapeDataString(eventName)}";
        var response = await GetAsync<EventsResponse>(endpoint, cancellationToken).ConfigureAwait(false);
        var events = response?.Events ?? Array.Empty<Event>();
        _logger.LogDebug("Found {Count} events matching '{EventName}'", events.Count, eventName);
        return events;
    }

    /// <summary>
    /// Disposes the client and releases resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the client and releases resources.
    /// </summary>
    /// <param name="disposing">True if called from Dispose, false if called from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _rateLimitSemaphore?.Dispose();
        }

        _disposed = true;
    }
}
