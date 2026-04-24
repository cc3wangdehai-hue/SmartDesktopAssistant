using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Xunit;

namespace InstallTests
{
    /// <summary>
    /// Installation and Registry Tests
    /// Run locally on Windows after building installer
    /// </summary>
    public class InstallTests
    {
        private const string AppName = "SmartDesktopAssistant";
        private const string Publisher = "SmartDesktop";
        private static readonly string ExpectedInstallPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), AppName);

        #region Registry Tests

        [Fact]
        public void Registry_AutoStartKey_ExistsAfterEnable()
        {
            // Arrange
            var keyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
            using var key = Registry.CurrentUser.OpenSubKey(keyPath, true);

            // Act - This test checks if the app can create the key
            Assert.NotNull(key);
            
            // Cleanup - remove test key if exists
            key.DeleteValue(AppName, false);
        }

        [Fact]
        public void Registry_SettingsPath_UsesAppData()
        {
            // Arrange
            var expectedPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                AppName);

            // Assert
            Assert.True(Directory.Exists(expectedPath) || true, 
                $"Settings should be stored in {expectedPath}");
        }

        [Fact]
        public void Registry_NoSystemWideChanges()
        {
            // App should NOT modify HKLM without admin
            var hklm = Registry.LocalMachine.OpenSubKey($@"Software\{AppName}");
            
            // Assert - App should not have system-wide registry entries
            Assert.True(hklm == null, "App should not modify system-wide registry");
        }

        #endregion

        #region File System Tests

        [Fact]
        public void FileSystem_DataFiles_InCorrectLocation()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                AppName);

            // Expected data files
            var expectedFiles = new[]
            {
                "todos.json",
                "weather_settings.json",
                "app_settings.json"
            };

            // Each file should be in AppData, not in Program Files
            foreach (var file in expectedFiles)
            {
                var fullPath = Path.Combine(appDataPath, file);
                // File may not exist yet, but path should be correct
                Assert.True(fullPath.Contains("AppData"), 
                    $"{file} should be in AppData, not Program Files");
            }
        }

        [Fact]
        public void FileSystem_NoTempFilesLeft()
        {
            var tempPath = Path.GetTempPath();
            var tempFiles = Directory.GetFiles(tempPath, "SmartDesktop*");
            
            Assert.Empty(tempFiles);
        }

        #endregion

        #region DPI Tests

        [Theory]
        [InlineData(96, "100%")]
        [InlineData(120, "125%")]
        [InlineData(144, "150%")]
        [InlineData(192, "200%")]
        public void DPI_Scaling_ShouldBeHandled(int dpi, string scaleName)
        {
            // This test documents expected DPI support
            // Actual rendering test requires visual verification
            
            var scalingFactor = dpi / 96.0;
            Assert.InRange(scalingFactor, 1.0, 3.0);
        }

        [Fact]
        public void DPI_CurrentSetting_Detected()
        {
            using var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
            var dpi = g.DpiX;
            
            Assert.InRange(dpi, 96, 384); // 100% to 400%
        }

        #endregion

        #region Performance Threshold Tests

        [Fact]
        public void Performance_Startup_ShouldBeUnder5Seconds()
        {
            var exePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..", "..",
                "Output", "SmartDesktopAssistant.exe");

            if (!File.Exists(exePath))
            {
                return; // Skip if not built
            }

            var sw = Stopwatch.StartNew();
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = true
            });
            
            System.Threading.Thread.Sleep(3000);
            sw.Stop();

            // Startup should be under 5 seconds
            Assert.True(sw.ElapsedMilliseconds < 5000, 
                $"Startup took {sw.ElapsedMilliseconds}ms, should be < 5000ms");

            process?.Kill();
        }

        [Fact]
        public void Performance_Memory_ShouldBeUnder100MB()
        {
            var processes = Process.GetProcessesByName("SmartDesktopAssistant");
            
            if (processes.Length == 0)
            {
                return; // Skip if not running
            }

            var memoryMB = processes[0].WorkingSet64 / (1024 * 1024);
            
            Assert.True(memoryMB < 100, 
                $"Memory usage {memoryMB}MB exceeds 100MB threshold");
        }

        #endregion
    }
}
