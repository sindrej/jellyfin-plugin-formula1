namespace Jellyfin.Plugin.Tsdb.API.Models;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Response wrapper for events from TheSportsDB API.
/// </summary>
public class EventsResponse
{
    /// <summary>
    /// Gets the list of events.
    /// </summary>
    [JsonPropertyName("events")]
    public IReadOnlyList<Round>? Events { get; init; }
}
