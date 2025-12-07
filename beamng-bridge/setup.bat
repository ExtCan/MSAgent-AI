@echo off
echo ========================================
echo MSAgent-AI BeamNG Bridge Setup
echo ========================================
echo.

echo Step 1: Checking Python installation...
python --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Python is not installed or not in PATH
    echo Please install Python 3.8 or higher from https://www.python.org/
    pause
    exit /b 1
)
python --version

echo.
echo Step 2: Installing dependencies...
pip install -r requirements.txt
if %errorlevel% neq 0 (
    echo ERROR: Failed to install dependencies
    pause
    exit /b 1
)

echo.
echo Step 3: Checking MSAgent-AI connection...
echo Make sure MSAgent-AI is running before starting the bridge!
echo.

echo ========================================
echo Setup Complete!
echo ========================================
echo.
echo To start the bridge server, run:
echo   python bridge.py
echo.
echo The server will run on http://localhost:5000
echo.
pause
