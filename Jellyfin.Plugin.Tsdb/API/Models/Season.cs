namespace Jellyfin.Plugin.Tsdb.API.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a season for a sports league from TheSportsDB API.
/// </summary>
public class Season
{
    /// <summary>
    /// Gets or sets the season ID.
    /// </summary>
    [JsonPropertyName("idSeason")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the season name/year (e.g., "2024" or "2023-2024").
    /// </summary>
    [JsonPropertyName("strSeason")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the league ID this season belongs to.
    /// </summary>
    [JsonPropertyName("idLeague")]
    public string? LeagueId { get; set; }

    /// <summary>
    /// Gets or sets the season poster image URL.
    /// </summary>
    [JsonPropertyName("strPoster")]
    public string? Poster { get; set; }
}
