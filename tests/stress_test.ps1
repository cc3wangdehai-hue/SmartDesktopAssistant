# Stress Test Script for Smart Desktop Assistant
# Tests performance under high load

param(
    [int]$DurationSeconds = 60,
    [int]$OperationsPerSecond = 10
)

Write-Host "==========================================" -ForegroundColor Yellow
Write-Host "Smart Desktop Assistant - Stress Test" -ForegroundColor Yellow
Write-Host "==========================================" -ForegroundColor Yellow
Write-Host "Duration: $DurationSeconds seconds"
Write-Host "Operations/sec: $OperationsPerSecond"
Write-Host ""

$startTime = Get-Date
$endTime = $startTime.AddSeconds($DurationSeconds)

# Start application
$exePath = ".\Output\SmartDesktopAssistant.exe"
if (-not (Test-Path $exePath)) {
    Write-Host "Error: Application not found. Build first." -ForegroundColor Red
    exit 1
}

$proc = Start-Process -FilePath $exePath -PassThru -WindowStyle Normal
Start-Sleep -Seconds 5

if ($proc.HasExited) {
    Write-Host "Error: Application crashed on startup" -ForegroundColor Red
    exit 1
}

# Monitor loop
$measurements = @()
$iteration = 0

Write-Host "Starting stress test..." -ForegroundColor Cyan

while ((Get-Date) -lt $endTime) {
    $iteration++
    
    # Force garbage collection periodically
    if ($iteration % 100 -eq 0) {
        [System.GC]::Collect()
    }
    
    # Record metrics
    $proc.Refresh()
    $memoryMB = [math]::Round($proc.WorkingSet64 / 1MB, 2)
    $cpuTime = $proc.TotalProcessorTime.TotalSeconds
    
    $measurements += @{
        Time = (Get-Date).ToString("HH:mm:ss")
        MemoryMB = $memoryMB
        CpuTime = $cpuTime
    }
    
    # Print status every 10 seconds
    if ($iteration % ($OperationsPerSecond * 10) -eq 0) {
        $elapsed = ((Get-Date) - $startTime).TotalSeconds
        Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Elapsed: $([math]::Round($elapsed))s | Memory: $memoryMB MB | CPU Time: $([math]::Round($cpuTime, 1))s"
    }
    
    Start-Sleep -Milliseconds (1000 / $OperationsPerSecond)
}

# Calculate results
$proc.Refresh()
$finalMemory = [math]::Round($proc.WorkingSet64 / 1MB, 2)
$initialMemory = $measurements[0].MemoryMB
$memoryGrowth = [math]::Round($finalMemory - $initialMemory, 2)
$avgMemory = [math]::Round(($measurements | Measure-Object -Property MemoryMB -Average).Average, 2)
$maxMemory = [math]::Round(($measurements | Measure-Object -Property MemoryMB -Maximum).Maximum, 2)

# Stop application
$proc.Kill()

# Summary
Write-Host "`n==========================================" -ForegroundColor Yellow
Write-Host "Stress Test Results" -ForegroundColor Yellow
Write-Host "==========================================" -ForegroundColor Yellow
Write-Host "Duration: $DurationSeconds seconds"
Write-Host "Iterations: $iteration"
Write-Host ""
Write-Host "Memory Statistics:" -ForegroundColor Cyan
Write-Host "  Initial: $initialMemory MB"
Write-Host "  Final: $finalMemory MB"
Write-Host "  Growth: $memoryGrowth MB"
Write-Host "  Average: $avgMemory MB"
Write-Host "  Peak: $maxMemory MB"
Write-Host ""

# Thresholds
$memoryThreshold = 150
if ($finalMemory -gt $memoryThreshold) {
    Write-Host "⚠ WARNING: Final memory ($finalMemory MB) exceeds threshold ($memoryThreshold MB)" -ForegroundColor Red
} elseif ($memoryGrowth -gt 50) {
    Write-Host "⚠ WARNING: Memory growth ($memoryGrowth MB) suggests possible leak" -ForegroundColor Yellow
} else {
    Write-Host "✅ Memory usage within acceptable limits" -ForegroundColor Green
}

# Save results to file
$resultsPath = "stress_test_results.json"
$results = @{
    DurationSeconds = $DurationSeconds
    Iterations = $iteration
    MemoryStats = @{
        InitialMB = $initialMemory
        FinalMB = $finalMemory
        GrowthMB = $memoryGrowth
        AverageMB = $avgMemory
        PeakMB = $maxMemory
    }
    Passed = ($finalMemory -le $memoryThreshold -and $memoryGrowth -le 50)
    Timestamp = (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
}
$results | ConvertTo-Json | Out-File $resultsPath
Write-Host "`nResults saved to: $resultsPath"
