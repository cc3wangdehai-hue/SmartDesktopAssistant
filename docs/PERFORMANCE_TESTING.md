# Performance Testing Guide

## Quick Performance Test (Run Locally)

```powershell
# Run benchmarks
cd Tests\PerformanceTests
dotnet run -c Release
```

## Expected Results

| Operation | Target Time | Target Memory |
|-----------|-------------|---------------|
| Weather API | < 1000ms | < 1MB |
| City Search | < 500ms | < 500KB |
| Todo Save | < 10ms | < 100KB |
| Todo Load (100 items) | < 50ms | < 500KB |

## Memory Guidelines

- Startup: < 50MB
- After 1 hour use: < 100MB
- Maximum acceptable: < 200MB

## Log Analysis

Logs are stored at:
```
%LOCALAPPDATA%\SmartDesktopAssistant\Logs\log_YYYYMMDD.txt
```

To analyze logs, share the log file with me and I'll identify issues.
