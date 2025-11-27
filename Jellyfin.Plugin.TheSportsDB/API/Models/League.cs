using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TheSportsDB.API.Models;

/// <summary>
/// Represents a sports league from TheSportsDB API.
/// </summary>
public class League
{
    /// <summary>
    /// Gets or sets the league ID.
    /// </summary>
    [JsonPropertyName("idLeague")]
    public string? IdLeague { get; set; }

    /// <summary>
    /// Gets or sets the league name.
    /// </summary>
    [JsonPropertyName("strLeague")]
    public string? StrLeague { get; set; }

    /// <summary>
    /// Gets or sets the alternate league names (comma-separated).
    /// </summary>
    [JsonPropertyName("strLeagueAlternate")]
    public string? StrLeagueAlternate { get; set; }

    /// <summary>
    /// Gets or sets the sport type.
    /// </summary>
    [JsonPropertyName("strSport")]
    public string? StrSport { get; set; }

    /// <summary>
    /// Gets or sets the English description.
    /// </summary>
    [JsonPropertyName("strDescriptionEN")]
    public string? StrDescriptionEN { get; set; }

    /// <summary>
    /// Gets or sets the banner image URL.
    /// </summary>
    [JsonPropertyName("strBanner")]
    public string? StrBanner { get; set; }

    /// <summary>
    /// Gets or sets the badge image URL.
    /// </summary>
    [JsonPropertyName("strBadge")]
    public string? StrBadge { get; set; }

    /// <summary>
    /// Gets or sets the logo image URL.
    /// </summary>
    [JsonPropertyName("strLogo")]
    public string? StrLogo { get; set; }

    /// <summary>
    /// Gets or sets the poster image URL.
    /// </summary>
    [JsonPropertyName("strPoster")]
    public string? StrPoster { get; set; }

    /// <summary>
    /// Gets or sets the first fanart image URL.
    /// </summary>
    [JsonPropertyName("strFanart1")]
    public string? StrFanart1 { get; set; }

    /// <summary>
    /// Gets or sets the second fanart image URL.
    /// </summary>
    [JsonPropertyName("strFanart2")]
    public string? StrFanart2 { get; set; }

    /// <summary>
    /// Gets or sets the third fanart image URL.
    /// </summary>
    [JsonPropertyName("strFanart3")]
    public string? StrFanart3 { get; set; }

    /// <summary>
    /// Gets or sets the fourth fanart image URL.
    /// </summary>
    [JsonPropertyName("strFanart4")]
    public string? StrFanart4 { get; set; }
}
