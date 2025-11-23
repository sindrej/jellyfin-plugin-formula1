using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.TheSportsDB.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        // Default to free tier API key
        ApiKey = "3";
        CacheDurationDays = 7;
        EnablePlugin = true;
        MaxRequestsPerMinute = 30;
    }

    /// <summary>
    /// Gets or sets the TheSportsDB API key.
    /// </summary>
    public string ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the cache duration in days for metadata.
    /// </summary>
    public int CacheDurationDays { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the plugin is enabled.
    /// </summary>
    public bool EnablePlugin { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of API requests per minute (30 for free tier, 100 for premium).
    /// </summary>
    public int MaxRequestsPerMinute { get; set; }
}
