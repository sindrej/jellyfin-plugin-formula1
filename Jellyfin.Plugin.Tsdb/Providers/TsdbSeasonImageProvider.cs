namespace Jellyfin.Plugin.Tsdb.Providers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Tsdb.API;
using Jellyfin.Plugin.Tsdb.Logger;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides images for sports seasons.
/// </summary>
public class TsdbSeasonImageProvider : IRemoteImageProvider
{
    private static readonly ImageType[] _seasonImageTypes = [ImageType.Primary, ImageType.Banner, ImageType.Backdrop];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PrefixedLogger<TsdbSeasonImageProvider> _logger;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TsdbSeasonImageProvider"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public TsdbSeasonImageProvider(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
    {
        _httpClientFactory = httpClientFactory;
        _loggerFactory = loggerFactory;
        var baseLogger = loggerFactory.CreateLogger<TsdbSeasonImageProvider>();
        _logger = new PrefixedLogger<TsdbSeasonImageProvider>(baseLogger);
    }

    /// <inheritdoc />
    public string Name => "TheSportsDB";

    /// <inheritdoc />
    public bool Supports(BaseItem item)
    {
        return item is Season;
    }

    /// <inheritdoc />
    public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
    {
        return item is Season ? _seasonImageTypes : [];
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
    {
        if (!IsEnabled())
        {
            _logger.LogDebug("TheSportsDB plugin is disabled");
            return Enumerable.Empty<RemoteImageInfo>();
        }

        _logger.LogInformation("Getting images for item: Name={Name}, Type={Type}", item.Name, item.GetType().Name);

        var images = new List<RemoteImageInfo>();

        try
        {
            var client = new TheSportsDBClient(_httpClientFactory, _loggerFactory.CreateLogger<TheSportsDBClient>());

            if (item is Season season)
            {
                // Get league ID from parent series
                var leagueId = season.Series?.GetProviderId("TheSportsDB");
                _logger.LogDebug("Season's series provider ID: TheSportsDB={LeagueId}", leagueId);

                if (!string.IsNullOrEmpty(leagueId))
                {
                    _logger.LogInformation("Fetching images for season {SeasonName} of league {LeagueId}", season.Name, leagueId);

                    var seasonYear = season.IndexNumber;

                    if (!seasonYear.HasValue)
                    {
                        _logger.LogWarning("Season '{SeasonName}' has no IndexNumber set, cannot fetch season-specific images", season.Name);
                    }
                    else
                    {
                        var seasons = await client.GetLeagueSeasonsAsync(leagueId, cancellationToken).ConfigureAwait(false);
                        var apiSeason = seasons.FirstOrDefault(s => s.Name == seasonYear.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));

                        if (apiSeason != null && !string.IsNullOrEmpty(apiSeason.Poster))
                        {
                            _logger.LogDebug("Adding season poster for {SeasonYear}", seasonYear);
                            images.Add(new RemoteImageInfo
                            {
                                Url = apiSeason.Poster,
                                Type = ImageType.Primary,
                                ProviderName = Name
                            });
                            _logger.LogInformation("Added season poster for '{SeasonName}'", season.Name);
                        }
                        else
                        {
                            _logger.LogWarning("No season poster available for league {LeagueId}, year {Year}", leagueId, seasonYear);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Season '{Name}' has no parent series with TheSportsDB provider ID set", season.Name);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching images for {ItemName}", item.Name);
        }

        _logger.LogInformation("Returning {Count} images for '{ItemName}'", images.Count, item.Name);
        return images;
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
