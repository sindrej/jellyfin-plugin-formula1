# Release Guide

This document describes how to create releases for the TheSportsDB Jellyfin plugin using the **automated GitHub Actions workflow**.

## 🚀 Automated Release Process

Releases are **fully automated** through GitHub Actions. When you merge a PR that updates the version in `build.yaml`, the workflow automatically:

✅ Builds the plugin DLL
✅ Creates a ZIP package
✅ Calculates checksums (MD5 + SHA256)
✅ Updates `manifest.json` with the new version
✅ Creates a git tag
✅ Creates a GitHub release with artifacts

**You only need to update 2 files and merge a PR!**

---

## Creating a New Release

### Step 1: Create Release Branch

```bash
# Create a new branch for the release
git checkout -b release-v1.0.1
```

### Step 2: Update Version in build.yaml

Edit `build.yaml` and update the version:

```yaml
---
name: "TheSportsDB"
guid: "55d3efd0-c081-4e0b-a57a-09402a4d549d"
version: "1.0.1.0"  # ← Update this
targetAbi: "10.9.0.0"
# ...
```

### Step 3: Update CHANGELOG.md

Add your release notes at the top of `CHANGELOG.md`:

```markdown
## [1.0.1] - 2025-11-24

### Added
- New feature 1
- New feature 2

### Fixed
- Bug fix 1

### Changed
- Improvement 1
```

**Format Rules:**
- Use `## [X.Y.Z] - YYYY-MM-DD` for the version header
- Include sections: `Added`, `Changed`, `Fixed`, `Removed` (as needed)
- Keep descriptions concise and user-focused

### Step 4: Commit Changes

```bash
git add build.yaml CHANGELOG.md
git commit -m "Release v1.0.1"
git push origin release-v1.0.1
```

### Step 5: Create Pull Request

1. Go to https://github.com/sindrej/jellyfin-plugin-formula1
2. Click "New Pull Request"
3. Select your `release-v1.0.1` branch
4. Title: `Release v1.0.1`
5. Description: Copy the changelog for this version
6. Create the PR

### Step 6: Merge the PR

Once the PR is reviewed and approved, **merge it to master**.

### Step 7: Watch the Magic! ✨

GitHub Actions will automatically (within ~2-3 minutes):

1. ✅ Detect the version change
2. ✅ Build the plugin
3. ✅ Create the release package
4. ✅ Calculate checksums
5. ✅ Update `manifest.json` with the new version entry
6. ✅ Commit the manifest update to master
7. ✅ Create git tag (e.g., `v1.0.1`)
8. ✅ Create GitHub release with:
   - `jellyfin-plugin-formula1_1.0.1.0.zip`
   - `checksums.txt`
   - Release notes from CHANGELOG.md

**Done!** Users can now install the new version from the plugin repository.

---

## Verifying the Release

After the workflow completes:

1. **Check the Release**: https://github.com/sindrej/jellyfin-plugin-formula1/releases
   - Verify the tag was created (e.g., `v1.0.1`)
   - Verify artifacts are attached (ZIP + checksums.txt)
   - Check release notes look correct

2. **Verify manifest.json**: https://raw.githubusercontent.com/sindrej/jellyfin-plugin-formula1/master/manifest.json
   - New version should be at the top of the `versions` array
   - Checksum should match the checksums.txt file
   - sourceUrl should point to the correct release

3. **Test Installation**:
   - In Jellyfin, go to Plugins → Repositories
   - Refresh the catalog
   - The new version should appear for update/install

---

## Workflow Details

### What Triggers the Workflow?

The workflow runs when:
- A push to `master` branch occurs
- The push modifies `build.yaml` or files in `Jellyfin.Plugin.TheSportsDB/`

This typically happens when a release PR is merged.

### Workflow File Location

`.github/workflows/release.yaml`

### What If a Tag Already Exists?

The workflow checks if a tag for the version already exists. If it does, the workflow skips the release to avoid duplicates.

### Manual Workflow Trigger

You can also manually trigger the workflow from the GitHub Actions tab if needed.

---

## Hotfix Releases

For urgent fixes:

```bash
# Create hotfix branch from master
git checkout -b hotfix-v1.0.2 master

# Make your fixes
# Update build.yaml to 1.0.2.0
# Update CHANGELOG.md

git add .
git commit -m "Hotfix v1.0.2: Fix critical bug"
git push origin hotfix-v1.0.2

# Create PR to master and merge
# Automated release will trigger
```

