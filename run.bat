@echo off
chcp 65001 >nul
echo ==========================================
echo    Smart Desktop Assistant - Launcher
echo ==========================================
echo.
echo Options:
echo   1. Standalone Widgets (Simple test version)
echo   2. Full Desktop Assistant (Advanced version)
echo.
set /p choice="Please select (1 or 2): "

if "%choice%"=="1" goto standalone
if "%choice%"=="2" goto full

:standalone
echo [INFO] Starting Standalone Widgets...
cd /d "%~dp0"
dotnet run --project WidgetsStandalone\WidgetsStandalone.csproj -c Release
goto end

:full
echo [INFO] Building Full Desktop Assistant...
cd /d "%~dp0"
dotnet build SmartDesktopAssistant.sln -c Release -o Output
if errorlevel 1 (
    echo [ERROR] Build failed!
    pause
    exit /b 1
)
echo.
echo [SUCCESS] Build completed!
echo.
echo [INFO] Starting Smart Desktop Assistant...
cd Output
start "" "SmartDesktopAssistant.exe"

:end
echo.
pause
