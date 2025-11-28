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
    public string? EventId { get; set; }

    /// <summary>
    /// Gets or sets the API Football ID.
    /// </summary>
    [JsonPropertyName("idAPIfootball")]
    public string? ApiFootballId { get; set; }

    /// <summary>
    /// Gets or sets the event name (e.g., "Monaco Grand Prix").
    /// </summary>
    [JsonPropertyName("strEvent")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the alternate event name.
    /// </summary>
    [JsonPropertyName("strEventAlternate")]
    public string? AlternateName { get; set; }

    /// <summary>
    /// Gets or sets the filename.
    /// </summary>
    [JsonPropertyName("strFilename")]
    public string? Filename { get; set; }

    /// <summary>
    /// Gets or sets the sport name.
    /// </summary>
    [JsonPropertyName("strSport")]
    public string? Sport { get; set; }

    /// <summary>
    /// Gets or sets the league ID.
    /// </summary>
    [JsonPropertyName("idLeague")]
    public string? LeagueId { get; set; }

    /// <summary>
    /// Gets or sets the league name.
    /// </summary>
    [JsonPropertyName("strLeague")]
    public string? League { get; set; }

    /// <summary>
    /// Gets or sets the league badge URL.
    /// </summary>
    [JsonPropertyName("strLeagueBadge")]
    public string? LeagueBadge { get; set; }

    /// <summary>
    /// Gets or sets the season year.
    /// </summary>
    [JsonPropertyName("strSeason")]
    public string? Season { get; set; }

    /// <summary>
    /// Gets or sets the English description.
    /// </summary>
    [JsonPropertyName("strDescriptionEN")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the home team name.
    /// </summary>
    [JsonPropertyName("strHomeTeam")]
    public string? HomeTeam { get; set; }

    /// <summary>
    /// Gets or sets the away team name.
    /// </summary>
    [JsonPropertyName("strAwayTeam")]
    public string? AwayTeam { get; set; }

    /// <summary>
    /// Gets or sets the home team score.
    /// </summary>
    [JsonPropertyName("intHomeScore")]
    public string? HomeScore { get; set; }

    /// <summary>
    /// Gets or sets the round number in the season.
    /// </summary>
    [JsonPropertyName("intRound")]
    public string? Round { get; set; }

    /// <summary>
    /// Gets or sets the away team score.
    /// </summary>
    [JsonPropertyName("intAwayScore")]
    public string? AwayScore { get; set; }

    /// <summary>
    /// Gets or sets the number of spectators.
    /// </summary>
    [JsonPropertyName("intSpectators")]
    public string? Spectators { get; set; }

    /// <summary>
    /// Gets or sets the official/referee name.
    /// </summary>
    [JsonPropertyName("strOfficial")]
    public string? Official { get; set; }

    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    [JsonPropertyName("strTimestamp")]
    public string? Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the event date (YYYY-MM-DD format).
    /// </summary>
    [JsonPropertyName("dateEvent")]
    public string? Date { get; set; }

    /// <summary>
    /// Gets or sets the local event date.
    /// </summary>
    [JsonPropertyName("dateEventLocal")]
    public string? DateLocal { get; set; }

    /// <summary>
    /// Gets or sets the event time (UTC).
    /// </summary>
    [JsonPropertyName("strTime")]
    public string? Time { get; set; }

    /// <summary>
    /// Gets or sets the local event time.
    /// </summary>
    [JsonPropertyName("strTimeLocal")]
    public string? TimeLocal { get; set; }

    /// <summary>
    /// Gets or sets the group/division.
    /// </summary>
    [JsonPropertyName("strGroup")]
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the home team ID.
    /// </summary>
    [JsonPropertyName("idHomeTeam")]
    public string? HomeTeamId { get; set; }

    /// <summary>
    /// Gets or sets the home team badge URL.
    /// </summary>
    [JsonPropertyName("strHomeTeamBadge")]
    public string? HomeTeamBadge { get; set; }

    /// <summary>
    /// Gets or sets the away team ID.
    /// </summary>
    [JsonPropertyName("idAwayTeam")]
    public string? AwayTeamId { get; set; }

    /// <summary>
    /// Gets or sets the away team badge URL.
    /// </summary>
    [JsonPropertyName("strAwayTeamBadge")]
    public string? AwayTeamBadge { get; set; }

    /// <summary>
    /// Gets or sets the score.
    /// </summary>
    [JsonPropertyName("intScore")]
    public string? Score { get; set; }

    /// <summary>
    /// Gets or sets the score votes.
    /// </summary>
    [JsonPropertyName("intScoreVotes")]
    public string? ScoreVotes { get; set; }

    /// <summary>
    /// Gets or sets the race result description.
    /// </summary>
    [JsonPropertyName("strResult")]
    public string? Result { get; set; }

    /// <summary>
    /// Gets or sets the venue/circuit ID.
    /// </summary>
    [JsonPropertyName("idVenue")]
    public string? VenueId { get; set; }

    /// <summary>
    /// Gets or sets the venue/circuit name.
    /// </summary>
    [JsonPropertyName("strVenue")]
    public string? Venue { get; set; }

    /// <summary>
    /// Gets or sets the country.
    /// </summary>
    [JsonPropertyName("strCountry")]
    public string? Country { get; set; }

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    [JsonPropertyName("strCity")]
    public string? City { get; set; }

    /// <summary>
    /// Gets or sets the poster image URL.
    /// </summary>
    [JsonPropertyName("strPoster")]
    public string? Poster { get; set; }

    /// <summary>
    /// Gets or sets the square image URL.
    /// </summary>
    [JsonPropertyName("strSquare")]
    public string? Square { get; set; }

    /// <summary>
    /// Gets or sets the fanart image URL.
    /// </summary>
    [JsonPropertyName("strFanart")]
    public string? Fanart { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail image URL.
    /// </summary>
    [JsonPropertyName("strThumb")]
    public string? Thumb { get; set; }

    /// <summary>
    /// Gets or sets the banner image URL.
    /// </summary>
    [JsonPropertyName("strBanner")]
    public string? Banner { get; set; }

    /// <summary>
    /// Gets or sets the map URL.
    /// </summary>
    [JsonPropertyName("strMap")]
    public string? Map { get; set; }

    /// <summary>
    /// Gets or sets the first tweet URL.
    /// </summary>
    [JsonPropertyName("strTweet1")]
    public string? Tweet1 { get; set; }

    /// <summary>
    /// Gets or sets the second tweet URL.
    /// </summary>
    [JsonPropertyName("strTweet2")]
    public string? Tweet2 { get; set; }

    /// <summary>
    /// Gets or sets the third tweet URL.
    /// </summary>
    [JsonPropertyName("strTweet3")]
    public string? Tweet3 { get; set; }

    /// <summary>
    /// Gets or sets the video URL (YouTube highlights).
    /// </summary>
    [JsonPropertyName("strVideo")]
    public string? Video { get; set; }

    /// <summary>
    /// Gets or sets the event status.
    /// </summary>
    [JsonPropertyName("strStatus")]
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets the postponed status.
    /// </summary>
    [JsonPropertyName("strPostponed")]
    public string? Postponed { get; set; }

    /// <summary>
    /// Gets or sets the locked status.
    /// </summary>
    [JsonPropertyName("strLocked")]
    public string? Locked { get; set; }
}
