@echo off
echo ==========================================
echo    Smart Desktop Assistant - Installer
echo ==========================================
echo.

dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] .NET SDK is not installed!
    echo.
    echo Please install .NET 8.0 SDK from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)

echo [OK] .NET SDK is installed
dotnet --version
echo.

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

echo [INFO] Building solution...
dotnet build SmartDesktopAssistant.sln -c Release --no-restore
if errorlevel 1 (
    echo [ERROR] Build failed!
    pause
    exit /b 1
)

if not exist "Output" mkdir Output

echo [INFO] Copying files to Output folder...
xcopy /E /Y "SmartDesktopAssistant\bin\Release\net8.0-windows\*" "Output\" >nul 2>&1

echo.
echo ==========================================
echo    Installation Complete!
echo ==========================================
echo.
echo Output folder: %~dp0Output
echo.
echo To run the application:
echo   1. Double-click run.bat
echo   2. Or run SmartDesktopAssistant.exe from Output folder
echo.
pause
