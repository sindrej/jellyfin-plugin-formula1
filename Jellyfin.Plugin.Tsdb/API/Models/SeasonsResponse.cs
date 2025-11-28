namespace Jellyfin.Plugin.Tsdb.API.Models;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Response wrapper for seasons API endpoint.
/// </summary>
public class SeasonsResponse
{
    /// <summary>
    /// Gets or initializes the list of seasons.
    /// </summary>
    [JsonPropertyName("seasons")]
    public IReadOnlyList<Season>? Seasons { get; init; }
}
