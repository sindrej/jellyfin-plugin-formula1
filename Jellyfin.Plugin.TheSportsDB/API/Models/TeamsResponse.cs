using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TheSportsDB.API.Models;

/// <summary>
/// Response wrapper for teams from TheSportsDB API.
/// </summary>
public class TeamsResponse
{
    /// <summary>
    /// Gets the list of teams.
    /// </summary>
    [JsonPropertyName("teams")]
    public IReadOnlyList<Team>? Teams { get; init; }
}
