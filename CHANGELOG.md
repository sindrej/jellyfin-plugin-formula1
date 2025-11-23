# Changelog

All notable changes to the TheSportsDB Jellyfin Plugin will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-11-23

### Added
- Initial release of TheSportsDB Formula 1 metadata plugin
- Episode metadata provider for Formula 1 Grand Prix races
  - Race name, date, and round number
  - Venue and country information
  - Race results and descriptions
- Series metadata provider for F1 seasons
  - Season year as series name
  - Race count and overview
  - F1-specific genres and tags
- Image provider for Formula 1 content
  - Event posters and banners
  - Team badges and logos
  - Fanart support
- TheSportsDB API client with full F1 support
  - Rate limiting (30 requests/minute for free tier)
  - Automatic retry with exponential backoff
  - HTTP 429 handling
- File-based metadata caching
  - Configurable cache duration (1-365 days)
  - Automatic cache expiration
- Configuration page with settings:
  - API key management
  - Cache duration
  - Rate limit configuration
  - Enable/disable toggle
- Comprehensive documentation
  - Installation guide
  - Usage instructions
  - Troubleshooting section
  - API information

### Technical Details
- Target: Jellyfin 10.9.0+
- Framework: .NET 8.0
- API: TheSportsDB v1
- License: GPLv3

## [Unreleased]

### Planned Features
- Support for other motorsports (F2, F3, Formula E)
- Enhanced driver metadata and statistics
- Team championship standings
- Qualifying results support
- Multi-language support for descriptions

---

## Version History

- [1.0.0] - Initial Release
