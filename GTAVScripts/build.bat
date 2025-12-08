@echo off
REM MSAgent-AI GTA V Script Builder
REM This script builds the MSAgentGTAV.dll and copies it to your GTA V scripts folder

echo ================================
echo MSAgent-AI GTA V Script Builder
echo ================================
echo.

REM Check if GTAV_DIR is set
if not defined GTAV_DIR (
    echo ERROR: GTAV_DIR environment variable is not set!
    echo.
    echo Please set it to your GTA V installation directory:
    echo Example: setx GTAV_DIR "C:\Program Files\Rockstar Games\Grand Theft Auto V"
    echo.
    echo After setting it, restart this command prompt and try again.
    echo.
    pause
    exit /b 1
)

echo GTA V Directory: %GTAV_DIR%
echo.

REM Check if GTA V directory exists
if not exist "%GTAV_DIR%\GTA5.exe" (
    echo ERROR: GTA5.exe not found in %GTAV_DIR%
    echo Please check your GTAV_DIR environment variable.
    echo.
    pause
    exit /b 1
)

REM Check if ScriptHookVDotNet3.dll exists
if not exist "%GTAV_DIR%\ScriptHookVDotNet3.dll" (
    echo ERROR: ScriptHookVDotNet3.dll not found in %GTAV_DIR%
    echo.
    echo Please install ScriptHookVDotNet first:
    echo https://github.com/scripthookvdotnet/scripthookvdotnet/releases
    echo.
    pause
    exit /b 1
)

echo Building MSAgentGTAV.dll...
echo.

REM Build the project
msbuild MSAgentGTAV.csproj /p:Configuration=Release /v:minimal

if errorlevel 1 (
    echo.
    echo ERROR: Build failed!
    echo.
    echo Common solutions:
    echo 1. Install Visual Studio 2019 or later with .NET desktop development
    echo 2. Install .NET Framework 4.8 SDK from:
    echo    https://dotnet.microsoft.com/download/dotnet-framework/net48
    echo 3. Add MSBuild to your PATH, typically located at:
    echo    C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin
    echo 4. Or open MSAgentGTAV.csproj in Visual Studio and build there
    echo.
    echo If MSBuild is not found, you can also build using Developer Command Prompt
    echo for Visual Studio (search in Start Menu).
    echo.
    pause
    exit /b 1
)

echo.
echo Build successful!
echo.

REM Create scripts folder if it doesn't exist
if not exist "%GTAV_DIR%\scripts" (
    echo Creating scripts folder...
    mkdir "%GTAV_DIR%\scripts"
)

REM Copy the DLL
echo Copying MSAgentGTAV.dll to GTA V scripts folder...
copy /Y "bin\Release\MSAgentGTAV.dll" "%GTAV_DIR%\scripts\"

if errorlevel 1 (
    echo.
    echo ERROR: Failed to copy DLL to scripts folder.
    echo Please check permissions and try again.
    echo.
    pause
    exit /b 1
)

echo.
echo ================================
echo SUCCESS!
echo ================================
echo.
echo MSAgentGTAV.dll has been installed to:
echo %GTAV_DIR%\scripts\MSAgentGTAV.dll
echo.
echo Next steps:
echo 1. Make sure MSAgent-AI application is running
echo 2. Launch GTA V
echo 3. Press F9 in-game to open the menu
echo.
pause
