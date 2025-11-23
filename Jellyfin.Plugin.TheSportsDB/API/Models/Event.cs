using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TheSportsDB.API.Models;

/// <summary>
/// Represents a motorsport event (Grand Prix) from TheSportsDB API.
/// </summary>
public class Event
{
    /// <summary>
    /// Gets or sets the event ID.
    /// </summary>
    [JsonPropertyName("idEvent")]
    public string? IdEvent { get; set; }

    /// <summary>
    /// Gets or sets the event name (e.g., "Monaco Grand Prix").
    /// </summary>
    [JsonPropertyName("strEvent")]
    public string? StrEvent { get; set; }

    /// <summary>
    /// Gets or sets the season year.
    /// </summary>
    [JsonPropertyName("strSeason")]
    public string? StrSeason { get; set; }

    /// <summary>
    /// Gets or sets the round number in the season.
    /// </summary>
    [JsonPropertyName("intRound")]
    public string? IntRound { get; set; }

    /// <summary>
    /// Gets or sets the event date (YYYY-MM-DD format).
    /// </summary>
    [JsonPropertyName("dateEvent")]
    public string? DateEvent { get; set; }

    /// <summary>
    /// Gets or sets the event time (UTC).
    /// </summary>
    [JsonPropertyName("strTime")]
    public string? StrTime { get; set; }

    /// <summary>
    /// Gets or sets the local event time.
    /// </summary>
    [JsonPropertyName("strTimeLocal")]
    public string? StrTimeLocal { get; set; }

    /// <summary>
    /// Gets or sets the venue/circuit ID.
    /// </summary>
    [JsonPropertyName("idVenue")]
    public string? IdVenue { get; set; }

    /// <summary>
    /// Gets or sets the venue/circuit name.
    /// </summary>
    [JsonPropertyName("strVenue")]
    public string? StrVenue { get; set; }

    /// <summary>
    /// Gets or sets the country.
    /// </summary>
    [JsonPropertyName("strCountry")]
    public string? StrCountry { get; set; }

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    [JsonPropertyName("strCity")]
    public string? StrCity { get; set; }

    /// <summary>
    /// Gets or sets the poster image URL.
    /// </summary>
    [JsonPropertyName("strPoster")]
    public string? StrPoster { get; set; }

    /// <summary>
    /// Gets or sets the square image URL.
    /// </summary>
    [JsonPropertyName("strSquare")]
    public string? StrSquare { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail image URL.
    /// </summary>
    [JsonPropertyName("strThumb")]
    public string? StrThumb { get; set; }

    /// <summary>
    /// Gets or sets the banner image URL.
    /// </summary>
    [JsonPropertyName("strBanner")]
    public string? StrBanner { get; set; }

    /// <summary>
    /// Gets or sets the fanart image URL.
    /// </summary>
    [JsonPropertyName("strFanart")]
    public string? StrFanart { get; set; }

    /// <summary>
    /// Gets or sets the video URL (YouTube highlights).
    /// </summary>
    [JsonPropertyName("strVideo")]
    public string? StrVideo { get; set; }

    /// <summary>
    /// Gets or sets the race result description.
    /// </summary>
    [JsonPropertyName("strResult")]
    public string? StrResult { get; set; }

    /// <summary>
    /// Gets or sets the English description.
    /// </summary>
    [JsonPropertyName("strDescriptionEN")]
    public string? StrDescriptionEN { get; set; }

    /// <summary>
    /// Gets or sets the event status.
    /// </summary>
    [JsonPropertyName("strStatus")]
    public string? StrStatus { get; set; }

    /// <summary>
    /// Gets or sets the number of spectators.
    /// </summary>
    [JsonPropertyName("intSpectators")]
    public string? IntSpectators { get; set; }
}
