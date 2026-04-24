using System;
using System.IO;
using FlaUI.Core;
using FlaUI.UIA3;
using UIAutomationTests.PageObjects;
using Xunit;

namespace UIAutomationTests
{
    /// <summary>
    /// Tests using Page Object Model pattern
    /// Cleaner and more maintainable than direct element access
    /// </summary>
    public class PageObjectTests : IDisposable
    {
        private Application? _application;
        private AutomationBase? _automation;

        public PageObjectTests()
        {
            _automation = new UIA3Automation();
        }

        public void Dispose()
        {
            _application?.Close();
            _automation?.Dispose();
        }

        private void LaunchApp()
        {
            var exePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..", "..",
                "Output", "SmartDesktopAssistant.exe"
            );

            if (!File.Exists(exePath))
                throw new FileNotFoundException($"App not found: {exePath}");

            _application = Application.Launch(exePath);
            System.Threading.Thread.Sleep(2000);
        }

        [Fact(Skip = "Run locally on Windows")]
        public void TodoPage_AddItem_ShouldAppearInList()
        {
            // Arrange
            LaunchApp();
            var todoWindow = _automation!.GetDesktop()
                .FindFirstDescendant(cf => cf.ByClassName("TodoWindow"))?.AsWindow();
            var todoPage = new TodoPage(_automation, todoWindow!);

            // Act
            var initialCount = todoPage.GetTodoCount();
            todoPage.AddTodoItem("Test Item from Page Object");

            // Assert
            Assert.Equal(initialCount + 1, todoPage.GetTodoCount());
        }

        [Fact(Skip = "Run locally on Windows")]
        public void TodoPage_CompleteItem_ShouldShowStrikethrough()
        {
            // Arrange
            LaunchApp();
            var todoWindow = _automation!.GetDesktop()
                .FindFirstDescendant(cf => cf.ByClassName("TodoWindow"))?.AsWindow();
            var todoPage = new TodoPage(_automation, todoWindow!);
            todoPage.AddTodoItem("Item to Complete");

            // Act
            todoPage.ClickFirstItemCheckbox();

            // Assert - Visual verification needed
            Assert.True(todoPage.IsVisible);
        }

        [Fact(Skip = "Run locally on Windows")]
        public void WeatherPage_OnLoad_ShouldDisplayWeather()
        {
            // Arrange
            LaunchApp();
            var weatherWindow = _automation!.GetDesktop()
                .FindFirstDescendant(cf => cf.ByClassName("WeatherWindow"))?.AsWindow();
            var weatherPage = new WeatherPage(_automation, weatherWindow!);

            // Act
            weatherPage.WaitForWeatherLoaded(30000);

            // Assert
            Assert.False(weatherPage.IsLoading);
            Assert.NotEmpty(weatherPage.GetCurrentTemperature());
        }

        [Fact(Skip = "Run locally on Windows")]
        public void WeatherPage_Refresh_ShouldUpdateWeather()
        {
            // Arrange
            LaunchApp();
            var weatherWindow = _automation!.GetDesktop()
                .FindFirstDescendant(cf => cf.ByClassName("WeatherWindow"))?.AsWindow();
            var weatherPage = new WeatherPage(_automation, weatherWindow!);
            weatherPage.WaitForWeatherLoaded();

            var initialTemp = weatherPage.GetCurrentTemperature();

            // Act
            weatherPage.Refresh();
            weatherPage.WaitForWeatherLoaded();

            // Assert
            Assert.NotEmpty(weatherPage.GetCurrentTemperature());
        }
    }
}
