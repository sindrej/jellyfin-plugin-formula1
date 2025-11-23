# Jellyfin TheSportsDB Formula 1 Plugin

A Jellyfin metadata plugin that provides Formula 1 metadata for race recordings using the TheSportsDB.com API.

[![GitHub release](https://img.shields.io/github/v/release/sindrej/jellyfin-plugin-formula1)](https://github.com/sindrej/jellyfin-plugin-formula1/releases)
[![License](https://img.shields.io/github/license/sindrej/jellyfin-plugin-formula1)](LICENSE)

## Features

- **🏎️ Formula 1 Metadata**: Complete race, season, team, and driver information
- **📺 Episode Provider**: Individual Grand Prix metadata with results and descriptions
- **📚 Series Provider**: Organize F1 seasons as TV series
- **🖼️ Rich Images**: Posters, banners, team logos, and fanart
- **⚡ Smart Caching**: Configurable local caching to minimize API calls
- **🔒 Rate Limiting**: Built-in rate limiting (30 req/min free tier, 100 req/min premium)
- **⚙️ Easy Configuration**: Simple setup through Jellyfin's dashboard

## Installation

### Method 1: Install from Repository (Recommended)

1. Open Jellyfin Dashboard
2. Navigate to **Plugins → Repositories**
3. Click **Add Repository**
4. Enter:
   - **Repository Name**: `TheSportsDB Formula 1`
   - **Repository URL**:
     ```
     https://raw.githubusercontent.com/sindrej/jellyfin-plugin-formula1/master/manifest.json
     ```
5. Click **Save**
6. Go to **Plugins → Catalog**
7. Find **TheSportsDB** in the metadata section
8. Click **Install**
9. Restart Jellyfin

### Method 2: Manual Installation

1. Download the latest release ZIP from [Releases](https://github.com/sindrej/jellyfin-plugin-formula1/releases)
2. Extract `Jellyfin.Plugin.TheSportsDB.dll`
3. Copy the DLL to your Jellyfin plugins directory:
   - **Linux**: `~/.local/share/jellyfin/plugins/TheSportsDB/`
   - **Windows**: `%APPDATA%\jellyfin\plugins\TheSportsDB\`
   - **macOS**: `~/.local/share/jellyfin/plugins/TheSportsDB/`
4. Restart Jellyfin

### Method 3: Build from Source

See [Building from Source](#building-from-source) section below.

## Quick Start

### 1. Configure the Plugin

After installation:
1. Go to **Dashboard → Plugins → TheSportsDB**
2. Click **Settings**
3. Configure:
   - ✅ **Enable Plugin**: Check to activate
   - 🔑 **API Key**: Default is "3" (free tier) - [Get your own key](https://www.thesportsdb.com/api.php)
   - ⏱️ **Cache Duration**: 7 days recommended (longer for historical content)
   - 📊 **Max Requests/Minute**: 30 (free) or 100 (premium $9/month)
4. Click **Save**

### 2. Organize Your F1 Content

Structure your files like this:

```
Formula 1/
├── 2024/
│   ├── Round 01 - Bahrain Grand Prix.mkv
│   ├── Round 02 - Saudi Arabian Grand Prix.mkv
│   ├── Round 03 - Australian Grand Prix.mkv
│   └── ...
├── 2023/
│   ├── Round 01 - Bahrain Grand Prix.mkv
│   ├── Round 02 - Saudi Arabian Grand Prix.mkv
│   └── ...
```

**Naming Tips:**
- Season = Year (2024, 2023, etc.)
- Include round number for best matching
- Race names should match official Grand Prix names

### 3. Set Up Your Library

1. Create a new **TV Shows** library (or use existing)
2. Add your F1 content folder
3. In library settings:
   - **Content type**: TV Shows
   - **Metadata downloaders**: Enable **TheSportsDB**, disable others
   - **Image fetchers**: Enable **TheSportsDB**
4. **Scan Library**

## What You Get

### Race (Episode) Metadata
- Official race name and date
- Round number in season
- Circuit/venue information
- Country and city
- Race results summary
- Event posters and images

### Season (Series) Metadata
- Season year
- Number of races
- Season overview
- F1-specific genres

### Images
- Race event posters
- Circuit banners
- Team logos and badges
- High-resolution fanart

## Requirements

- **Jellyfin**: 10.9.0 or later
- **.NET Runtime**: 8.0
- **API Key**: TheSportsDB (free tier included)

## API Information

This plugin uses the TheSportsDB API:
- **API**: https://www.thesportsdb.com/api.php
- **Documentation**: https://www.thesportsdb.com/docs_api_examples
- **Formula 1 League ID**: 4370

### Free Tier vs Premium

| Feature | Free Tier | Premium ($9/month) |
|---------|-----------|-------------------|
| Requests/minute | 30 | 100 |
| Results per query | 2-15 | 10-3000 |
| API version | v1 only | v1 + v2 |
| **Best for** | Small libraries, patient scans | Large libraries, quick scans |

## Configuration

Access plugin settings: **Dashboard → Plugins → TheSportsDB → Settings**

| Setting | Default | Description |
|---------|---------|-------------|
| Enable Plugin | ✅ Checked | Turn plugin on/off |
| API Key | `3` | Free tier key (get your own at TheSportsDB.com) |
| Cache Duration | `7` days | How long to cache metadata (1-365 days) |
| Max Requests/Minute | `30` | API rate limit (30=free, 100=premium) |

## Troubleshooting

### No Metadata Found

- ✅ Verify plugin is enabled in settings
- ✅ Check file naming matches F1 race names
- ✅ Review Jellyfin logs for errors
- ✅ Ensure API key is valid
- ✅ Check network connectivity to thesportsdb.com

### Slow Metadata Fetching

- Increase cache duration to reduce API calls
- Consider upgrading to premium tier (100 req/min)
- Reduce concurrent library scans

### Rate Limit Errors (HTTP 429)

The plugin automatically waits 60 seconds when rate limited. To reduce occurrences:
- Increase cache duration
- Reduce library scan frequency
- Upgrade to premium tier

### Plugin Not Appearing in Catalog

- Verify repository URL is correct in Plugins → Repositories
- Check the URL is accessible: [manifest.json](https://raw.githubusercontent.com/sindrej/jellyfin-plugin-formula1/master/manifest.json)
- Refresh the catalog page

## Building from Source

### Prerequisites
- [.NET SDK 8.0](https://dotnet.microsoft.com/download)
- Git

### Build Steps

```bash
# Clone the repository
git clone https://github.com/sindrej/jellyfin-plugin-formula1.git
cd jellyfin-plugin-formula1

# Build the plugin
dotnet build --configuration Release

# The DLL will be in:
# Jellyfin.Plugin.TheSportsDB/bin/Release/net8.0/Jellyfin.Plugin.TheSportsDB.dll
```

### Creating a Release

See [RELEASE.md](RELEASE.md) for detailed release instructions.

Quick release:
```bash
./release.sh 1.0.0
```

## Project Structure

```
jellyfin-plugin-formula1/
├── Jellyfin.Plugin.TheSportsDB/
│   ├── API/                    # API client and models
│   │   ├── Models/            # Event, Team, Player models
│   │   └── TheSportsDBClient.cs
│   ├── Cache/                 # Metadata caching
│   ├── Configuration/         # Plugin config and UI
│   ├── Providers/             # Metadata providers
│   │   ├── TheSportsDBEpisodeProvider.cs
│   │   ├── TheSportsDBSeriesProvider.cs
│   │   └── TheSportsDBImageProvider.cs
│   └── Plugin.cs              # Main plugin class
├── manifest.json              # Plugin repository manifest
├── CHANGELOG.md               # Version history
├── RELEASE.md                 # Release guide
└── release.sh                 # Release automation script
```

## Contributing

Contributions welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

### Development Setup

See the [Jellyfin Plugin Development Guide](https://jellyfin.org/docs/general/server/plugins/) for IDE setup and debugging.

## Roadmap

Future enhancements:
- [ ] Support for F2, F3, Formula E
- [ ] Driver championship standings
- [ ] Qualifying results
- [ ] Team points tracking
- [ ] Multi-language descriptions
- [ ] Custom metadata field mapping

## Credits & Attribution

- **Jellyfin**: https://jellyfin.org/
- **TheSportsDB**: https://www.thesportsdb.com/
- **Formula 1**: https://www.formula1.com/

## License

This project is licensed under the **GNU General Public License v3.0** - see the [LICENSE](LICENSE) file for details.

**Important**: When compiled, this plugin links against Jellyfin's GPLv3-licensed packages, making the binary GPLv3-licensed as well.

## Support

- 📖 **Documentation**: [Plugin README](Jellyfin.Plugin.TheSportsDB/README.md)
- 🐛 **Issues**: [GitHub Issues](https://github.com/sindrej/jellyfin-plugin-formula1/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/sindrej/jellyfin-plugin-formula1/discussions)

## Disclaimer

This plugin is not officially affiliated with, endorsed by, or connected to:
- Formula 1 Companies
- TheSportsDB
- Jellyfin Project

All trademarks and copyrights belong to their respective owners.

---

**Made with ❤️ for the Jellyfin and Formula 1 communities**
