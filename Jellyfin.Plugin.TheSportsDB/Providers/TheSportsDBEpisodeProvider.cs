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
/// Provides Formula 1 race metadata for episodes.
/// </summary>
public class TheSportsDBEpisodeProvider : IRemoteMetadataProvider<Episode, EpisodeInfo>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TheSportsDBEpisodeProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TheSportsDBEpisodeProvider"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger.</param>
    public TheSportsDBEpisodeProvider(IHttpClientFactory httpClientFactory, ILogger<TheSportsDBEpisodeProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
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
            var client = new TheSportsDBClient(_httpClientFactory, _logger);

            // Try to extract season year from series name or parent index
            var seasonYear = ExtractSeasonYear(searchInfo);
            if (seasonYear.HasValue)
            {
                var events = await client.GetEventsForSeasonAsync(seasonYear.Value, cancellationToken).ConfigureAwait(false);

                foreach (var evt in events.Where(e => e.StrEvent != null && e.StrEvent.Contains("Grand Prix", StringComparison.OrdinalIgnoreCase)))
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for F1 race metadata");
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<MetadataResult<Episode>> GetMetadata(EpisodeInfo info, CancellationToken cancellationToken)
    {
        var result = new MetadataResult<Episode>();

        if (!IsEnabled())
        {
            return result;
        }

        try
        {
            var client = new TheSportsDBClient(_httpClientFactory, _logger);
            var eventId = info.GetProviderId("TheSportsDB");

            API.Models.Event? raceEvent = null;

            if (!string.IsNullOrEmpty(eventId))
            {
                // Fetch by ID if we have it
                raceEvent = await client.GetEventByIdAsync(eventId, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Try to find the event by season and episode number
                var seasonYear = ExtractSeasonYear(info);
                if (seasonYear.HasValue && info.IndexNumber.HasValue)
                {
                    var events = await client.GetEventsForSeasonAsync(seasonYear.Value, cancellationToken).ConfigureAwait(false);
                    raceEvent = events.FirstOrDefault(e =>
                        !string.IsNullOrEmpty(e.IntRound) &&
                        int.TryParse(e.IntRound, out var round) &&
                        round == info.IndexNumber.Value &&
                        e.StrEvent != null &&
                        e.StrEvent.Contains("Grand Prix", StringComparison.OrdinalIgnoreCase));
                }
            }

            if (raceEvent != null)
            {
                result.Item = new Episode
                {
                    Name = raceEvent.StrEvent,
                    Overview = raceEvent.StrDescriptionEN ?? raceEvent.StrResult,
                    CommunityRating = null
                };

                if (!string.IsNullOrEmpty(raceEvent.IdEvent))
                {
                    result.Item.SetProviderId("TheSportsDB", raceEvent.IdEvent);
                }

                if (!string.IsNullOrEmpty(raceEvent.DateEvent) && DateTime.TryParse(raceEvent.DateEvent, out var eventDate))
                {
                    result.Item.PremiereDate = eventDate;
                }

                if (!string.IsNullOrEmpty(raceEvent.IntRound) && int.TryParse(raceEvent.IntRound, out var roundNumber))
                {
                    result.Item.IndexNumber = roundNumber;
                }

                if (!string.IsNullOrEmpty(raceEvent.StrSeason) && int.TryParse(raceEvent.StrSeason, out var season))
                {
                    result.Item.ParentIndexNumber = season;
                }

                result.HasMetadata = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching F1 race metadata for {Name}", info.Name);
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
        // First try parent index number (season number)
        if (info.ParentIndexNumber.HasValue && info.ParentIndexNumber.Value >= 1950 && info.ParentIndexNumber.Value <= 2100)
        {
            return info.ParentIndexNumber.Value;
        }

        // Try to extract year from series name (e.g., "Formula 1 2024")
        if (!string.IsNullOrEmpty(info.SeriesName))
        {
            var parts = info.SeriesName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (int.TryParse(part, out var year) && year >= 1950 && year <= 2100)
                {
                    return year;
                }
            }
        }

        return null;
    }
}
