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
/// Provides images for sports series (leagues).
/// </summary>
public class TsdbSeriesImageProvider : IRemoteImageProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PrefixedLogger<TsdbSeriesImageProvider> _logger;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TsdbSeriesImageProvider"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public TsdbSeriesImageProvider(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
    {
        _httpClientFactory = httpClientFactory;
        _loggerFactory = loggerFactory;
        var baseLogger = loggerFactory.CreateLogger<TsdbSeriesImageProvider>();
        _logger = new PrefixedLogger<TsdbSeriesImageProvider>(baseLogger);
    }

    /// <inheritdoc />
    public string Name => TsdbPlugin.ProviderName;

    /// <inheritdoc />
    public bool Supports(BaseItem item)
    {
        return item is Series;
    }

    /// <inheritdoc />
    public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
    {
        yield return ImageType.Primary;
        yield return ImageType.Logo;
        yield return ImageType.Banner;
        yield return ImageType.Backdrop;
        yield return ImageType.Art;
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

            if (item is Series series)
            {
                var leagueId = series.GetProviderId("TheSportsDB");
                _logger.LogDebug("Series provider ID: TheSportsDB={LeagueId}", leagueId);

                if (!string.IsNullOrEmpty(leagueId))
                {
                    _logger.LogInformation("Fetching images for league {LeagueId}", leagueId);

                    var league = await client.GetLeagueByIdAsync(leagueId, cancellationToken).ConfigureAwait(false);

                    if (league != null)
                    {
                        _logger.LogDebug("Adding images for league: {LeagueName}", league.Name);
                        AddLeagueImages(league, images);
                        _logger.LogInformation("Added {Count} images for series '{SeriesName}'", images.Count, series.Name);
                    }
                    else
                    {
                        _logger.LogWarning("No league found with ID: {LeagueId}", leagueId);
                    }
                }
                else
                {
                    _logger.LogWarning("Series '{Name}' has no TheSportsDB provider ID set", series.Name);
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
    /// Adds league images to the collection.
    /// </summary>
    /// <param name="league">The league.</param>
    /// <param name="images">The images collection.</param>
    private void AddLeagueImages(API.Models.League league, List<RemoteImageInfo> images)
    {
        if (!string.IsNullOrEmpty(league.Badge))
        {
            images.Add(new RemoteImageInfo
            {
                Url = league.Badge,
                Type = ImageType.Primary,
                ProviderName = Name,
            });
        }

        if (!string.IsNullOrEmpty(league.Logo))
        {
            images.Add(new RemoteImageInfo
            {
                Url = league.Logo,
                Type = ImageType.Logo,
                ProviderName = Name,
            });
        }

        if (!string.IsNullOrEmpty(league.Banner))
        {
            images.Add(new RemoteImageInfo
            {
                Url = league.Banner,
                Type = ImageType.Banner,
                ProviderName = Name,
            });
        }

        if (!string.IsNullOrEmpty(league.Poster))
        {
            images.Add(new RemoteImageInfo
            {
                Url = league.Poster,
                Type = ImageType.Primary,
                ProviderName = Name,
            });
        }
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
