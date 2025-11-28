namespace Jellyfin.Plugin.Tsdb.API.Models;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Response wrapper for league details API endpoint.
/// </summary>
public class LeagueDetailsResponse
{
    /// <summary>
    /// Gets or initializes the list of league details.
    /// </summary>
    [JsonPropertyName("leagues")]
    public IReadOnlyList<League>? Leagues { get; init; }
}
