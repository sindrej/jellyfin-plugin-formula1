using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.TheSportsDB.API;
using Jellyfin.Plugin.TheSportsDB.Logger;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TheSportsDB.Providers;

/// <summary>
/// Provides images for sports content (leagues and events).
/// </summary>
public class TheSportsDBImageProvider : IRemoteImageProvider
{
    private static readonly ImageType[] _episodeImageTypes = [ImageType.Primary, ImageType.Thumb, ImageType.Backdrop];
    private static readonly ImageType[] _seriesImageTypes = [ImageType.Primary, ImageType.Logo, ImageType.Banner, ImageType.Backdrop];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TheSportsDBImageProvider> _logger;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TheSportsDBImageProvider"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public TheSportsDBImageProvider(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
    {
        _httpClientFactory = httpClientFactory;
        _loggerFactory = loggerFactory;
        var baseLogger = loggerFactory.CreateLogger<TheSportsDBImageProvider>();
        _logger = new PrefixedLogger<TheSportsDBImageProvider>(baseLogger);
    }

    /// <inheritdoc />
    public string Name => "TheSportsDB";

    /// <inheritdoc />
    public bool Supports(BaseItem item)
    {
        return item is Episode or Series;
    }

    /// <inheritdoc />
    public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
    {
        return item switch
        {
            Episode => _episodeImageTypes,
            Series => _seriesImageTypes,
            _ => []
        };
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

            if (item is Episode episode)
            {
                var eventId = episode.GetProviderId("TheSportsDB");
                _logger.LogDebug("Episode provider ID: TheSportsDB={EventId}", eventId);

                if (!string.IsNullOrEmpty(eventId))
                {
                    _logger.LogDebug("Fetching images for episode event ID: {EventId}", eventId);
                    var raceEvent = await client.GetEventByIdAsync(eventId, cancellationToken).ConfigureAwait(false);
                    if (raceEvent != null)
                    {
                        AddEventImages(raceEvent, images);
                        _logger.LogInformation("Added {Count} images for episode '{Name}'", images.Count, episode.Name);
                    }
                    else
                    {
                        _logger.LogWarning("No event found for episode provider ID: {EventId}", eventId);
                    }
                }
                else
                {
                    _logger.LogWarning("Episode '{Name}' has no TheSportsDB provider ID set", episode.Name);
                }
            }
            else if (item is Series series)
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
    /// Adds event images to the collection.
    /// </summary>
    /// <param name="raceEvent">The race event.</param>
    /// <param name="images">The images collection.</param>
    private void AddEventImages(API.Models.Event raceEvent, List<RemoteImageInfo> images)
    {
        if (!string.IsNullOrEmpty(raceEvent.Poster))
        {
            images.Add(new RemoteImageInfo
            {
                Url = raceEvent.Poster,
                Type = ImageType.Primary,
                ProviderName = Name
            });
        }

        if (!string.IsNullOrEmpty(raceEvent.Thumb))
        {
            images.Add(new RemoteImageInfo
            {
                Url = raceEvent.Thumb,
                Type = ImageType.Thumb,
                ProviderName = Name
            });
        }

        if (!string.IsNullOrEmpty(raceEvent.Banner))
        {
            images.Add(new RemoteImageInfo
            {
                Url = raceEvent.Banner,
                Type = ImageType.Banner,
                ProviderName = Name
            });
        }

        if (!string.IsNullOrEmpty(raceEvent.Fanart))
        {
            images.Add(new RemoteImageInfo
            {
                Url = raceEvent.Fanart,
                Type = ImageType.Backdrop,
                ProviderName = Name
            });
        }
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
                ProviderName = Name
            });
        }

        if (!string.IsNullOrEmpty(league.Logo))
        {
            images.Add(new RemoteImageInfo
            {
                Url = league.Logo,
                Type = ImageType.Logo,
                ProviderName = Name
            });
        }

        if (!string.IsNullOrEmpty(league.Banner))
        {
            images.Add(new RemoteImageInfo
            {
                Url = league.Banner,
                Type = ImageType.Banner,
                ProviderName = Name
            });
        }

        if (!string.IsNullOrEmpty(league.Poster))
        {
            images.Add(new RemoteImageInfo
            {
                Url = league.Poster,
                Type = ImageType.Primary,
                ProviderName = Name
            });
        }

        // Add fanart images
        AddFanartImage(league.Fanart1, images);
        AddFanartImage(league.Fanart2, images);
        AddFanartImage(league.Fanart3, images);
        AddFanartImage(league.Fanart4, images);
    }

    /// <summary>
    /// Adds a fanart image to the collection if the URL is not empty.
    /// </summary>
    /// <param name="fanartUrl">The fanart URL.</param>
    /// <param name="images">The images collection.</param>
    private void AddFanartImage(string? fanartUrl, List<RemoteImageInfo> images)
    {
        if (!string.IsNullOrEmpty(fanartUrl))
        {
            images.Add(new RemoteImageInfo
            {
                Url = fanartUrl,
                Type = ImageType.Backdrop,
                ProviderName = Name
            });
        }
    }

    /// <summary>
    /// Checks if the plugin is enabled.
    /// </summary>
    /// <returns>True if enabled, false otherwise.</returns>
    private bool IsEnabled()
    {
        return Plugin.Instance?.Configuration?.EnablePlugin ?? false;
    }
}
