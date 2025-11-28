namespace Jellyfin.Plugin.Tsdb.Providers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Tsdb.API;
using Jellyfin.Plugin.Tsdb.Logger;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides sports season metadata.
/// Maps: Season Year → TV Season (e.g., "2024" becomes a season).
/// </summary>
public class TsdbSeasonProvider : IRemoteMetadataProvider<Season, SeasonInfo>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PrefixedLogger<TsdbSeasonProvider> _logger;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TsdbSeasonProvider"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public TsdbSeasonProvider(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
    {
        _httpClientFactory = httpClientFactory;
        _loggerFactory = loggerFactory;
        var baseLogger = loggerFactory.CreateLogger<TsdbSeasonProvider>();
        _logger = new PrefixedLogger<TsdbSeasonProvider>(baseLogger);
    }

    /// <inheritdoc />
    public string Name => "TheSportsDB";

    /// <inheritdoc />
    public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeasonInfo searchInfo, CancellationToken cancellationToken)
    {
        // Season search is not really useful for sports - seasons are typically just year numbers
        return Task.FromResult(Enumerable.Empty<RemoteSearchResult>());
    }

    /// <inheritdoc />
    public async Task<MetadataResult<Season>> GetMetadata(SeasonInfo info, CancellationToken cancellationToken)
    {
        var result = new MetadataResult<Season>();

        if (!IsEnabled())
        {
            _logger.LogDebug("TheSportsDB plugin is disabled");
            return result;
        }

        _logger.LogInformation(
            "Getting metadata for season: Name={Name}, IndexNumber={IndexNumber}, Path={Path}",
            info.Name,
            info.IndexNumber,
            info.Path);

        try
        {
            var client = new TheSportsDBClient(_httpClientFactory, _loggerFactory.CreateLogger<TheSportsDBClient>());

            // Get league ID from series provider IDs
            var leagueId = info.SeriesProviderIds?.GetValueOrDefault("TheSportsDB");
            var seasonYear = info.IndexNumber;

            if (!string.IsNullOrEmpty(leagueId) && seasonYear.HasValue)
            {
                _logger.LogDebug("Fetching season data for league {LeagueId}, year {Year}", leagueId, seasonYear);

                // Get season data from TheSportsDB
                var seasons = await client.GetLeagueSeasonsAsync(leagueId, cancellationToken).ConfigureAwait(false);
                var apiSeason = seasons.FirstOrDefault(s => s.Name == seasonYear.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));

                if (apiSeason != null)
                {
                    result.Item = new Season
                    {
                        Name = apiSeason.Name,
                        IndexNumber = seasonYear,
                        Overview = $"Season {apiSeason.Name}"
                    };

                    result.HasMetadata = true;
                    _logger.LogInformation("Successfully retrieved metadata for season {Season}", apiSeason.Name);
                }
                else
                {
                    _logger.LogWarning("No season data found for league {LeagueId}, year {Year}", leagueId, seasonYear);
                }
            }
            else
            {
                _logger.LogWarning("Missing league ID or season number for season metadata lookup");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching season metadata for {Name}", info.Name);
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
        return TsdbPlugin.Instance?.Configuration?.EnablePlugin ?? false;
    }
}
