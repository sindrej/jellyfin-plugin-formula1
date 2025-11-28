namespace Jellyfin.Plugin.Tsdb;

using System;
using System.Collections.Generic;
using System.Globalization;
using Jellyfin.Plugin.Tsdb.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

/// <summary>
/// The main plugin.
/// </summary>
public class TsdbPlugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Gets the provider name.
    /// </summary>
    public const string ProviderName = "TheSportsDB";

    /// <summary>
    /// Initializes a new instance of the <see cref="TsdbPlugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    public TsdbPlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    /// <inheritdoc />
    public override string Name => "TheSportsDB";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("55d3efd0-c081-4e0b-a57a-09402a4d549d");

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static TsdbPlugin? Instance { get; private set; }

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return
        [
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", GetType().Namespace)
            }
        ];
    }
}
