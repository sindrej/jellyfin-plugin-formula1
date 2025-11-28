using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.TheSportsDB.API;
using Jellyfin.Plugin.TheSportsDB.Logger;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TheSportsDB.Providers;

/// <summary>
/// Provides sports event metadata for episodes.
/// Maps: Event → Episode within a League/Season structure.
/// </summary>
public partial class TheSportsDBEpisodeProvider : IRemoteMetadataProvider<Episode, EpisodeInfo>
{
    private static readonly Regex _roundNumberRegex = MyRegex();

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PrefixedLogger<TheSportsDBEpisodeProvider> _logger;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TheSportsDBEpisodeProvider"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public TheSportsDBEpisodeProvider(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
    {
        _httpClientFactory = httpClientFactory;
        _loggerFactory = loggerFactory;
        var baseLogger = loggerFactory.CreateLogger<TheSportsDBEpisodeProvider>();
        _logger = new PrefixedLogger<TheSportsDBEpisodeProvider>(baseLogger);
    }

    /// <inheritdoc />
    public string Name => "TheSportsDB";

    /// <inheritdoc />
    public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(EpisodeInfo searchInfo, CancellationToken cancellationToken)
    {
        if (!IsEnabled())
        {
            return Enumerable.Empty<RemoteSearchResult>();
        }

        var results = new List<RemoteSearchResult>();

        try
        {
            var client = new TheSportsDBClient(_httpClientFactory, _loggerFactory.CreateLogger<TheSportsDBClient>());

            // Get league ID from series provider IDs
            var leagueId = searchInfo.SeriesProviderIds?.GetValueOrDefault("TheSportsDB");
            var seasonYear = searchInfo.ParentIndexNumber;

            if (!string.IsNullOrEmpty(leagueId) && seasonYear.HasValue)
            {
                _logger.LogDebug("Searching for events: League {LeagueId}, Season {Season}", leagueId, seasonYear);
                var events = await client.GetEventsForSeasonAsync(leagueId, seasonYear.Value, cancellationToken).ConfigureAwait(false);

                foreach (var evt in events)
                {
                    var result = new RemoteSearchResult
                    {
                        Name = evt.Name,
                        SearchProviderName = Name,
                        ImageUrl = evt.Poster ?? evt.Thumb,
                        Overview = evt.Description ?? evt.Result
                    };

                    if (!string.IsNullOrEmpty(evt.Id))
                    {
                        result.SetProviderId("TheSportsDB", evt.Id);
                    }

                    if (!string.IsNullOrEmpty(evt.Date) && DateTime.TryParse(evt.Date, out var eventDate))
                    {
                        result.PremiereDate = eventDate;
                    }

                    if (!string.IsNullOrEmpty(evt.Round) && int.TryParse(evt.Round, out var roundNumber))
                    {
                        result.IndexNumber = roundNumber;
                    }

                    results.Add(result);
                }
            }
            else if (!string.IsNullOrEmpty(searchInfo.Name))
            {
                // Fallback: search by name
                _logger.LogDebug("Searching events by name: {Name}", searchInfo.Name);
                var events = await client.SearchEventsAsync(searchInfo.Name, cancellationToken).ConfigureAwait(false);

                foreach (var evt in events.Where(e => e.Season != null))
                {
                    var result = new RemoteSearchResult
                    {
                        Name = evt.Name,
                        SearchProviderName = Name,
                        ImageUrl = evt.Poster ?? evt.Thumb,
                        Overview = evt.Description ?? evt.Result
                    };

                    if (!string.IsNullOrEmpty(evt.Id))
                    {
                        result.SetProviderId("TheSportsDB", evt.Id);
                    }

                    if (!string.IsNullOrEmpty(evt.Date) && DateTime.TryParse(evt.Date, out var eventDate))
                    {
                        result.PremiereDate = eventDate;
                    }

                    results.Add(result);
                }
            }
            else
            {
                _logger.LogWarning("Cannot search for episodes without league ID and season or event name");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for event metadata");
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<MetadataResult<Episode>> GetMetadata(EpisodeInfo info, CancellationToken cancellationToken)
    {
        var result = new MetadataResult<Episode>();

        if (!IsEnabled())
        {
            _logger.LogDebug("TheSportsDB plugin is disabled");
            return result;
        }

        _logger.LogInformation(
            "Getting metadata for episode: Name={Name}, IndexNumber={IndexNumber}, ParentIndexNumber={ParentIndexNumber}, Path={Path}",
            info.Name,
            info.IndexNumber,
            info.ParentIndexNumber,
            info.Path);

        try
        {
            var client = new TheSportsDBClient(_httpClientFactory, _loggerFactory.CreateLogger<TheSportsDBClient>());
            var eventId = info.GetProviderId("TheSportsDB");

            API.Models.Event? raceEvent = null;

            if (!string.IsNullOrEmpty(eventId))
            {
                // Fetch by ID if we have it
                _logger.LogDebug("Fetching event using provider ID: {EventId}", eventId);
                raceEvent = await client.GetEventByIdAsync(eventId, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Try to find the event by league, season, and episode number
                var leagueId = info.SeriesProviderIds?.GetValueOrDefault("TheSportsDB");
                var seasonYear = info.ParentIndexNumber;
                var roundNumber = info.IndexNumber ?? ExtractRoundNumber(info.Name);

                _logger.LogDebug("League ID: {LeagueId}, Season: {Season}, Round: {Round}", leagueId, seasonYear, roundNumber);

                if (!string.IsNullOrEmpty(leagueId) && seasonYear.HasValue && roundNumber.HasValue)
                {
                    _logger.LogInformation(
                        "Searching for event: League {LeagueId}, Season {Season}, Round {Round}",
                        leagueId,
                        seasonYear.Value,
                        roundNumber.Value);

                    var events = await client.GetEventsForSeasonAsync(
                        leagueId,
                        seasonYear.Value,
                        cancellationToken).ConfigureAwait(false);

                    raceEvent = events.FirstOrDefault(e =>
                        !string.IsNullOrEmpty(e.Round) &&
                        int.TryParse(e.Round, out var round) &&
                        round == roundNumber.Value);

                    if (raceEvent != null)
                    {
                        _logger.LogInformation("Found event by round number: {EventName}", raceEvent.Name);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "No event found for League {LeagueId}, Season {Season}, Round {Round}",
                            leagueId,
                            seasonYear.Value,
                            roundNumber.Value);
                    }
                }

                // Fallback: Try fuzzy matching by event name
                if (raceEvent == null && !string.IsNullOrEmpty(leagueId) && seasonYear.HasValue && !string.IsNullOrEmpty(info.Name))
                {
                    var eventName = ExtractEventName(info.Name);
                    if (!string.IsNullOrEmpty(eventName))
                    {
                        _logger.LogInformation("Trying fuzzy match by event name: '{EventName}'", eventName);
                        var events = await client.GetEventsForSeasonAsync(leagueId, seasonYear.Value, cancellationToken).ConfigureAwait(false);

                        // Try exact match first
                        raceEvent = events.FirstOrDefault(e =>
                            e.Name != null &&
                            e.Name.Contains(eventName, StringComparison.OrdinalIgnoreCase));

                        if (raceEvent != null)
                        {
                            _logger.LogInformation("Found event by fuzzy name match: {EventName}", raceEvent.Name);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "No event found matching event name '{EventName}' in league {LeagueId}, season {Season}",
                                eventName,
                                leagueId,
                                seasonYear.Value);
                        }
                    }
                }
            }

            if (raceEvent != null)
            {
                _logger.LogInformation(
                    "Setting metadata for event: {EventName} (Season {Season}, Round {Round})",
                    raceEvent.Name,
                    raceEvent.Season,
                    raceEvent.Round);

                result.Item = new Episode
                {
                    Name = raceEvent.Name,
                    Overview = raceEvent.Description ?? raceEvent.Result,
                    CommunityRating = null
                };

                if (!string.IsNullOrEmpty(raceEvent.Id))
                {
                    result.Item.SetProviderId("TheSportsDB", raceEvent.Id);
                    _logger.LogDebug("Set provider ID: TheSportsDB={EventId}", raceEvent.Id);
                }

                if (!string.IsNullOrEmpty(raceEvent.Date) && DateTime.TryParse(raceEvent.Date, out var eventDate))
                {
                    result.Item.PremiereDate = eventDate;
                    _logger.LogDebug("Set premiere date: {Date}", eventDate);
                }

                if (!string.IsNullOrEmpty(raceEvent.Round) && int.TryParse(raceEvent.Round, out var round))
                {
                    result.Item.IndexNumber = round;
                    _logger.LogDebug("Set round number (IndexNumber): {Round}", round);
                }

                if (!string.IsNullOrEmpty(raceEvent.Season) && int.TryParse(raceEvent.Season, out var season))
                {
                    result.Item.ParentIndexNumber = season;
                    _logger.LogDebug("Set season (ParentIndexNumber): {Season}", season);
                }

                result.HasMetadata = true;
                _logger.LogInformation("Successfully retrieved metadata for event: {EventName}", raceEvent.Name);
            }
            else
            {
                _logger.LogWarning(
                    "Could not find event for episode: Name={Name}, IndexNumber={IndexNumber}",
                    info.Name,
                    info.IndexNumber);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching event metadata for {Name}", info.Name);
        }

        return result;
    }

    /// <inheritdoc />
    public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        return httpClient.GetAsync(url, cancellationToken);
    }

    /// <summary>
    /// Checks if the plugin is enabled.
    /// </summary>
    /// <returns>True if enabled, false otherwise.</returns>
    private bool IsEnabled()
    {
        return Plugin.Instance?.Configuration?.EnablePlugin ?? false;
    }

    /// <summary>
    /// Extracts the round number from episode name using regex patterns.
    /// </summary>
    /// <param name="name">The episode name (e.g., "Round 22 - Las Vegas Grand Prix").</param>
    /// <returns>The round number if found, null otherwise.</returns>
    private int? ExtractRoundNumber(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        var match = _roundNumberRegex.Match(name);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var roundNumber))
        {
            _logger.LogDebug("Extracted round number from name '{Name}': {RoundNumber}", name, roundNumber);
            return roundNumber;
        }

        _logger.LogDebug("Could not extract round number from name '{Name}'", name);
        return null;
    }

    /// <summary>
    /// Extracts the event name from the filename by removing round number prefix.
    /// </summary>
    /// <param name="name">The episode name (e.g., "Round 22 - Las Vegas Grand Prix").</param>
    /// <returns>The event name (e.g., "Las Vegas Grand Prix").</returns>
    private string? ExtractEventName(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        // Remove "Round XX - " or similar prefixes
        var cleanName = _roundNumberRegex.Replace(name, string.Empty)
            .Trim('-', ' ', '_');

        if (string.IsNullOrWhiteSpace(cleanName))
        {
            return name; // Return original if cleaning resulted in empty string
        }

        _logger.LogDebug("Extracted event name from '{OriginalName}': '{EventName}'", name, cleanName);
        return cleanName;
    }

    [GeneratedRegex(@"(?:Round|R|Episode|Ep\.?)\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-NO")]
    private static partial Regex MyRegex();
}
