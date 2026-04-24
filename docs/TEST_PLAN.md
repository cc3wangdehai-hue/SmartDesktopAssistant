# Smart Desktop Assistant - Test Plan

## 1. Test Overview

| Item | Description |
|------|-------------|
| **Project** | Smart Desktop Assistant (C# WPF) |
| **Version** | 1.0.0 |
| **Test Period** | 2026-04-24 ~ TBD |
| **Coverage Target** | 80% |

---

## 2. Test Strategy

### 2.1 Testing Pyramid

```
                    ┌─────────┐
                    │ E2E/UI  │  10%  (Manual + FlaUI)
                    ├─────────┤
                    │Integration│ 20%  (API + Storage)
                    ├─────────┤
                    │  Unit   │  70%  (xUnit + Moq)
                    └─────────┘
```

### 2.2 Test Types

| Type | Coverage | Automation |
|------|----------|------------|
| Unit Tests | 70% | ✅ 100% Auto |
| Integration Tests | 20% | ✅ 100% Auto |
| UI Tests | 10% | ⚠️ 50% Auto, 50% Manual |

---

## 3. Functional Test Cases

### 3.1 Weather Widget

| ID | Test Case | Priority | Type |
|----|-----------|----------|------|
| W01 | Display current temperature | P0 | Unit+UI |
| W02 | Show weather description (晴/多云/雨) | P0 | Unit+UI |
| W03 | Refresh weather data | P1 | UI |
| W04 | Switch between two cities | P1 | UI |
| W05 | Handle API timeout gracefully | P2 | Unit |
| W06 | Display loading state | P1 | UI |
| W07 | Cache weather data locally | P2 | Unit |
| W08 | Show 7-day forecast | P2 | UI |

### 3.2 Todo Widget

| ID | Test Case | Priority | Type |
|----|-----------|----------|------|
| T01 | Add new todo item | P0 | Unit+UI |
| T02 | Mark item as completed | P0 | Unit+UI |
| T03 | Delete todo item | P0 | Unit+UI |
| T04 | Edit todo item | P1 | UI |
| T05 | Set priority (High/Medium/Low) | P1 | Unit |
| T06 | Set due date | P2 | Unit |
| T07 | Add notes to item | P2 | Unit |
| T08 | Persist data on close | P0 | Unit |
| T09 | Handle empty state | P1 | UI |
| T10 | Sort by priority/date | P2 | Unit |

### 3.3 Files Widget

| ID | Test Case | Priority | Type |
|----|-----------|----------|------|
| F01 | Drag & drop file | P0 | UI (Manual) |
| F02 | Display file list | P0 | UI |
| F03 | Open file on double-click | P0 | UI |
| F04 | Remove file from list | P1 | UI |
| F05 | Show file icon/thumbnail | P2 | UI |
| F06 | Handle multiple files | P1 | UI |

### 3.4 Settings

| ID | Test Case | Priority | Type |
|----|-----------|----------|------|
| S01 | Change city names | P1 | UI |
| S02 | Toggle widget visibility | P1 | UI |
| S03 | Enable/disable auto-start | P2 | Unit+Registry |
| S04 | Save settings persistently | P0 | Unit |

### 3.5 System Tray

| ID | Test Case | Priority | Type |
|----|-----------|----------|------|
| ST01 | Show tray icon | P0 | UI |
| ST02 | Context menu works | P0 | UI |
| ST03 | Toggle widgets from menu | P1 | UI |
| ST04 | Exit application cleanly | P0 | UI |

---

## 4. Non-Functional Test Cases

### 4.1 Performance

| ID | Test Case | Threshold | Type |
|----|-----------|-----------|------|
| P01 | Startup time | < 5s | Auto |
| P02 | Memory at idle | < 100MB | Auto |
| P03 | CPU at idle | < 5% | Auto |
| P04 | Weather API response | < 3s | Auto |
| P05 | Todo save operation | < 50ms | Auto |

### 4.2 Compatibility

| ID | Test Case | Target | Type |
|----|-----------|--------|------|
| C01 | Windows 10 22H2 | x64 | Manual |
| C02 | Windows 11 | x64 | Manual |
| C03 | DPI scaling 125% | 120 DPI | Auto+Manual |
| C04 | DPI scaling 150% | 144 DPI | Auto+Manual |
| C05 | High contrast mode | Theme | Manual |
| C06 | Dark mode support | Theme | Manual |

### 4.3 Security

| ID | Test Case | Type |
|----|-----------|------|
| SEC01 | No sensitive data in logs | Manual |
| SEC02 | Settings stored in AppData | Unit |
| SEC03 | No registry modifications without permission | Unit |
| SEC04 | Dependency vulnerability check | Auto |

---

## 5. Test Environment

| Environment | OS | .NET | Purpose |
|-------------|-----|------|---------|
| CI | Windows Latest | 8.0.x | Automated Tests |
| Dev | Windows 10/11 | 8.0.x | Manual Testing |
| UAT | Windows 10/11 | 8.0.x | User Acceptance |

---

## 6. Test Schedule

| Phase | Activities | Duration |
|-------|-----------|----------|
| Phase 1 | Unit + Integration Tests | ✅ Done |
| Phase 2 | UI Automation Tests | In Progress |
| Phase 3 | Manual Exploratory Tests | Pending |
| Phase 4 | UAT | Pending |

---

## 7. Entry/Exit Criteria

### Entry Criteria
- [ ] Code compiles without errors
- [ ] All unit tests pass
- [ ] Coverage > 70%

### Exit Criteria
- [ ] All P0 tests pass
- [ ] All P1 tests pass
- [ ] Coverage > 80%
- [ ] No critical bugs open
- [ ] Performance thresholds met

---

## 8. Risk Analysis

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Weather API unavailable | Medium | High | Add cache, show last known |
| Network timeout | High | Medium | Graceful degradation |
| DPI scaling issues | Medium | Medium | Test on multiple displays |
| Memory leak | Low | High | Long-running tests |
