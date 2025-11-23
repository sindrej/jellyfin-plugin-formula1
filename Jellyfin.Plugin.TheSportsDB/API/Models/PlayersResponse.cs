using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TheSportsDB.API.Models;

/// <summary>
/// Response wrapper for players from TheSportsDB API.
/// </summary>
public class PlayersResponse
{
    /// <summary>
    /// Gets or sets the list of players/drivers.
    /// </summary>
    [JsonPropertyName("players")]
    public IReadOnlyList<Player>? Players { get; init; }
}
