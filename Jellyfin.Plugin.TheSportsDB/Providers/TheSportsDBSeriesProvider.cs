using System;
using System.Collections.Generic;
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
/// Provides Formula 1 season metadata for series.
/// </summary>
public class TheSportsDBSeriesProvider : IRemoteMetadataProvider<Series, SeriesInfo>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TheSportsDBSeriesProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TheSportsDBSeriesProvider"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger.</param>
    public TheSportsDBSeriesProvider(IHttpClientFactory httpClientFactory, ILogger<TheSportsDBSeriesProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => "TheSportsDB";

    /// <inheritdoc />
    public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo, CancellationToken cancellationToken)
    {
        if (!IsEnabled())
        {
            return Enumerable.Empty<RemoteSearchResult>();
        }

        var results = new List<RemoteSearchResult>();

        try
        {
            // Try to extract year from series name (e.g., "Formula 1 2024" or "F1 2024")
            var year = ExtractYear(searchInfo.Name);
            if (year.HasValue)
            {
                var result = new RemoteSearchResult
                {
                    Name = $"Formula 1 {year.Value}",
                    SearchProviderName = Name,
                    ProductionYear = year.Value
                };

                result.SetProviderId("TheSportsDB", $"formula1_{year.Value}");
                result.SetProviderId("Formula1Season", year.Value.ToString());

                results.Add(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for F1 season metadata");
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
    {
        var result = new MetadataResult<Series>();

        if (!IsEnabled())
        {
            return result;
        }

        try
        {
            var seasonYear = ExtractYear(info.Name);
            if (!seasonYear.HasValue)
            {
                // Try to get from provider ID
                var seasonId = info.GetProviderId("Formula1Season");
                if (!string.IsNullOrEmpty(seasonId) && int.TryParse(seasonId, out var parsedYear))
                {
                    seasonYear = parsedYear;
                }
            }

            if (seasonYear.HasValue)
            {
                var client = new TheSportsDBClient(_httpClientFactory, _logger);

                // Fetch one event from the season to verify it exists and get some metadata
                var events = await client.GetEventsForSeasonAsync(seasonYear.Value, cancellationToken).ConfigureAwait(false);
                var firstEvent = events.FirstOrDefault(e => e.StrEvent != null && e.StrEvent.Contains("Grand Prix", StringComparison.OrdinalIgnoreCase));

                result.Item = new Series
                {
                    Name = $"Formula 1 {seasonYear.Value}",
                    ProductionYear = seasonYear.Value,
                    Overview = $"Formula 1 World Championship {seasonYear.Value} season with {events.Count(e => e.StrEvent != null && e.StrEvent.Contains("Grand Prix", StringComparison.OrdinalIgnoreCase))} races."
                };

                result.Item.SetProviderId("TheSportsDB", $"formula1_{seasonYear.Value}");
                result.Item.SetProviderId("Formula1Season", seasonYear.Value.ToString());

                // Add F1 as a genre/tag
                result.Item.Genres = new[] { "Formula 1", "Motorsport", "Racing" };

                if (firstEvent != null && !string.IsNullOrEmpty(firstEvent.DateEvent) && DateTime.TryParse(firstEvent.DateEvent, out var firstRaceDate))
                {
                    result.Item.PremiereDate = firstRaceDate;
                }

                result.HasMetadata = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching F1 season metadata for {Name}", info.Name);
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
    /// Extracts a year from a string (e.g., "Formula 1 2024" returns 2024).
    /// </summary>
    /// <param name="name">The string to extract from.</param>
    /// <returns>The year if found, null otherwise.</returns>
    private int? ExtractYear(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        var parts = name.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            if (int.TryParse(part, out var year) && year >= 1950 && year <= 2100)
            {
                return year;
            }
        }

        return null;
    }
}
