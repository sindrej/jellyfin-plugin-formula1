using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.TheSportsDB.API.Models;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TheSportsDB.API;

/// <summary>
/// Client for interacting with TheSportsDB API.
/// </summary>
public class TheSportsDBClient : IDisposable
{
    private const string BaseUrl = "https://www.thesportsdb.com/api/v1/json";
    private const int FormulaOneLeagueId = 4370;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TheSportsDBClient> _logger;
    private readonly SemaphoreSlim _rateLimitSemaphore = new SemaphoreSlim(1, 1);
    private readonly Queue<DateTime> _requestTimestamps = new Queue<DateTime>();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="TheSportsDBClient"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger.</param>
    public TheSportsDBClient(IHttpClientFactory httpClientFactory, ILogger<TheSportsDBClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Gets the API key from plugin configuration.
    /// </summary>
    private string ApiKey => Plugin.Instance?.Configuration?.ApiKey ?? "3";

    /// <summary>
    /// Gets the maximum requests per minute from plugin configuration.
    /// </summary>
    private int MaxRequestsPerMinute => Plugin.Instance?.Configuration?.MaxRequestsPerMinute ?? 30;

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
        var maxRetries = 3;
        var retryDelay = TimeSpan.FromSeconds(2);

        for (var attempt = 0; attempt < maxRetries; attempt++)
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
                var result = JsonSerializer.Deserialize<T>(content, JsonOptions);

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed (attempt {Attempt}/{MaxRetries})", attempt + 1, maxRetries);

                if (attempt < maxRetries - 1)
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
    /// Gets all events (races) for a specific Formula 1 season.
    /// </summary>
    /// <param name="season">The season year (e.g., 2024).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of events.</returns>
    public async Task<IReadOnlyList<Event>> GetEventsForSeasonAsync(int season, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching events for F1 season: {Season}", season);
        var endpoint = $"eventsseason.php?id={FormulaOneLeagueId}&s={season}";
        var response = await GetAsync<EventsResponse>(endpoint, cancellationToken).ConfigureAwait(false);
        var events = response?.Events ?? Array.Empty<Event>();
        _logger.LogDebug("Found {Count} events for season {Season}", events.Count, season);
        if (events.Count > 0)
        {
            _logger.LogDebug(
                "Sample event: {EventName} (Round {Round}, ID: {EventId})",
                events[0].StrEvent,
                events[0].IntRound,
                events[0].IdEvent);
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
                evt.StrEvent,
                evt.IntRound,
                evt.StrSeason);
        }
        else
        {
            _logger.LogWarning("No event found with ID: {EventId}", eventId);
        }

        return evt;
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
    /// Gets all Formula 1 teams.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of teams.</returns>
    public async Task<IReadOnlyList<Team>> GetFormulaOneTeamsAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching all Formula 1 teams");
        var endpoint = "search_all_teams.php?l=Formula%201";
        var response = await GetAsync<TeamsResponse>(endpoint, cancellationToken).ConfigureAwait(false);
        var teams = response?.Teams ?? Array.Empty<Team>();
        _logger.LogDebug("Found {Count} F1 teams", teams.Count);
        return teams;
    }

    /// <summary>
    /// Gets a specific team by ID.
    /// </summary>
    /// <param name="teamId">The team ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The team details.</returns>
    public async Task<Team?> GetTeamByIdAsync(string teamId, CancellationToken cancellationToken)
    {
        var endpoint = $"lookupteam.php?id={teamId}";
        var response = await GetAsync<TeamsResponse>(endpoint, cancellationToken).ConfigureAwait(false);
        return response?.Teams is { Count: > 0 } teams ? teams[0] : null;
    }

    /// <summary>
    /// Gets all drivers for a specific team.
    /// </summary>
    /// <param name="teamId">The team ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of drivers.</returns>
    public async Task<IReadOnlyList<Player>> GetTeamDriversAsync(string teamId, CancellationToken cancellationToken)
    {
        var endpoint = $"lookup_all_players.php?id={teamId}";
        var response = await GetAsync<PlayersResponse>(endpoint, cancellationToken).ConfigureAwait(false);
        return response?.Players ?? Array.Empty<Player>();
    }

    /// <summary>
    /// Gets a specific driver by ID.
    /// </summary>
    /// <param name="playerId">The driver ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The driver details.</returns>
    public async Task<Player?> GetPlayerByIdAsync(string playerId, CancellationToken cancellationToken)
    {
        var endpoint = $"lookupplayer.php?id={playerId}";
        var response = await GetAsync<PlayersResponse>(endpoint, cancellationToken).ConfigureAwait(false);
        return response?.Players is { Count: > 0 } players ? players[0] : null;
    }

    /// <summary>
    /// Searches for drivers by name.
    /// </summary>
    /// <param name="playerName">The driver name to search for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of matching drivers.</returns>
    public async Task<IReadOnlyList<Player>> SearchPlayersAsync(string playerName, CancellationToken cancellationToken)
    {
        var endpoint = $"searchplayers.php?p={Uri.EscapeDataString(playerName)}";
        var response = await GetAsync<PlayersResponse>(endpoint, cancellationToken).ConfigureAwait(false);
        return response?.Players ?? Array.Empty<Player>();
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
