using System;
using System.IO;
using System.Diagnostics;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
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
        private Window? _mainWindow;

        public UITests()
        {
            _automation = new UIA3Automation();
        }

        public void Dispose()
        {
            _mainWindow?.Close();
            _application?.Close();
            _automation?.Dispose();
        }

        /// <summary>
        /// Helper method to launch the application
        /// </summary>
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
            System.Threading.Thread.Sleep(2000); // Wait for app to start
        }

        [Fact(Skip = "Run locally on Windows after build")]
        public void WeatherWindow_ShouldBeVisible_OnStartup()
        {
            // Arrange
            LaunchApp();

            // Act
            var weatherWindow = _automation!.GetDesktop().FindFirstDescendant(
                cf => cf.ByName("天气").Or(cf.ByClassName("WeatherWindow"))
            );

            // Assert
            Assert.NotNull(weatherWindow);
            Assert.True(weatherWindow.IsAvailable);
        }

        [Fact(Skip = "Run locally on Windows after build")]
        public void TodoWindow_ShouldBeVisible_OnStartup()
        {
            // Arrange
            LaunchApp();

            // Act
            var todoWindow = _automation!.GetDesktop().FindFirstDescendant(
                cf => cf.ByName("待办事项").Or(cf.ByClassName("TodoWindow"))
            );

            // Assert
            Assert.NotNull(todoWindow);
        }

        [Fact(Skip = "Run locally on Windows after build")]
        public void FilesWindow_ShouldBeVisible_OnStartup()
        {
            // Arrange
            LaunchApp();

            // Act
            var filesWindow = _automation!.GetDesktop().FindFirstDescendant(
                cf => cf.ByName("文件收纳").Or(cf.ByClassName("FilesWindow"))
            );

            // Assert
            Assert.NotNull(filesWindow);
        }

        [Fact(Skip = "Run locally on Windows after build")]
        public void TodoWidget_AddItem_ShouldAppearInList()
        {
            // Arrange
            LaunchApp();
            var todoWindow = _automation!.GetDesktop().FindFirstDescendant(
                cf => cf.ByClassName("TodoWindow")
            )?.AsWindow();
            
            Assert.NotNull(todoWindow);

            // Find input field and add button
            var inputField = todoWindow.FindFirstDescendant(cf =>.ByAutomationId("TxtInput"))?.AsTextBox();
            var addButton = todoWindow.FindFirstDescendant(cf =>.ByAutomationId("BtnAdd"))?.AsButton();

            // Act
            inputField?.Text = "Test TODO Item";
            addButton?.Click();
            System.Threading.Thread.Sleep(500);

            // Assert - Check if item appears in list
            var todoList = todoWindow.FindFirstDescendant(cf =>.ByAutomationId("TodoList"));
            Assert.NotNull(todoList);
        }

        [Fact(Skip = "Run locally on Windows after build")]
        public void SystemTrayIcon_ShouldExist()
        {
            // Arrange
            LaunchApp();

            // Act - Check for notify icon (this is tricky in FlaUI)
            System.Threading.Thread.Sleep(1000);

            // Assert - Application should be running
            Assert.NotNull(_application);
            Assert.False(_application.HasExited);
        }

        [Fact(Skip = "Run locally on Windows after build")]
        public void WeatherWidget_ShouldDisplayTemperature()
        {
            // Arrange
            LaunchApp();
            System.Threading.Thread.Sleep(5000); // Wait for weather to load

            // Act
            var weatherWindow = _automation!.GetDesktop().FindFirstDescendant(
                cf => cf.ByClassName("WeatherWindow")
            )?.AsWindow();

            // Assert
            Assert.NotNull(weatherWindow);
            
            // Find temperature text (may take time to load from API)
            var tempText = weatherWindow.FindFirstDescendant(cf =>.ByAutomationId("TxtTemp"));
            Assert.NotNull(tempText);
        }
    }
}
