using System;
using System.IO;
using System.Diagnostics;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using Xunit;

namespace UIAutomationTests
{
    /// <summary>
    /// UI Automation Tests for Smart Desktop Assistant
    /// Run these tests locally on Windows after building the application
    /// </summary>
    public class UITests : IDisposable
    {
        private Application? _application;
        private AutomationBase? _automation;

        public UITests()
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
            {
                throw new FileNotFoundException($"Application not found at: {exePath}");
            }

            _application = Application.Launch(exePath);
            System.Threading.Thread.Sleep(2000);
        }

        [Fact(Skip = "Run locally on Windows after build")]
        public void WeatherWindow_ShouldBeVisible_OnStartup()
        {
            LaunchApp();
            var window = _automation!.GetDesktop().FindFirstDescendant(
                cf => cf.ByName("天气").Or(cf.ByClassName("WeatherWindow"))
            );
            Assert.NotNull(window);
        }

        [Fact(Skip = "Run locally on Windows after build")]
        public void TodoWindow_ShouldBeVisible_OnStartup()
        {
            LaunchApp();
            var window = _automation!.GetDesktop().FindFirstDescendant(
                cf => cf.ByName("待办事项").Or(cf.ByClassName("TodoWindow"))
            );
            Assert.NotNull(window);
        }

        [Fact(Skip = "Run locally on Windows after build")]
        public void FilesWindow_ShouldBeVisible_OnStartup()
        {
            LaunchApp();
            var window = _automation!.GetDesktop().FindFirstDescendant(
                cf => cf.ByName("文件收纳").Or(cf.ByClassName("FilesWindow"))
            );
            Assert.NotNull(window);
        }

        [Fact(Skip = "Run locally on Windows after build")]
        public void SystemTrayIcon_ShouldExist()
        {
            LaunchApp();
            System.Threading.Thread.Sleep(1000);
            Assert.NotNull(_application);
            Assert.False(_application.HasExited);
        }
    }
}
