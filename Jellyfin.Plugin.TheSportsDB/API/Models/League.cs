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
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the league name.
    /// </summary>
    [JsonPropertyName("strLeague")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the alternate league names (comma-separated).
    /// </summary>
    [JsonPropertyName("strLeagueAlternate")]
    public string? AlternateName { get; set; }

    /// <summary>
    /// Gets or sets the sport type.
    /// </summary>
    [JsonPropertyName("strSport")]
    public string? Sport { get; set; }

    /// <summary>
    /// Gets or sets the English description.
    /// </summary>
    [JsonPropertyName("strDescriptionEN")]
    public string? DescriptionEng { get; set; }

    /// <summary>
    /// Gets or sets the banner image URL.
    /// </summary>
    [JsonPropertyName("strBanner")]
    public string? Banner { get; set; }

    /// <summary>
    /// Gets or sets the badge image URL.
    /// </summary>
    [JsonPropertyName("strBadge")]
    public string? Badge { get; set; }

    /// <summary>
    /// Gets or sets the logo image URL.
    /// </summary>
    [JsonPropertyName("strLogo")]
    public string? Logo { get; set; }

    /// <summary>
    /// Gets or sets the poster image URL.
    /// </summary>
    [JsonPropertyName("strPoster")]
    public string? Poster { get; set; }

    /// <summary>
    /// Gets or sets the first fanart image URL.
    /// </summary>
    [JsonPropertyName("strFanart1")]
    public string? Fanart1 { get; set; }

    /// <summary>
    /// Gets or sets the second fanart image URL.
    /// </summary>
    [JsonPropertyName("strFanart2")]
    public string? Fanart2 { get; set; }

    /// <summary>
    /// Gets or sets the third fanart image URL.
    /// </summary>
    [JsonPropertyName("strFanart3")]
    public string? Fanart3 { get; set; }

    /// <summary>
    /// Gets or sets the fourth fanart image URL.
    /// </summary>
    [JsonPropertyName("strFanart4")]
    public string? Fanart4 { get; set; }
}
