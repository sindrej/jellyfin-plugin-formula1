## Description

<!-- Briefly describe what this PR does -->

## Type of Change

- [ ] 🐛 Bug fix (patch version bump)
- [ ] ✨ New feature (minor version bump)
- [ ] 💥 Breaking change (major version bump)
- [ ] 📝 Documentation update
- [ ] 🔧 Configuration change
- [ ] 🚀 Release (automated release will trigger)

## Release Checklist

**Only for Release PRs** - if this is a release PR, ensure:

- [ ] Version updated in `build.yaml`
- [ ] `CHANGELOG.md` updated with release notes
  - [ ] Version header follows format: `## [X.Y.Z] - YYYY-MM-DD`
  - [ ] Added sections: Added / Changed / Fixed / Removed
  - [ ] Release notes are user-focused and clear
- [ ] Code builds successfully locally (`dotnet build`)
- [ ] All tests pass (if applicable)

**After merge, the automated workflow will:**
- ✅ Build and package the plugin
- ✅ Calculate checksums
- ✅ Update `manifest.json`
- ✅ Create git tag
- ✅ Create GitHub release with artifacts

## Testing

<!-- How has this been tested? -->

- [ ] Tested locally in development
- [ ] Tested with Jellyfin instance

## Additional Notes

<!-- Any additional information, context, or screenshots -->
