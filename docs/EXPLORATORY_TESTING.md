# Exploratory Testing Guide

## What is Exploratory Testing?

Exploratory testing is unscripted testing where you freely explore the application to find bugs that automated tests might miss.

---

## Heuristics Checklist (启发式清单)

### 🎯 Functional Heuristics

| Heuristic | Questions to Ask |
|-----------|------------------|
| **Happy Path** | Does the main workflow complete successfully? |
| **Error Path** | What happens if something goes wrong? |
| **Boundary Values** | What if I enter very long text? Empty text? |
| **Default Values** | Are defaults sensible? |
| **Required Fields** | Can I skip required fields? |
| **Data Persistence** | Does data survive app restart? |

### 🔍 UI Heuristics

| Heuristic | Questions to Ask |
|-----------|------------------|
| **Visibility** | Can I see all elements clearly? |
| **Accessibility** | Can I use keyboard only? |
| **Responsiveness** | Does UI respond within 100ms? |
| **Consistency** | Do similar elements look similar? |
| **Feedback** | Does the app give clear feedback? |
| **Recovery** | Can I undo mistakes? |

### ⚡ Stress Heuristics

| Heuristic | Questions to Ask |
|-----------|------------------|
| **Rapid Clicking** | What if I click buttons rapidly? |
| **Long Running** | What if app runs for hours? |
| **Low Resources** | What if memory is low? |
| **Network Issues** | What if network is slow/unstable? |
| **Concurrent Ops** | What if I do multiple things at once? |

---

## Test Charters (测试章程)

### Charter 1: Weather Widget Stress Test
**Duration:** 15 minutes  
**Focus:** Test weather refresh under various conditions

| Scenario | Steps | Observations |
|----------|-------|--------------|
| Rapid Refresh | Click refresh 10 times quickly | Does it queue or debounce? |
| Offline Mode | Disconnect network, refresh | Error handling? |
| Slow Network | Throttle to 3G speed | Loading state? Timeout? |
| City Change | Rapidly switch between cities | Race conditions? |

### Charter 2: Todo Widget Data Integrity
**Duration:** 10 minutes  
**Focus:** Ensure data is never lost

| Scenario | Steps | Observations |
|----------|-------|--------------|
| Add-Del-Add | Add item, delete, add again | Data consistency? |
| Long Text | Add item with 1000+ characters | Truncation? UI overflow? |
| Special Chars | Add emoji, Chinese, symbols | Encoding issues? |
| Force Close | Add item, kill process | Data saved? |

### Charter 3: Multi-Window Interaction
**Duration:** 10 minutes  
**Focus:** Test all widgets together

| Scenario | Steps | Observations |
|----------|-------|--------------|
| All Widgets | Open all, interact randomly | Memory growth? Lag? |
| Drag Overlap | Drag widgets to overlap | Rendering issues? |
| Minimize All | Minimize all, restore | State preserved? |

---

## Bug Hunting Checklist

### 🔴 Critical Bugs to Look For

- [ ] App crashes or hangs
- [ ] Data loss or corruption
- [ ] Memory leak (growing memory usage)
- [ ] UI freezes during operations
- [ ] Buttons not responding

### 🟡 Important Bugs

- [ ] Confusing error messages
- [ ] UI alignment issues
- [ ] Slow response time (> 2s)
- [ ] Inconsistent behavior
- [ ] Accessibility issues

### 🟢 Minor Issues

- [ ] Typos in text
- [ ] Slight visual glitches
- [ ] Suboptimal defaults
- [ ] Missing tooltips

---

## Session Report Template

After each exploratory session, report:

```
## Exploratory Session Report

**Date:** YYYY-MM-DD
**Duration:** XX minutes
**Focus:** [Charter name]

### Findings

| # | Bug/Issue | Severity | Steps to Reproduce |
|---|-----------|----------|-------------------|
| 1 | | | |
| 2 | | | |

### Observations
- [Observation 1]
- [Observation 2]

### Questions for Developer
- [Question 1]
```
