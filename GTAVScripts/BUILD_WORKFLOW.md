# GitHub Actions Build Workflow

This document explains the automated build system for the GTA V MSAgent integration script.

## Workflow File

`.github/workflows/build-gtav-script.yml`

## What It Does

The workflow automatically:

1. **Downloads Dependencies**
   - Fetches ScriptHookVDotNet v3.6.0 from the official GitHub releases
   - Extracts ScriptHookVDotNet3.dll for compilation
   - No need to manually provide the dependency

2. **Builds the Script**
   - Updates the .csproj to use the downloaded DLL
   - Compiles MSAgentGTAV.cs to MSAgentGTAV.dll
   - Verifies the build succeeded

3. **Creates Artifacts**
   - **MSAgentGTAV-Script**: DLL + core documentation (README, QUICKSTART)
   - **MSAgentGTAV-Release-Package**: Complete package with INSTALL.txt

## When It Runs

The workflow triggers on:

- **Push to main/master** - When changes to GTAVScripts/ are merged
- **Pull Requests** - To test builds before merging
- **Manual Trigger** - Via GitHub's "Run workflow" button
- **File Changes** - Only when GTAVScripts/ or the workflow file changes

## How to Download Artifacts

1. Go to the repository on GitHub
2. Click the "Actions" tab
3. Click on a successful workflow run (green checkmark)
4. Scroll to the "Artifacts" section at the bottom
5. Download either:
   - **MSAgentGTAV-Script** (minimal, just the DLL)
   - **MSAgentGTAV-Release-Package** (complete with docs)

## Artifact Contents

### MSAgentGTAV-Script
```
├── MSAgentGTAV.dll          # The compiled script
├── README.md                # Full documentation
└── QUICKSTART.md            # Quick setup guide
```

### MSAgentGTAV-Release-Package
```
├── MSAgentGTAV.dll          # The compiled script
├── README.md                # Full documentation
├── QUICKSTART.md            # Quick setup guide
├── TROUBLESHOOTING.md       # Debug guide
└── INSTALL.txt              # Installation instructions
```

## Retention

Artifacts are kept for **90 days** after the workflow run.

## Manual Triggering

To manually trigger a build:

1. Go to Actions tab
2. Select "Build GTA V Script" workflow
3. Click "Run workflow" button
4. Select the branch
5. Click "Run workflow"

## Build Requirements

The workflow runs on `windows-latest` and requires:
- MSBuild (provided by microsoft/setup-msbuild@v2)
- Internet access to download ScriptHookVDotNet
- .NET Framework 4.8 (included in Windows)

## Troubleshooting

**Build fails with "ScriptHookVDotNet3.dll not found":**
- The download step may have failed
- Check the workflow logs for download errors
- The ScriptHookVDotNet release URL may need updating

**Artifact upload fails:**
- Check that MSAgentGTAV.dll was created in bin/Release/
- Verify the paths in the upload-artifact step

**Workflow doesn't trigger:**
- Check if changes were made to GTAVScripts/ directory
- Try manual trigger via workflow_dispatch
- Ensure you're pushing to main/master or in a PR targeting those branches

## Updating ScriptHookVDotNet Version

To use a different version of ScriptHookVDotNet:

1. Edit `.github/workflows/build-gtav-script.yml`
2. Find the line: `$url = "https://github.com/scripthookvdotnet/scripthookvdotnet/releases/download/v3.6.0/..."`
3. Update to the desired version
4. Commit and push

## Local Building

For local development, use:
- `build.bat` (automated script)
- Visual Studio (open MSAgentGTAV.csproj)
- MSBuild command line

The workflow is primarily for creating distributable builds.
