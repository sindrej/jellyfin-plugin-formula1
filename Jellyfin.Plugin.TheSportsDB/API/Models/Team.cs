using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TheSportsDB.API.Models;

/// <summary>
/// Represents a motorsport team from TheSportsDB API.
/// </summary>
public class Team
{
    /// <summary>
    /// Gets or sets the team ID.
    /// </summary>
    [JsonPropertyName("idTeam")]
    public string? IdTeam { get; set; }

    /// <summary>
    /// Gets or sets the team name.
    /// </summary>
    [JsonPropertyName("strTeam")]
    public string? StrTeam { get; set; }

    /// <summary>
    /// Gets or sets the alternate team name.
    /// </summary>
    [JsonPropertyName("strTeamAlternate")]
    public string? StrTeamAlternate { get; set; }

    /// <summary>
    /// Gets or sets the year the team was formed.
    /// </summary>
    [JsonPropertyName("intFormedYear")]
    public string? IntFormedYear { get; set; }

    /// <summary>
    /// Gets or sets the team location/headquarters.
    /// </summary>
    [JsonPropertyName("strLocation")]
    public string? StrLocation { get; set; }

    /// <summary>
    /// Gets or sets the team country.
    /// </summary>
    [JsonPropertyName("strCountry")]
    public string? StrCountry { get; set; }

    /// <summary>
    /// Gets or sets the team badge URL.
    /// </summary>
    [JsonPropertyName("strBadge")]
    public string? StrBadge { get; set; }

    /// <summary>
    /// Gets or sets the team logo URL.
    /// </summary>
    [JsonPropertyName("strLogo")]
    public string? StrLogo { get; set; }

    /// <summary>
    /// Gets or sets the team banner URL.
    /// </summary>
    [JsonPropertyName("strBanner")]
    public string? StrBanner { get; set; }

    /// <summary>
    /// Gets or sets the team description in English.
    /// </summary>
    [JsonPropertyName("strDescriptionEN")]
    public string? StrDescriptionEN { get; set; }

    /// <summary>
    /// Gets or sets the team website.
    /// </summary>
    [JsonPropertyName("strWebsite")]
    public string? StrWebsite { get; set; }

    /// <summary>
    /// Gets or sets the team Facebook page.
    /// </summary>
    [JsonPropertyName("strFacebook")]
    public string? StrFacebook { get; set; }

    /// <summary>
    /// Gets or sets the team Twitter handle.
    /// </summary>
    [JsonPropertyName("strTwitter")]
    public string? StrTwitter { get; set; }

    /// <summary>
    /// Gets or sets the team Instagram handle.
    /// </summary>
    [JsonPropertyName("strInstagram")]
    public string? StrInstagram { get; set; }

    /// <summary>
    /// Gets or sets the team YouTube channel.
    /// </summary>
    [JsonPropertyName("strYoutube")]
    public string? StrYoutube { get; set; }

    /// <summary>
    /// Gets or sets the primary team color.
    /// </summary>
    [JsonPropertyName("strColour1")]
    public string? StrColour1 { get; set; }

    /// <summary>
    /// Gets or sets the secondary team color.
    /// </summary>
    [JsonPropertyName("strColour2")]
    public string? StrColour2 { get; set; }

    /// <summary>
    /// Gets or sets the tertiary team color.
    /// </summary>
    [JsonPropertyName("strColour3")]
    public string? StrColour3 { get; set; }

    /// <summary>
    /// Gets or sets fanart image 1 URL.
    /// </summary>
    [JsonPropertyName("strFanart1")]
    public string? StrFanart1 { get; set; }

    /// <summary>
    /// Gets or sets fanart image 2 URL.
    /// </summary>
    [JsonPropertyName("strFanart2")]
    public string? StrFanart2 { get; set; }

    /// <summary>
    /// Gets or sets fanart image 3 URL.
    /// </summary>
    [JsonPropertyName("strFanart3")]
    public string? StrFanart3 { get; set; }

    /// <summary>
    /// Gets or sets fanart image 4 URL.
    /// </summary>
    [JsonPropertyName("strFanart4")]
    public string? StrFanart4 { get; set; }
}

/// <summary>
/// Response wrapper for teams from TheSportsDB API.
/// </summary>
public class TeamsResponse
{
    /// <summary>
    /// Gets or sets the list of teams.
    /// </summary>
    [JsonPropertyName("teams")]
    public List<Team>? Teams { get; set; }
}
