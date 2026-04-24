@echo off
chcp 65001 >nul
echo ==========================================
echo    Smart Desktop Assistant - Installer
echo ==========================================
echo.

:: Check if .NET SDK is installed
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] .NET SDK is not installed!
    echo.
    echo Please install .NET 8.0 SDK:
    echo 1. Visit: https://dotnet.microsoft.com/download/dotnet/8.0
    echo 2. Download and install "SDK" (not Runtime)
    echo 3. Restart this installer
    echo.
    pause
    exit /b 1
)

echo [OK] .NET SDK is installed:
dotnet --version
echo.

:: Restore NuGet packages
echo [INFO] Restoring NuGet packages...
cd /d "%~dp0"
dotnet restore SmartDesktopAssistant.sln
if errorlevel 1 (
    echo [ERROR] Package restore failed!
    pause
    exit /b 1
)
echo [OK] Packages restored.
echo.

:: Build the solution
echo [INFO] Building solution...
dotnet build SmartDesktopAssistant.sln -c Release --no-restore
if errorlevel 1 (
    echo [ERROR] Build failed!
    pause
    exit /b 1
)

:: Create output directory
if not exist "Output" mkdir Output

:: Copy files to output
echo [INFO] Copying files to Output folder...
xcopy /E /Y "SmartDesktopAssistant\bin\Release\net8.0-windows\*" "Output\" >nul 2>&1
xcopy /E /Y "Widgets\WeatherWidget\bin\Release\net8.0-windows\*" "Output\" >nul 2>&1
xcopy /E /Y "Widgets\TodoWidget\bin\Release\net8.0-windows\*" "Output\" >nul 2>&1

echo.
echo ==========================================
echo    Installation Complete!
echo ==========================================
echo.
echo Output folder: %~dp0Output
echo.
echo To run the application:
echo   1. Double-click run.bat
echo   2. Or run SmartDesktopAssistant.exe directly
echo.
echo Features:
echo   - Weather Widget (2 cities)
echo   - TODO Widget (local storage)
echo   - System tray support
echo.
pause
