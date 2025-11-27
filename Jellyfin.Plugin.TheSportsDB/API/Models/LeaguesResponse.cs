using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TheSportsDB.API.Models;

/// <summary>
/// Response wrapper for all leagues API endpoint.
/// </summary>
public class LeaguesResponse
{
    /// <summary>
    /// Gets or initializes the list of leagues.
    /// </summary>
    [JsonPropertyName("leagues")]
    public IReadOnlyList<League>? Leagues { get; init; }
}
