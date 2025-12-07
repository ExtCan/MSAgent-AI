@echo off
echo Starting BeamNG to MSAgent-AI Bridge...
echo.
echo Make sure MSAgent-AI is running!
echo.

python bridge.py

if %errorlevel% neq 0 (
    echo.
    echo ERROR: Bridge server failed to start
    echo Check that:
    echo   1. Python is installed
    echo   2. Dependencies are installed (run setup.bat)
    echo   3. MSAgent-AI is running
    echo   4. Port 5000 is not in use
    echo.
    pause
)
