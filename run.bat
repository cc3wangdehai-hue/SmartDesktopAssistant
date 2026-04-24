@echo off
echo ==========================================
echo    Smart Desktop Assistant - Launcher
echo ==========================================
echo.
echo Options:
echo   1. Standalone Widgets (Simple test)
echo   2. Full Desktop Assistant
echo.
set /p choice="Please select (1 or 2): "

if "%choice%"=="1" goto standalone
if "%choice%"=="2" goto full
echo Invalid choice!
pause
exit /b 1

:standalone
echo [INFO] Building Standalone Widgets...
cd /d "%~dp0"
dotnet build WidgetsStandalone\WidgetsStandalone.csproj -c Release
if %ERRORLEVEL% neq 0 (
    echo [ERROR] Build failed!
    pause
    exit /b 1
)
echo [SUCCESS] Build completed!
echo [INFO] Starting Standalone Widgets...
start "" "WidgetsStandalone\bin\Release\net8.0-windows\WidgetsStandalone.exe"
goto end

:full
echo [INFO] Building Full Desktop Assistant...
cd /d "%~dp0"
dotnet build SmartDesktopAssistant\SmartDesktopAssistant.csproj -c Release
if %ERRORLEVEL% neq 0 (
    echo [ERROR] Build failed!
    pause
    exit /b 1
)
echo [SUCCESS] Build completed!
echo [INFO] Starting Smart Desktop Assistant...
start "" "SmartDesktopAssistant\bin\Release\net8.0-windows\SmartDesktopAssistant.exe"

:end
echo.
echo Press any key to exit...
pause >nul
