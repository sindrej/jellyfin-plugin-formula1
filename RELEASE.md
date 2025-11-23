# Release Guide

This document describes how to create and publish releases for the TheSportsDB Jellyfin plugin.

## Prerequisites

- .NET SDK 8.0 installed
- GitHub CLI (`gh`) installed and authenticated
- Write access to https://github.com/sindrej/jellyfin-plugin-formula1

## Release Process

### 1. Update Version Number

Update the version in the following files:
- `build.yaml` - Change `version` field
- `Jellyfin.Plugin.TheSportsDB/Jellyfin.Plugin.TheSportsDB.csproj` - If using Version property
- `manifest.json` - Add new version entry (see step 5)

### 2. Update CHANGELOG.md

Add release notes for the new version:
```markdown
## [X.Y.Z] - YYYY-MM-DD

### Added
- New feature 1
- New feature 2

### Changed
- Changed feature 1

### Fixed
- Bug fix 1
```

### 3. Build the Plugin

```bash
# Clean previous builds
dotnet clean

# Build in Release mode
dotnet build --configuration Release

# Or publish to get all dependencies
dotnet publish Jellyfin.Plugin.TheSportsDB/Jellyfin.Plugin.TheSportsDB.csproj \
  --configuration Release \
  --output ./publish
```

The DLL will be in:
- Build: `Jellyfin.Plugin.TheSportsDB/bin/Release/net8.0/Jellyfin.Plugin.TheSportsDB.dll`
- Publish: `./publish/Jellyfin.Plugin.TheSportsDB.dll`

### 4. Create Release Archive

```bash
# Create release directory
mkdir -p release
cd publish

# Create ZIP file with version number
zip -r ../release/jellyfin-plugin-formula1_1.0.0.0.zip Jellyfin.Plugin.TheSportsDB.dll

cd ..
```

### 5. Calculate Checksum

```bash
# Calculate MD5 checksum
md5sum release/jellyfin-plugin-formula1_1.0.0.0.zip

# Or SHA256
shasum -a 256 release/jellyfin-plugin-formula1_1.0.0.0.zip
```

### 6. Update manifest.json

Add a new version entry at the top of the `versions` array in `manifest.json`:

```json
{
  "version": "1.0.0.0",
  "changelog": "Release notes here",
  "targetAbi": "10.9.0.0",
  "sourceUrl": "https://github.com/sindrej/jellyfin-plugin-formula1/releases/download/v1.0.0/jellyfin-plugin-formula1_1.0.0.0.zip",
  "checksum": "INSERT_CHECKSUM_HERE",
  "timestamp": "2025-11-23T00:00:00Z"
}
```

Replace `INSERT_CHECKSUM_HERE` with the checksum from step 5.

### 7. Commit Changes

```bash
git add build.yaml manifest.json CHANGELOG.md
git commit -m "Release version 1.0.0"
git push
```

### 8. Create Git Tag

```bash
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
```

### 9. Create GitHub Release

Using GitHub CLI:

```bash
gh release create v1.0.0 \
  ./release/jellyfin-plugin-formula1_1.0.0.0.zip \
  --title "TheSportsDB Formula 1 Plugin v1.0.0" \
  --notes "$(cat <<EOF
## TheSportsDB Formula 1 Plugin v1.0.0

### Features
- Feature 1
- Feature 2

### Installation
Add this repository to Jellyfin:
\`\`\`
https://raw.githubusercontent.com/sindrej/jellyfin-plugin-formula1/master/manifest.json
\`\`\`

Or download the ZIP file and extract to your Jellyfin plugins directory.

### Requirements
- Jellyfin 10.9.0 or later
- .NET 8.0 Runtime

See the [README](https://github.com/sindrej/jellyfin-plugin-formula1) for full documentation.
EOF
)"
```

Or manually through GitHub web interface:
1. Go to https://github.com/sindrej/jellyfin-plugin-formula1/releases/new
2. Choose tag: v1.0.0
3. Release title: "TheSportsDB Formula 1 Plugin v1.0.0"
4. Add release notes
5. Upload `jellyfin-plugin-formula1_1.0.0.0.zip`
6. Publish release

### 10. Verify Installation

Test that users can install from your repository:

1. Open Jellyfin Dashboard
2. Go to Plugins → Repositories
3. Add repository:
   ```
   https://raw.githubusercontent.com/sindrej/jellyfin-plugin-formula1/master/manifest.json
   ```
4. Go to Plugins → Catalog
5. Find "TheSportsDB" plugin
6. Install and verify it works

## Quick Release Script

For future releases, you can use this script:

```bash
#!/bin/bash

VERSION=$1

if [ -z "$VERSION" ]; then
    echo "Usage: ./release.sh <version>"
    echo "Example: ./release.sh 1.0.1"
    exit 1
fi

echo "Building version $VERSION..."

# Clean and build
dotnet clean
dotnet publish Jellyfin.Plugin.TheSportsDB/Jellyfin.Plugin.TheSportsDB.csproj \
  --configuration Release \
  --output ./publish

# Create release package
mkdir -p release
cd publish
zip -r ../release/jellyfin-plugin-formula1_${VERSION}.0.zip Jellyfin.Plugin.TheSportsDB.dll
cd ..

# Calculate checksum
CHECKSUM=$(md5sum release/jellyfin-plugin-formula1_${VERSION}.0.zip | cut -d' ' -f1)
echo "Checksum: $CHECKSUM"

echo ""
echo "Release package created: release/jellyfin-plugin-formula1_${VERSION}.0.zip"
echo "Checksum: $CHECKSUM"
echo ""
echo "Next steps:"
echo "1. Update manifest.json with checksum: $CHECKSUM"
echo "2. Update CHANGELOG.md"
echo "3. Commit changes"
echo "4. Create tag: git tag -a v${VERSION} -m 'Release version ${VERSION}'"
echo "5. Push: git push && git push origin v${VERSION}"
echo "6. Create GitHub release with the ZIP file"
```

Save this as `release.sh`, make it executable with `chmod +x release.sh`, and run:

```bash
./release.sh 1.0.0
```

## Troubleshooting

### Build Errors

If you get build errors:
- Ensure .NET SDK 8.0 is installed: `dotnet --version`
- Clean the solution: `dotnet clean`
- Restore packages: `dotnet restore`

### Checksum Mismatch

If users report checksum errors:
- Verify the checksum in manifest.json matches the file
- Recalculate: `md5sum release/jellyfin-plugin-formula1_*.zip`
- Update manifest.json and commit

### Release Not Appearing in Jellyfin

- Verify manifest.json is accessible at the raw GitHub URL
- Check JSON syntax is valid
- Ensure version number format is correct (X.Y.Z.0)
- Verify sourceUrl points to actual release artifact
