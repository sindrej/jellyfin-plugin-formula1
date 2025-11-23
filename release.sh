#!/bin/bash

VERSION=$1

if [ -z "$VERSION" ]; then
    echo "Usage: ./release.sh <version>"
    echo "Example: ./release.sh 1.0.1"
    exit 1
fi

echo "Building TheSportsDB Plugin version $VERSION..."
echo ""

# Clean previous builds
echo "Cleaning previous builds..."
dotnet clean
rm -rf publish release

# Build the plugin
echo "Building plugin..."
dotnet publish Jellyfin.Plugin.TheSportsDB/Jellyfin.Plugin.TheSportsDB.csproj \
  --configuration Release \
  --output ./publish

if [ $? -ne 0 ]; then
    echo "Build failed!"
    exit 1
fi

# Create release package
echo "Creating release package..."
mkdir -p release
cd publish
zip -r ../release/jellyfin-plugin-formula1_${VERSION}.0.zip Jellyfin.Plugin.TheSportsDB.dll
cd ..

# Calculate checksum
CHECKSUM=$(md5 -q release/jellyfin-plugin-formula1_${VERSION}.0.zip 2>/dev/null || md5sum release/jellyfin-plugin-formula1_${VERSION}.0.zip | cut -d' ' -f1)

echo ""
echo "=========================================="
echo "Release package created successfully!"
echo "=========================================="
echo ""
echo "File: release/jellyfin-plugin-formula1_${VERSION}.0.zip"
echo "MD5 Checksum: $CHECKSUM"
echo ""
echo "Next steps:"
echo "1. Update manifest.json:"
echo "   - Add new version entry"
echo "   - Set checksum to: $CHECKSUM"
echo "   - Set sourceUrl to: https://github.com/sindrej/jellyfin-plugin-formula1/releases/download/v${VERSION}/jellyfin-plugin-formula1_${VERSION}.0.zip"
echo ""
echo "2. Update CHANGELOG.md with release notes"
echo ""
echo "3. Update build.yaml version to: ${VERSION}.0"
echo ""
echo "4. Commit changes:"
echo "   git add build.yaml manifest.json CHANGELOG.md"
echo "   git commit -m \"Release version ${VERSION}\""
echo ""
echo "5. Create and push tag:"
echo "   git tag -a v${VERSION} -m \"Release version ${VERSION}\""
echo "   git push && git push origin v${VERSION}"
echo ""
echo "6. Create GitHub release:"
echo "   gh release create v${VERSION} ./release/jellyfin-plugin-formula1_${VERSION}.0.zip \\"
echo "     --title \"TheSportsDB Formula 1 Plugin v${VERSION}\" \\"
echo "     --notes-file CHANGELOG.md"
echo ""
echo "See RELEASE.md for detailed instructions."
echo ""
