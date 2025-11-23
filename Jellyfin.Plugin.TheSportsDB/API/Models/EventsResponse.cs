using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TheSportsDB.API.Models;

/// <summary>
/// Response wrapper for events from TheSportsDB API.
/// </summary>
public class EventsResponse
{
    /// <summary>
    /// Gets or sets the list of events.
    /// </summary>
    [JsonPropertyName("events")]
    public IReadOnlyList<Event>? Events { get; init; }
}
