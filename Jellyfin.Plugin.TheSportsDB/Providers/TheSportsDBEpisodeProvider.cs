using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.TheSportsDB.API;
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
public class TheSportsDBEpisodeProvider : IRemoteMetadataProvider<Episode, EpisodeInfo>
{
    private static readonly System.Text.RegularExpressions.Regex RoundNumberRegex = new System.Text.RegularExpressions.Regex(
        @"(?:Round|R|Episode|Ep\.?)\s*(\d+)",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TheSportsDBEpisodeProvider> _logger;
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
        _logger = loggerFactory.CreateLogger<TheSportsDBEpisodeProvider>();
    }

    /// <inheritdoc />
    public string Name => "TheSportsDB";

    /// <summary>
    /// Parses the first year from a season string.
    /// Examples: "2023-2024" → 2023, "2024" → 2024.
    /// </summary>
    /// <param name="seasonStr">The season string to parse.</param>
    /// <returns>The parsed year if found, null otherwise.</returns>
    private int? ParseSeasonYear(string? seasonStr)
    {
        if (string.IsNullOrEmpty(seasonStr))
        {
            return null;
        }

        var separators = new[] { '-', '/', ' ' };
        var parts = seasonStr.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            if (int.TryParse(part.Trim(), out var year) && year >= 1900 && year <= 2100)
            {
                _logger.LogDebug("Parsed season year {Year} from '{SeasonStr}'", year, seasonStr);
                return year;
            }
        }

        _logger.LogWarning("Could not parse year from season string: {SeasonStr}", seasonStr);
        return null;
    }

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
                        Name = evt.StrEvent,
                        SearchProviderName = Name,
                        ImageUrl = evt.StrPoster ?? evt.StrThumb,
                        Overview = evt.StrDescriptionEN ?? evt.StrResult
                    };

                    if (!string.IsNullOrEmpty(evt.IdEvent))
                    {
                        result.SetProviderId("TheSportsDB", evt.IdEvent);
                    }

                    if (!string.IsNullOrEmpty(evt.DateEvent) && DateTime.TryParse(evt.DateEvent, out var eventDate))
                    {
                        result.PremiereDate = eventDate;
                    }

                    if (!string.IsNullOrEmpty(evt.IntRound) && int.TryParse(evt.IntRound, out var roundNumber))
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

                foreach (var evt in events.Where(e => e.StrSeason != null))
                {
                    var result = new RemoteSearchResult
                    {
                        Name = evt.StrEvent,
                        SearchProviderName = Name,
                        ImageUrl = evt.StrPoster ?? evt.StrThumb,
                        Overview = evt.StrDescriptionEN ?? evt.StrResult
                    };

                    if (!string.IsNullOrEmpty(evt.IdEvent))
                    {
                        result.SetProviderId("TheSportsDB", evt.IdEvent);
                    }

                    if (!string.IsNullOrEmpty(evt.DateEvent) && DateTime.TryParse(evt.DateEvent, out var eventDate))
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
                    var events = await client.GetEventsForSeasonAsync(leagueId, seasonYear.Value, cancellationToken).ConfigureAwait(false);
                    raceEvent = events.FirstOrDefault(e =>
                        !string.IsNullOrEmpty(e.IntRound) &&
                        int.TryParse(e.IntRound, out var round) &&
                        round == roundNumber.Value);

                    if (raceEvent != null)
                    {
                        _logger.LogInformation("Found event by round number: {EventName}", raceEvent.StrEvent);
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
                            e.StrEvent != null &&
                            e.StrEvent.Contains(eventName, StringComparison.OrdinalIgnoreCase));

                        if (raceEvent != null)
                        {
                            _logger.LogInformation("Found event by fuzzy name match: {EventName}", raceEvent.StrEvent);
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
                    raceEvent.StrEvent,
                    raceEvent.StrSeason,
                    raceEvent.IntRound);

                result.Item = new Episode
                {
                    Name = raceEvent.StrEvent,
                    Overview = raceEvent.StrDescriptionEN ?? raceEvent.StrResult,
                    CommunityRating = null
                };

                if (!string.IsNullOrEmpty(raceEvent.IdEvent))
                {
                    result.Item.SetProviderId("TheSportsDB", raceEvent.IdEvent);
                    _logger.LogDebug("Set provider ID: TheSportsDB={EventId}", raceEvent.IdEvent);
                }

                if (!string.IsNullOrEmpty(raceEvent.DateEvent) && DateTime.TryParse(raceEvent.DateEvent, out var eventDate))
                {
                    result.Item.PremiereDate = eventDate;
                    _logger.LogDebug("Set premiere date: {Date}", eventDate);
                }

                if (!string.IsNullOrEmpty(raceEvent.IntRound) && int.TryParse(raceEvent.IntRound, out var round))
                {
                    result.Item.IndexNumber = round;
                    _logger.LogDebug("Set round number (IndexNumber): {Round}", round);
                }

                if (!string.IsNullOrEmpty(raceEvent.StrSeason) && int.TryParse(raceEvent.StrSeason, out var season))
                {
                    result.Item.ParentIndexNumber = season;
                    _logger.LogDebug("Set season (ParentIndexNumber): {Season}", season);
                }

                result.HasMetadata = true;
                _logger.LogInformation("Successfully retrieved metadata for event: {EventName}", raceEvent.StrEvent);
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
    /// Extracts the season year from episode info.
    /// </summary>
    /// <param name="info">The episode info.</param>
    /// <returns>The season year if found, null otherwise.</returns>
    private int? ExtractSeasonYear(EpisodeInfo info)
    {
        // First try parent index number (season number = year)
        if (info.ParentIndexNumber.HasValue && info.ParentIndexNumber.Value >= 1900 && info.ParentIndexNumber.Value <= 2100)
        {
            _logger.LogDebug("Extracted season year from ParentIndexNumber: {Year}", info.ParentIndexNumber.Value);
            return info.ParentIndexNumber.Value;
        }

        // Try to extract year from episode name as fallback
        if (!string.IsNullOrEmpty(info.Name))
        {
            var parts = info.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (int.TryParse(part, out var year) && year >= 1900 && year <= 2100)
                {
                    _logger.LogDebug("Extracted season year from episode Name '{Name}': {Year}", info.Name, year);
                    return year;
                }
            }
        }

        _logger.LogDebug("Could not extract season year from episode info");
        return null;
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

        var match = RoundNumberRegex.Match(name);
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
        var cleanName = RoundNumberRegex.Replace(name, string.Empty)
            .Trim('-', ' ', '_');

        if (string.IsNullOrWhiteSpace(cleanName))
        {
            return name; // Return original if cleaning resulted in empty string
        }

        _logger.LogDebug("Extracted event name from '{OriginalName}': '{EventName}'", name, cleanName);
        return cleanName;
    }
}
