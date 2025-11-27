using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.TheSportsDB.API;
using MediaBrowser.Common.Net;
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
    private static readonly ImageType[] EpisodeImageTypes = { ImageType.Primary, ImageType.Thumb, ImageType.Backdrop };
    private static readonly ImageType[] SeriesImageTypes = { ImageType.Primary, ImageType.Logo, ImageType.Banner, ImageType.Backdrop };

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
        _logger = loggerFactory.CreateLogger<TheSportsDBImageProvider>();
    }

    /// <inheritdoc />
    public string Name => "TheSportsDB";

    /// <inheritdoc />
    public bool Supports(BaseItem item)
    {
        return item is Episode || item is Series;
    }

    /// <inheritdoc />
    public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
    {
        if (item is Episode)
        {
            return EpisodeImageTypes;
        }

        if (item is Series)
        {
            return SeriesImageTypes;
        }

        return Array.Empty<ImageType>();
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
                        _logger.LogDebug("Adding images for league: {LeagueName}", league.StrLeague);
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
        if (!string.IsNullOrEmpty(raceEvent.StrPoster))
        {
            images.Add(new RemoteImageInfo
            {
                Url = raceEvent.StrPoster,
                Type = ImageType.Primary,
                ProviderName = Name
            });
        }

        if (!string.IsNullOrEmpty(raceEvent.StrThumb))
        {
            images.Add(new RemoteImageInfo
            {
                Url = raceEvent.StrThumb,
                Type = ImageType.Thumb,
                ProviderName = Name
            });
        }

        if (!string.IsNullOrEmpty(raceEvent.StrBanner))
        {
            images.Add(new RemoteImageInfo
            {
                Url = raceEvent.StrBanner,
                Type = ImageType.Banner,
                ProviderName = Name
            });
        }

        if (!string.IsNullOrEmpty(raceEvent.StrFanart))
        {
            images.Add(new RemoteImageInfo
            {
                Url = raceEvent.StrFanart,
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
        if (!string.IsNullOrEmpty(league.StrBadge))
        {
            images.Add(new RemoteImageInfo
            {
                Url = league.StrBadge,
                Type = ImageType.Primary,
                ProviderName = Name
            });
        }

        if (!string.IsNullOrEmpty(league.StrLogo))
        {
            images.Add(new RemoteImageInfo
            {
                Url = league.StrLogo,
                Type = ImageType.Logo,
                ProviderName = Name
            });
        }

        if (!string.IsNullOrEmpty(league.StrBanner))
        {
            images.Add(new RemoteImageInfo
            {
                Url = league.StrBanner,
                Type = ImageType.Banner,
                ProviderName = Name
            });
        }

        if (!string.IsNullOrEmpty(league.StrPoster))
        {
            images.Add(new RemoteImageInfo
            {
                Url = league.StrPoster,
                Type = ImageType.Primary,
                ProviderName = Name
            });
        }

        // Add fanart images
        AddFanartImage(league.StrFanart1, images);
        AddFanartImage(league.StrFanart2, images);
        AddFanartImage(league.StrFanart3, images);
        AddFanartImage(league.StrFanart4, images);
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
