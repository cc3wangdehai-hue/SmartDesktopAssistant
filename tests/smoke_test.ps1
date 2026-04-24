# Smoke Test Script for Smart Desktop Assistant

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Smart Desktop Assistant - Smoke Test" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

$passCount = 0
$failCount = 0

function Test-Step {
    param([string]$name, [scriptblock]$test)
    Write-Host "Testing: $name" -NoNewline
    try {
        & $test
        Write-Host " [PASS]" -ForegroundColor Green
        $script:passCount++
    } catch {
        Write-Host " [FAIL]" -ForegroundColor Red
        Write-Host "  Error: $_" -ForegroundColor Red
        $script:failCount++
    }
}

# Tests
Test-Step "Build output" { if (-not (Test-Path ".\Output\SmartDesktopAssistant.exe")) { throw "Not found" } }
Test-Step "Widget DLLs" { if (-not (Test-Path ".\Output\WeatherWidget.dll")) { throw "Not found" } }
Test-Step "App starts" { $p = Start-Process ".\Output\SmartDesktopAssistant.exe" -PassThru; sleep 3; if ($p.HasExited) { throw "Crashed" }; $p.Kill() }

Write-Host "`nPassed: $passCount, Failed: $failCount"
exit $failCount