---

## Rollback a Release

If you need to rollback a release:

1. **Delete the GitHub Release**: https://github.com/sindrej/jellyfin-plugin-formula1/releases
   - Delete the problematic release
   - Delete the git tag

2. **Revert manifest.json**:
   ```bash
   git checkout master
   git pull
   # Edit manifest.json to remove the problematic version entry
   git add manifest.json
   git commit -m "Rollback release vX.Y.Z"
   git push
   ```

3. **Fix the issue and create a new release**

---

## Manual Release (Fallback)

If the automated workflow fails, you can create a release manually:

### Prerequisites

- .NET SDK 8.0 installed
- GitHub CLI (`gh`) installed

### Manual Steps

```bash
# 1. Build the plugin
dotnet publish Jellyfin.Plugin.TheSportsDB/Jellyfin.Plugin.TheSportsDB.csproj \
  --configuration Release \
  --output ./publish

# 2. Create release package
mkdir -p release
cd publish
zip -r ../release/jellyfin-plugin-formula1_1.0.1.0.zip Jellyfin.Plugin.TheSportsDB.dll
cd ..

# 3. Calculate checksum
md5sum release/jellyfin-plugin-formula1_1.0.1.0.zip

# 4. Update manifest.json manually with the checksum

# 5. Create tag and release
git add manifest.json
git commit -m "Update manifest for v1.0.1"
git push

git tag -a v1.0.1 -m "Release v1.0.1"
git push origin v1.0.1

gh release create v1.0.1 \
  ./release/jellyfin-plugin-formula1_1.0.1.0.zip \
  --title "TheSportsDB Formula 1 Plugin v1.0.1" \
  --notes-file CHANGELOG.md
```

---

## Troubleshooting

### Workflow Didn't Trigger

- Verify the PR modified `build.yaml`
- Check GitHub Actions tab for workflow runs
- Ensure the branch was merged (not closed without merging)

### Build Failed

- Check the workflow logs in GitHub Actions
- Verify the code compiles locally: `dotnet build`
- Look for missing dependencies or syntax errors

### manifest.json Not Updated

- The workflow commits changes as `github-actions[bot]`
- Check the commit history on master
- Verify the workflow had write permissions

### Wrong Checksum in manifest.json

- The workflow calculates MD5 automatically
- If incorrect, manually update manifest.json and push

### Tag Already Exists Error

- A tag with that version already exists
- Delete the tag: `git push --delete origin v1.0.1`
- Or increment the version number

---

## Version Numbering

Follow [Semantic Versioning](https://semver.org/):

- **Major (1.x.x)**: Breaking changes
- **Minor (x.1.x)**: New features, backwards compatible
- **Patch (x.x.1)**: Bug fixes, backwards compatible

Examples:
- `1.0.0` - Initial release
- `1.0.1` - Bug fix
- `1.1.0` - New feature (F2 support)
- `2.0.0` - Breaking change (new API)

---

## Release Checklist

Before merging a release PR:

- [ ] Version updated in `build.yaml`
- [ ] `CHANGELOG.md` updated with release notes
- [ ] Release notes follow the format guidelines
- [ ] Code builds successfully locally
- [ ] All tests pass (if applicable)
- [ ] PR has been reviewed

After merge:

- [ ] GitHub Actions workflow completed successfully
- [ ] Release created on GitHub
- [ ] manifest.json updated correctly
- [ ] Tag created
- [ ] Test installation from plugin repository

---

## FAQ

**Q: Do I need to update manifest.json manually?**
A: No! The workflow updates it automatically with the correct checksum and timestamp.

**Q: Can I create multiple releases in one day?**
A: Yes, just increment the patch version for each release.

**Q: What if I forget to update CHANGELOG.md?**
A: The release will still be created, but release notes will be empty. Update CHANGELOG.md and create a new patch release.

**Q: Can I test the workflow before merging?**
A: Create a test branch and push it. The workflow only runs on master, so it won't trigger a release.

**Q: How do I know the workflow succeeded?**
A: Check the GitHub Actions tab. You'll see a green checkmark and a release will appear.

---

## Support

For issues with the automated workflow:
- Check GitHub Actions logs
- Review the workflow file: `.github/workflows/release.yaml`
- Open an issue if you suspect a workflow bug

---

**Made with ❤️ for effortless releases!**
