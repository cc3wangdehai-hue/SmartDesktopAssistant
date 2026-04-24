@echo off
echo ==========================================
echo    Build Verification Script
echo ==========================================
echo.

set ERRORS=0

echo [1/5] Checking project files...
echo.

if not exist "SmartDesktopAssistant\SmartDesktopAssistant.csproj" (
    echo    [FAIL] Missing: SmartDesktopAssistant.csproj
    set ERRORS=1
) else (
    echo    [OK] SmartDesktopAssistant.csproj
)

if not exist "Widgets\WeatherWidget\WeatherWidget.csproj" (
    echo    [FAIL] Missing: WeatherWidget.csproj
    set ERRORS=1
) else (
    echo    [OK] WeatherWidget.csproj
)

if not exist "Widgets\TodoWidget\TodoWidget.csproj" (
    echo    [FAIL] Missing: TodoWidget.csproj
    set ERRORS=1
) else (
    echo    [OK] TodoWidget.csproj
)

if not exist "Widgets\FilesWidget\FilesWidget.csproj" (
    echo    [FAIL] Missing: FilesWidget.csproj
    set ERRORS=1
) else (
    echo    [OK] FilesWidget.csproj
)

if not exist "Widgets\SettingsWidget\SettingsWidget.csproj" (
    echo    [FAIL] Missing: SettingsWidget.csproj
    set ERRORS=1
) else (
    echo    [OK] SettingsWidget.csproj
)

if not exist "SmartDesktopAssistant.sln" (
    echo    [FAIL] Missing: SmartDesktopAssistant.sln
    set ERRORS=1
) else (
    echo    [OK] SmartDesktopAssistant.sln
)

if %ERRORS%==1 (
    echo.
    echo [ERROR] Project files missing!
    pause
    exit /b 1
)

echo.
echo [PASS] All project files exist
echo.

echo [2/5] Checking solution structure...
dotnet sln SmartDesktopAssistant.sln list >nul 2>&1
if errorlevel 1 (
    echo    [FAIL] Solution file has issues
    set ERRORS=1
) else (
    echo    [OK] Solution structure correct
)
echo.

echo [3/5] Restoring NuGet packages...
dotnet restore SmartDesktopAssistant.sln --verbosity quiet
if errorlevel 1 (
    echo    [FAIL] Package restore failed
    set ERRORS=1
) else (
    echo    [OK] Packages restored
)
echo.

echo [4/5] Building project (Release)...
dotnet build SmartDesktopAssistant.sln -c Release --no-restore -v quiet
if errorlevel 1 (
    echo.
    echo    [FAIL] Build failed! Details:
    echo.
    dotnet build SmartDesktopAssistant.sln -c Release --no-restore
    set ERRORS=1
) else (
    echo    [OK] Build successful
)
echo.

echo [5/5] Verifying output files...
echo.

if not exist "SmartDesktopAssistant\bin\Release\net8.0-windows\SmartDesktopAssistant.exe" (
    echo    [FAIL] SmartDesktopAssistant.exe not generated
    set ERRORS=1
) else (
    echo    [OK] SmartDesktopAssistant.exe generated
)

if not exist "SmartDesktopAssistant\bin\Release\net8.0-windows\WeatherWidget.dll" (
    echo    [FAIL] WeatherWidget.dll not generated
    set ERRORS=1
) else (
    echo    [OK] WeatherWidget.dll generated
)

if not exist "SmartDesktopAssistant\bin\Release\net8.0-windows\TodoWidget.dll" (
    echo    [FAIL] TodoWidget.dll not generated
    set ERRORS=1
) else (
    echo    [OK] TodoWidget.dll generated
)

echo.
echo ==========================================
echo              Summary
echo ==========================================
echo.

if %ERRORS%==0 (
    echo    All checks passed!
    echo.
    echo    Output location:
    echo    %~dp0SmartDesktopAssistant\bin\Release\net8.0-windows\
    echo.
    pause
    exit /b 0
) else (
    echo    Issues found. Please fix and retry!
    echo.
    pause
    exit /b 1
)
