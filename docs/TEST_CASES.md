# Smart Desktop Assistant - Test Case Checklist

## 📋 Test Summary

| Category | Total | Automated | Manual |
|----------|-------|-----------|--------|
| Unit Tests | 12 | 12 | 0 |
| UI Tests | 6 | 6 | 0 |
| Integration Tests | 6 | 0 | 6 |
| **Total** | **24** | **18** | **6** |

---

## 🤖 Automated Unit Tests (GitHub Actions)

### Weather Service Tests
- [x] `SearchCityAsync_ValidCity_ReturnsCityInfo`
- [x] `SearchCityAsync_InvalidCity_ReturnsNull`
- [x] `GetWeatherAsync_ValidCoordinates_ReturnsWeatherData`
- [x] `GetWeatherByCityAsync_ValidCity_ReturnsCityAndWeather`
- [x] `WeatherData_GetWeatherDescription_KnownCode_ReturnsCorrect`

### Todo Storage Tests
- [x] `Load_EmptyStorage_ReturnsEmptyList`
- [x] `Add_NewItem_StoresItem`
- [x] `Update_ExistingItem_UpdatesItem`
- [x] `Delete_ExistingItem_RemovesItem`

---

## 🖥️ UI Tests (Run Locally with FlaUI)

Run: `dotnet test Tests\UIAutomationTests`

- [ ] WeatherWindow visible on startup
- [ ] TodoWindow visible on startup
- [ ] FilesWindow visible on startup
- [ ] Add TODO item works
- [ ] Weather displays temperature

---

## 👤 Manual Tests (Your Windows PC)

### Quick Test (5 min)
1. Run `run.bat`, select option 2
2. Check all 3 widgets appear
3. Test adding a TODO item
4. Test tray icon menu

### Full Test
See docs/FULL_TEST_PLAN.md
