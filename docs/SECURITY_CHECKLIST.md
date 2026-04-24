# Security Testing Checklist

## Automated Security Checks

### Dependency Vulnerability Scan

Run this command to check for known vulnerabilities:

```bash
dotnet list package --vulnerable --include-transitive
```

Add to CI pipeline to fail on high-severity vulnerabilities.

### Static Analysis

```bash
dotnet build /warnaserror
```

---

## Security Test Cases

### SEC-01: No Sensitive Data in Logs

| Check | Expected | Status |
|-------|----------|--------|
| Passwords never logged | No password in log files | ☐ |
| API keys never logged | No API key in log files | ☐ |
| User data sanitized | PII masked in logs | ☐ |

### SEC-02: Secure Storage

| Check | Expected | Status |
|-------|----------|--------|
| Settings in AppData | Not in Program Files | ☐ |
| No plaintext passwords | Hash or encrypt | ☐ |
| File permissions correct | User-only access | ☐ |

### SEC-03: Registry Safety

| Check | Expected | Status |
|-------|----------|--------|
| HKCU only, not HKLM | No admin required | ☐ |
| Registry keys cleaned on uninstall | No orphaned keys | ☐ |
| No sensitive data in registry | Only settings | ☐ |

### SEC-04: Network Security

| Check | Expected | Status |
|-------|----------|--------|
| HTTPS for API calls | weather API uses HTTPS | ☐ |
| Certificate validation | Validates SSL certs | ☐ |
| No hardcoded credentials | Config or env vars | ☐ |

### SEC-05: Input Validation

| Check | Expected | Status |
|-------|----------|--------|
| City name sanitized | No injection possible | ☐ |
| Todo text sanitized | No XSS/storage issues | ☐ |
| File path validated | No path traversal | ☐ |

### SEC-06: Update Security

| Check | Expected | Status |
|-------|----------|--------|
| Updates from trusted source | HTTPS + signature | ☐ |
| No auto-execute downloads | User confirmation | ☐ |

---

## Security Scan Results Template

```
## Security Scan Report

**Date:** YYYY-MM-DD
**Scanner:** dotnet list package --vulnerable

### Vulnerabilities Found

| Package | Severity | CVE | Action |
|---------|----------|-----|--------|
| | | | |

### Recommendations
- [Recommendation 1]
- [Recommendation 2]

### Status
☐ Pass  ☐ Fail  ☐ Needs Review
```
