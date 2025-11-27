using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TheSportsDB.API.Models;

/// <summary>
/// Represents a season for a sports league from TheSportsDB API.
/// </summary>
public class Season
{
    /// <summary>
    /// Gets or sets the season ID.
    /// </summary>
    [JsonPropertyName("idSeason")]
    public string? IdSeason { get; set; }

    /// <summary>
    /// Gets or sets the season name/year (e.g., "2024" or "2023-2024").
    /// </summary>
    [JsonPropertyName("strSeason")]
    public string? StrSeason { get; set; }

    /// <summary>
    /// Gets or sets the league ID this season belongs to.
    /// </summary>
    [JsonPropertyName("idLeague")]
    public string? IdLeague { get; set; }
}
