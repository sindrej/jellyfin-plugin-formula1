# Jellyfin TheSportsDB Plugin

A Jellyfin metadata plugin that provides Formula 1 metadata for race recordings using the TheSportsDB.com API.

## Features

- **Episode Metadata**: Fetches metadata for individual Formula 1 Grand Prix races
- **Series Metadata**: Organizes F1 seasons as TV series
- **Image Support**: Downloads race posters, banners, fanart, and team logos
- **Rate Limiting**: Built-in rate limiting to respect API limits (30 requests/minute for free tier)
- **Caching**: Local metadata caching to minimize API calls
- **Configurable**: Easy configuration through Jellyfin's plugin settings

## Requirements

- Jellyfin Server 10.9.0 or later
- .NET 8.0 Runtime
- TheSportsDB API key (free tier key "3" is included by default)

## Installation

### Building from Source

1. Ensure you have [.NET SDK 8.0](https://dotnet.microsoft.com/download) installed
2. Clone this repository
3. Build the plugin:
   ```bash
   cd Jellyfin.Plugin.TheSportsDB
   dotnet build
   ```
4. The compiled DLL will be in `bin/Debug/net8.0/`

### Installing the Plugin

1. Copy `Jellyfin.Plugin.TheSportsDB.dll` to your Jellyfin plugins directory:
   - **Linux**: `~/.local/share/jellyfin/plugins/TheSportsDB/`
   - **Windows**: `%APPDATA%\jellyfin\plugins\TheSportsDB\`
   - **macOS**: `~/.local/share/jellyfin/plugins/TheSportsDB/`
2. Restart Jellyfin Server
3. Navigate to Dashboard → Plugins to verify installation

## Configuration

Access the plugin configuration through:
**Dashboard → Plugins → TheSportsDB → Settings**

### Configuration Options

- **Enable Plugin**: Toggle the plugin on/off
- **API Key**: Your TheSportsDB API key (default: "3" for free tier)
  - Get your own key at [TheSportsDB.com](https://www.thesportsdb.com/api.php)
- **Cache Duration**: How long to cache metadata (1-365 days)
  - Longer duration reduces API calls but may show outdated data
  - Recommended: 7 days for current season, 30+ days for historical seasons
- **Max Requests Per Minute**: API rate limit
  - Free tier: 30 requests/minute
  - Premium tier ($9/month): 100 requests/minute

## Usage

### Organizing Your F1 Content

For best results, organize your F1 race recordings as follows:

```
Formula 1/
├── 2024/
│   ├── Round 01 - Bahrain Grand Prix.mkv
│   ├── Round 02 - Saudi Arabian Grand Prix.mkv
│   └── ...
├── 2023/
│   ├── Round 01 - Bahrain Grand Prix.mkv
│   └── ...
```

### Library Setup

1. Create a new library or use an existing TV Shows library
2. Add your F1 content folder
3. In Library settings:
   - Set content type to "TV Shows"
   - Enable "TheSportsDB" under metadata providers
   - You may want to disable other metadata providers to avoid conflicts
4. Scan the library

### Naming Conventions

The plugin expects:
- **Season = Year**: Each F1 season should be organized by year (e.g., 2024, 2023)
- **Episode = Race**: Individual race files
- **Episode Number**: Round number in the season (optional but recommended)

Examples of supported naming:
- `Formula 1 2024/Round 01 - Bahrain Grand Prix.mkv`
- `F1 2024/01 - Bahrain Grand Prix.mkv`
- `Formula 1/2024/Monaco Grand Prix.mkv`

## Features in Detail

### Metadata Fetched

For each race (episode):
- Race name (e.g., "Monaco Grand Prix")
- Race date and time
- Round number
- Circuit/venue information
- Country and city
- Race results and description
- Related images (poster, banner, fanart)

For each season (series):
- Season year
- Number of races
- Season overview
- F1-related genres and tags

### Image Types

- **Primary**: Race posters, team badges
- **Banner**: Event banners, team banners
- **Backdrop/Fanart**: High-resolution backgrounds
- **Thumb**: Thumbnail images

## Limitations

### Free Tier API Limits

The free tier of TheSportsDB API has the following limitations:
- **30 requests per minute**: The plugin enforces this automatically
- **15 results per season query**: F1 has ~23 races per season
  - The plugin makes multiple API calls if needed
- **Limited search results**: 2 results per search query

These limitations mean:
- Initial library scans may take longer due to rate limiting
- Very large historical libraries may take significant time to fully populate
- Consider caching aggressively (7-30 day cache duration)

### Data Coverage

TheSportsDB provides:
- ✅ Race names, dates, venues
- ✅ Team information (current teams)
- ✅ Driver information
- ✅ Race results (text descriptions)
- ✅ Images (posters, logos, fanart)
- ❌ Lap-by-lap timing data
- ❌ Championship standings
- ❌ Detailed qualifying results

## Troubleshooting

### Plugin Not Appearing

- Verify DLL is in correct plugins directory
- Check Jellyfin logs for plugin loading errors
- Ensure .NET 8.0 runtime is installed

### No Metadata Found

- Verify plugin is enabled in configuration
- Check API key is valid
- Review naming conventions for your files
- Check Jellyfin logs for API errors
- Verify rate limits aren't being exceeded

### Slow Metadata Fetching

- Increase cache duration to reduce API calls
- Check network connectivity to TheSportsDB API
- Consider upgrading to premium API tier for higher rate limits

### Rate Limit Errors (HTTP 429)

- The plugin automatically waits 60 seconds on rate limit errors
- Reduce concurrent library scans
- Increase cache duration
- Consider premium API tier ($9/month for 100 req/min)

## API Information

This plugin uses TheSportsDB API v1:
- **Base URL**: https://www.thesportsdb.com/api/v1/json/
- **Documentation**: https://www.thesportsdb.com/api.php
- **Formula 1 League ID**: 4370

### API Tiers

| Feature | Free Tier | Premium ($9/month) |
|---------|-----------|-------------------|
| Requests/minute | 30 | 100 |
| Results per query | 2-15 | 10-3000 |
| API access | v1 only | v1 + v2 |

## Development

### Project Structure

```
Jellyfin.Plugin.TheSportsDB/
├── API/
│   ├── Models/          # API response models
│   └── TheSportsDBClient.cs
├── Cache/
│   └── MetadataCache.cs # Local caching
├── Configuration/
│   ├── PluginConfiguration.cs
│   └── configPage.html
├── Providers/
│   ├── TheSportsDBEpisodeProvider.cs
│   ├── TheSportsDBSeriesProvider.cs
│   └── TheSportsDBImageProvider.cs
└── Plugin.cs
```

### Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

This project is licensed under the GPLv3 License - see the LICENSE file for details.

Note: When compiled, this plugin links against Jellyfin's GPLv3-licensed packages, making the binary GPLv3 as well.

## Credits

- **Jellyfin**: https://jellyfin.org/
- **TheSportsDB**: https://www.thesportsdb.com/
- **Formula 1**: https://www.formula1.com/

## Support

For issues and questions:
- Check Jellyfin logs first
- Review this README and troubleshooting section
- Open an issue on the GitHub repository

## Version History

### 1.0.0 (Initial Release)
- Formula 1 metadata support
- Episode (race) metadata provider
- Series (season) metadata provider
- Image provider for posters and fanart
- Rate limiting and caching
- Configurable API settings
