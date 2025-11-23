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
/// Provides images for Formula 1 content.
/// </summary>
public class TheSportsDBImageProvider : IRemoteImageProvider
{
    private static readonly ImageType[] EpisodeImageTypes = { ImageType.Primary, ImageType.Thumb, ImageType.Backdrop };
    private static readonly ImageType[] SeriesImageTypes = { ImageType.Primary, ImageType.Banner, ImageType.Backdrop };
    private static readonly char[] NameSeparators = { ' ', '-', '_' };

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
            return Enumerable.Empty<RemoteImageInfo>();
        }

        var images = new List<RemoteImageInfo>();

        try
        {
            var client = new TheSportsDBClient(_httpClientFactory, _loggerFactory.CreateLogger<TheSportsDBClient>());

            if (item is Episode episode)
            {
                var eventId = episode.GetProviderId("TheSportsDB");
                if (!string.IsNullOrEmpty(eventId))
                {
                    var raceEvent = await client.GetEventByIdAsync(eventId, cancellationToken).ConfigureAwait(false);
                    if (raceEvent != null)
                    {
                        AddEventImages(raceEvent, images);
                    }
                }
            }
            else if (item is Series series)
            {
                var seasonYear = ExtractYear(series.Name);
                if (!seasonYear.HasValue)
                {
                    var seasonId = series.GetProviderId("Formula1Season");
                    if (!string.IsNullOrEmpty(seasonId) && int.TryParse(seasonId, out var parsedYear))
                    {
                        seasonYear = parsedYear;
                    }
                }

                if (seasonYear.HasValue)
                {
                    // Get images from the first event of the season
                    var events = await client.GetEventsForSeasonAsync(seasonYear.Value, cancellationToken).ConfigureAwait(false);
                    var firstEvent = events.FirstOrDefault(e => !string.IsNullOrEmpty(e.StrPoster) || !string.IsNullOrEmpty(e.StrBanner));

                    if (firstEvent != null)
                    {
                        AddEventImages(firstEvent, images);
                    }

                    // Also get F1 team images for additional artwork
                    var teams = await client.GetFormulaOneTeamsAsync(cancellationToken).ConfigureAwait(false);
                    foreach (var team in teams.Take(3)) // Limit to top 3 teams for performance
                    {
                        AddTeamImages(team, images);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching images for {ItemName}", item.Name);
        }

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
    /// Adds team images to the collection.
    /// </summary>
    /// <param name="team">The team.</param>
    /// <param name="images">The images collection.</param>
    private void AddTeamImages(API.Models.Team team, List<RemoteImageInfo> images)
    {
        if (!string.IsNullOrEmpty(team.StrBadge))
        {
            images.Add(new RemoteImageInfo
            {
                Url = team.StrBadge,
                Type = ImageType.Primary,
                ProviderName = Name
            });
        }

        if (!string.IsNullOrEmpty(team.StrBanner))
        {
            images.Add(new RemoteImageInfo
            {
                Url = team.StrBanner,
                Type = ImageType.Banner,
                ProviderName = Name
            });
        }

        // Add fanart images
        AddFanartImage(team.StrFanart1, images);
        AddFanartImage(team.StrFanart2, images);
        AddFanartImage(team.StrFanart3, images);
        AddFanartImage(team.StrFanart4, images);
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

    /// <summary>
    /// Extracts a year from a string.
    /// </summary>
    /// <param name="name">The string to extract from.</param>
    /// <returns>The year if found, null otherwise.</returns>
    private int? ExtractYear(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        var parts = name.Split(NameSeparators, StringSplitOptions.RemoveEmptyEntries);
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
