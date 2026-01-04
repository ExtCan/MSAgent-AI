# Built DLL

This folder is intended to contain the pre-built `MSAgentGTAV.dll` file.

## Why is the DLL not here?

The MSAgentGTAV script requires:
- Windows environment
- .NET Framework 4.8
- ScriptHookVDotNet3.dll dependency

This repository is built on **Linux CI runners** which cannot compile Windows .NET Framework projects.

## How to get the built DLL:

### Option 1: GitHub Actions Artifacts (Recommended)

The GitHub Actions workflow automatically builds the DLL on Windows runners:

1. Go to the [Actions tab](../../../actions/workflows/build-gtav-script.yml)
2. Click on the latest successful workflow run
3. Download the **MSAgentGTAV-Release-Package** artifact
4. Extract the ZIP to get `MSAgentGTAV.dll`

### Option 2: Build Locally on Windows

If you have Windows with Visual Studio:

1. Open `GTAVScripts/MSAgentGTAV.csproj` in Visual Studio
2. Ensure you have ScriptHookVDotNet3.dll (download from GTA V mods)
3. Build the project (Release configuration)
4. The DLL will be in `bin/Release/MSAgentGTAV.dll`

Alternatively, use the `build.bat` script:
```cmd
cd GTAVScripts
set GTAV_DIR=C:\Path\To\Your\GTA V
build.bat
```

### Option 3: Manual Windows Build

If GitHub Actions doesn't work for you and you need the DLL committed here:

1. Build the DLL on a Windows machine (see Option 2)
2. Copy `MSAgentGTAV.dll` to this `built/` folder
3. Commit and push the DLL

The `.gitignore` has been configured to allow DLLs in this folder specifically.

## Current Status

The GitHub Actions workflow should automatically build the DLL when changes are pushed to the GTAVScripts folder. Check the Actions tab for build status and downloadable artifacts.

**Note**: Due to CI limitations (Linux runners), pre-built DLLs cannot be automatically committed to this folder. You must either download from GitHub Actions artifacts or build locally on Windows.
