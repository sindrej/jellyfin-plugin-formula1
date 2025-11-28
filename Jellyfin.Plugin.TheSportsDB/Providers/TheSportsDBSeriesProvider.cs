using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.TheSportsDB.API;
using Jellyfin.Plugin.TheSportsDB.Logger;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TheSportsDB.Providers;

/// <summary>
/// Provides sports league metadata for series.
/// Maps: League → TV Show (e.g., "Formula 1" becomes a TV show).
/// </summary>
public class TheSportsDBSeriesProvider : IRemoteMetadataProvider<Series, SeriesInfo>
{
    private static readonly char[] _nameSeparators = [' ', '-', '_', ','];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TheSportsDBSeriesProvider> _logger;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TheSportsDBSeriesProvider"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public TheSportsDBSeriesProvider(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
    {
        _httpClientFactory = httpClientFactory;
        _loggerFactory = loggerFactory;
        var baseLogger = loggerFactory.CreateLogger<TheSportsDBSeriesProvider>();
        _logger = new PrefixedLogger<TheSportsDBSeriesProvider>(baseLogger);
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
            var client = new TheSportsDBClient(_httpClientFactory, _loggerFactory.CreateLogger<TheSportsDBClient>());

            _logger.LogInformation("Searching for leagues matching: {Name}", searchInfo.Name);
            var allLeagues = await client.GetAllLeaguesAsync(cancellationToken).ConfigureAwait(false);

            foreach (var league in allLeagues)
            {
                if (!IsLeagueMatch(searchInfo.Name, league))
                {
                    continue;
                }

                var result = new RemoteSearchResult
                {
                    Name = league.Name,
                    SearchProviderName = Name,
                    Overview = league.DescriptionEng,
                    ImageUrl = league.Badge ?? league.Logo ?? league.Poster
                };

                if (!string.IsNullOrEmpty(league.Id))
                {
                    result.SetProviderId("TheSportsDB", league.Id);
                }

                results.Add(result);

                _logger.LogDebug("Found matching league: {LeagueName} (ID: {LeagueId})", league.Name, league.Id);
            }

            _logger.LogInformation("Found {Count} matching leagues for '{Name}'", results.Count, searchInfo.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for league metadata");
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
    {
        var result = new MetadataResult<Series>();

        if (!IsEnabled())
        {
            _logger.LogDebug("TheSportsDB plugin is disabled");
            return result;
        }

        _logger.LogInformation(
            "Getting metadata for series: Name={Name}, Year={Year}, Path={Path}",
            info.Name,
            info.Year,
            info.Path);

        try
        {
            var client = new TheSportsDBClient(_httpClientFactory, _loggerFactory.CreateLogger<TheSportsDBClient>());

            var leagueId = info.GetProviderId("TheSportsDB");
            API.Models.League? league = null;

            if (!string.IsNullOrEmpty(leagueId))
            {
                _logger.LogDebug("Fetching league by provider ID: {LeagueId}", leagueId);
                league = await client.GetLeagueByIdAsync(leagueId, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                _logger.LogDebug("Searching for league by name: {Name}", info.Name);
                var allLeagues = await client.GetAllLeaguesAsync(cancellationToken).ConfigureAwait(false);
                league = allLeagues.FirstOrDefault(l => IsLeagueMatch(info.Name, l));

                if (league != null)
                {
                    _logger.LogInformation("Found league: {LeagueName} (ID: {LeagueId})", league.Name, league.Id);
                }
                else
                {
                    _logger.LogWarning("No matching league found for: {Name}", info.Name);
                    return result;
                }
            }

            if (league != null && !string.IsNullOrEmpty(league.Id))
            {
                result.Item = new Series
                {
                    Name = league.Name,
                    Overview = league.DescriptionEng
                };

                result.Item.SetProviderId("TheSportsDB", league.Id);

                // Set genres based on sport type
                if (!string.IsNullOrEmpty(league.Sport))
                {
                    result.Item.Genres = new[] { league.Sport, "Sports" };
                }
                else
                {
                    result.Item.Genres = new[] { "Sports" };
                }

                result.HasMetadata = true;
                _logger.LogInformation("Successfully retrieved metadata for league: {LeagueName}", league.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching league metadata for {Name}", info.Name);
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
    /// Checks if a search name matches a league.
    /// </summary>
    /// <param name="searchName">The search name from the user.</param>
    /// <param name="league">The league to match against.</param>
    /// <returns>True if the names match, false otherwise.</returns>
    private bool IsLeagueMatch(string? searchName, API.Models.League league)
    {
        if (string.IsNullOrEmpty(searchName) || string.IsNullOrEmpty(league.Name))
        {
            return false;
        }

        var normalizedSearch = searchName.ToLowerInvariant().Trim();
        var normalizedLeague = league.Name.ToLowerInvariant().Trim();

        // Exact match
        if (normalizedSearch == normalizedLeague)
        {
            return true;
        }

        // Check alternate names
        if (!string.IsNullOrEmpty(league.AlternateName))
        {
            var alternates = league.AlternateName.Split(_nameSeparators, StringSplitOptions.RemoveEmptyEntries);
            foreach (var alternate in alternates)
            {
                var normalizedAlternate = alternate.ToLowerInvariant().Trim();
                if (normalizedSearch == normalizedAlternate)
                {
                    return true;
                }
            }
        }

        // Partial match (contains)
        return normalizedSearch.Contains(normalizedLeague, StringComparison.Ordinal) || normalizedLeague.Contains(normalizedSearch, StringComparison.Ordinal);
    }
}
