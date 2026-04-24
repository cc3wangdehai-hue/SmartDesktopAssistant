using System;
using System.IO;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using UIAutomationTests.PageObjects;
using Xunit;

namespace UIAutomationTests
{
    public class PageObjectTests : IDisposable
    {
        private Application? _application;
        private AutomationBase? _automation;

        public PageObjectTests() => _automation = new UIA3Automation();
        public void Dispose() { _application?.Close(); _automation?.Dispose(); }

        private Window? LaunchAppAndGetTodoWindow()
        {
            var exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "Output", "SmartDesktopAssistant.exe");
            if (!File.Exists(exePath)) return null;
            _application = Application.Launch(exePath);
            System.Threading.Thread.Sleep(2000);
            return _automation!.GetDesktop().FindFirstDescendant(cf => cf.ByClassName("TodoWindow"))?.AsWindow();
        }

        private Window? LaunchAppAndGetWeatherWindow()
        {
            var exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "Output", "SmartDesktopAssistant.exe");
            if (!File.Exists(exePath)) return null;
            _application = Application.Launch(exePath);
            System.Threading.Thread.Sleep(2000);
            return _automation!.GetDesktop().FindFirstDescendant(cf => cf.ByClassName("WeatherWindow"))?.AsWindow();
        }

        [Fact(Skip = "Run locally on Windows")]
        public void TodoPage_AddItem_ShouldAppearInList()
        {
            var window = LaunchAppAndGetTodoWindow();
            if (window == null) return;
            var todoPage = new TodoPage(_automation!, window);
            var initialCount = todoPage.GetTodoCount();
            todoPage.AddTodoItem("Test Item");
            Assert.Equal(initialCount + 1, todoPage.GetTodoCount());
        }

        [Fact(Skip = "Run locally on Windows")]
        public void WeatherPage_OnLoad_ShouldDisplayWeather()
        {
            var window = LaunchAppAndGetWeatherWindow();
            if (window == null) return;
            var weatherPage = new WeatherPage(_automation!, window);
            weatherPage.WaitForWeatherLoaded(30000);
            Assert.False(weatherPage.IsLoading);
        }
    }
}
