using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TheSportsDB.API.Models;

/// <summary>
/// Represents a motorsport driver from TheSportsDB API.
/// </summary>
public class Player
{
    /// <summary>
    /// Gets or sets the player/driver ID.
    /// </summary>
    [JsonPropertyName("idPlayer")]
    public string? IdPlayer { get; set; }

    /// <summary>
    /// Gets or sets the player/driver full name.
    /// </summary>
    [JsonPropertyName("strPlayer")]
    public string? StrPlayer { get; set; }

    /// <summary>
    /// Gets or sets the player/driver last name.
    /// </summary>
    [JsonPropertyName("strLastName")]
    public string? StrLastName { get; set; }

    /// <summary>
    /// Gets or sets the driver's nationality.
    /// </summary>
    [JsonPropertyName("strNationality")]
    public string? StrNationality { get; set; }

    /// <summary>
    /// Gets or sets the driver's birth location.
    /// </summary>
    [JsonPropertyName("strBirthLocation")]
    public string? StrBirthLocation { get; set; }

    /// <summary>
    /// Gets or sets the driver's birth date.
    /// </summary>
    [JsonPropertyName("dateBorn")]
    public string? DateBorn { get; set; }

    /// <summary>
    /// Gets or sets the driver's height.
    /// </summary>
    [JsonPropertyName("strHeight")]
    public string? StrHeight { get; set; }

    /// <summary>
    /// Gets or sets the driver's weight.
    /// </summary>
    [JsonPropertyName("strWeight")]
    public string? StrWeight { get; set; }

    /// <summary>
    /// Gets or sets the driver's racing number.
    /// </summary>
    [JsonPropertyName("strNumber")]
    public string? StrNumber { get; set; }

    /// <summary>
    /// Gets or sets the driver's position/role.
    /// </summary>
    [JsonPropertyName("strPosition")]
    public string? StrPosition { get; set; }

    /// <summary>
    /// Gets or sets the driver thumbnail image URL.
    /// </summary>
    [JsonPropertyName("strThumb")]
    public string? StrThumb { get; set; }

    /// <summary>
    /// Gets or sets the driver cutout image URL.
    /// </summary>
    [JsonPropertyName("strCutout")]
    public string? StrCutout { get; set; }

    /// <summary>
    /// Gets or sets the driver render image URL.
    /// </summary>
    [JsonPropertyName("strRender")]
    public string? StrRender { get; set; }

    /// <summary>
    /// Gets or sets the driver poster image URL.
    /// </summary>
    [JsonPropertyName("strPoster")]
    public string? StrPoster { get; set; }

    /// <summary>
    /// Gets or sets the driver banner image URL.
    /// </summary>
    [JsonPropertyName("strBanner")]
    public string? StrBanner { get; set; }

    /// <summary>
    /// Gets or sets the driver description in English.
    /// </summary>
    [JsonPropertyName("strDescriptionEN")]
    public string? StrDescriptionEN { get; set; }

    /// <summary>
    /// Gets or sets the driver's Twitter handle.
    /// </summary>
    [JsonPropertyName("strTwitter")]
    public string? StrTwitter { get; set; }

    /// <summary>
    /// Gets or sets the driver's Instagram handle.
    /// </summary>
    [JsonPropertyName("strInstagram")]
    public string? StrInstagram { get; set; }

    /// <summary>
    /// Gets or sets the driver's Facebook page.
    /// </summary>
    [JsonPropertyName("strFacebook")]
    public string? StrFacebook { get; set; }

    /// <summary>
    /// Gets or sets the team ID the driver belongs to.
    /// </summary>
    [JsonPropertyName("idTeam")]
    public string? IdTeam { get; set; }

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
/// Response wrapper for players from TheSportsDB API.
/// </summary>
public class PlayersResponse
{
    /// <summary>
    /// Gets or sets the list of players/drivers.
    /// </summary>
    [JsonPropertyName("players")]
    public List<Player>? Players { get; set; }
}
