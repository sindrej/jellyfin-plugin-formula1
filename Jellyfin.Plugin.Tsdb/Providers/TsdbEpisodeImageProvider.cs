namespace Jellyfin.Plugin.Tsdb.Providers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Tsdb.API;
using Jellyfin.Plugin.Tsdb.API.Models;
using Jellyfin.Plugin.Tsdb.Logger;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides images for sports episodes (events).
/// </summary>
public class TsdbEpisodeImageProvider : IRemoteImageProvider
{
    private static readonly ImageType[] _episodeImageTypes = [ImageType.Primary, ImageType.Thumb, ImageType.Backdrop];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PrefixedLogger<TsdbEpisodeImageProvider> _logger;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TsdbEpisodeImageProvider"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public TsdbEpisodeImageProvider(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
    {
        _httpClientFactory = httpClientFactory;
        _loggerFactory = loggerFactory;
        var baseLogger = loggerFactory.CreateLogger<TsdbEpisodeImageProvider>();
        _logger = new PrefixedLogger<TsdbEpisodeImageProvider>(baseLogger);
    }

    /// <inheritdoc />
    public string Name => "TheSportsDB";

    /// <inheritdoc />
    public bool Supports(BaseItem item)
    {
        return item is Episode;
    }

    /// <inheritdoc />
    public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
    {
        return item is Episode ? _episodeImageTypes : [];
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
    /// <param name="round">The race event.</param>
    /// <param name="images">The images collection.</param>
    private void AddEventImages(Round round, List<RemoteImageInfo> images)
    {
        if (!string.IsNullOrEmpty(round.Poster))
        {
            images.Add(new RemoteImageInfo
            {
                Url = round.Poster,
                Type = ImageType.Primary,
                ProviderName = Name
            });
        }

        if (!string.IsNullOrEmpty(round.Thumb))
        {
            images.Add(new RemoteImageInfo
            {
                Url = round.Thumb,
                Type = ImageType.Thumb,
                ProviderName = Name
            });
        }

        if (!string.IsNullOrEmpty(round.Banner))
        {
            images.Add(new RemoteImageInfo
            {
                Url = round.Banner,
                Type = ImageType.Banner,
                ProviderName = Name
            });
        }

        if (!string.IsNullOrEmpty(round.Fanart))
        {
            images.Add(new RemoteImageInfo
            {
                Url = round.Fanart,
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
        return TsdbPlugin.Instance?.Configuration?.EnablePlugin ?? false;
    }
}
