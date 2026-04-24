using System;
using System.Windows;
using WeatherWidget;
using TodoWidget;
using FilesWidget;
using SettingsWidget;
using System.Drawing;
using System.Windows.Forms;

namespace SmartDesktopAssistant
{
    public partial class App : System.Windows.Application
    {
        private WeatherWindow? _weatherWindow;
        private TodoWindow? _todoWindow;
        private FilesWindow? _filesWindow;
        private SettingsWindow? _settingsWindow;
        private NotifyIcon? _notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Create system tray icon
            SetupNotifyIcon();

            // Create and show independent widget windows with error handling
            try
            {
                _weatherWindow = new WeatherWindow();
                _weatherWindow.Left = 100;
                _weatherWindow.Top = 100;
                _weatherWindow.Show();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"天气窗口初始化失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                _todoWindow = new TodoWindow();
                _todoWindow.Left = 320;
                _todoWindow.Top = 100;
                _todoWindow.Show();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"待办窗口初始化失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                _filesWindow = new FilesWindow();
                _filesWindow.Left = 100;
                _filesWindow.Top = 400;
                _filesWindow.Show();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"文件窗口初始化失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupNotifyIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Text = "智能桌面助手",
                Visible = true,
                Icon = SystemIcons.Application
            };
            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();

            // Show/Hide Weather Widget
            _notifyIcon.ContextMenuStrip.Items.Add("显示/隐藏天气", null, (s, args) =>
            {
                ToggleWindow(_weatherWindow);
            });

            // Show/Hide Todo Widget
            _notifyIcon.ContextMenuStrip.Items.Add("显示/隐藏待办", null, (s, args) =>
            {
                ToggleWindow(_todoWindow);
            });

            // Show/Hide Files Widget
            _notifyIcon.ContextMenuStrip.Items.Add("显示/隐藏文件", null, (s, args) =>
            {
                ToggleWindow(_filesWindow);
            });

            _notifyIcon.ContextMenuStrip.Items.Add("-");

            // Settings
            _notifyIcon.ContextMenuStrip.Items.Add("⚙ 设置", null, (s, args) =>
            {
                OpenSettings();
            });

            _notifyIcon.ContextMenuStrip.Items.Add("-");

            // Refresh Weather
            _notifyIcon.ContextMenuStrip.Items.Add("刷新天气", null, async (s, args) =>
            {
                if (_weatherWindow != null)
                {
                    await _weatherWindow.RefreshWeather();
                }
            });

            _notifyIcon.ContextMenuStrip.Items.Add("-");

            // Show All Windows
            _notifyIcon.ContextMenuStrip.Items.Add("显示全部窗口", null, (s, args) =>
            {
                ShowAllWindows();
            });

            // Exit
            _notifyIcon.ContextMenuStrip.Items.Add("退出", null, (s, args) =>
            {
                ExitApplication();
            });

            // Double-click to show settings
            _notifyIcon.DoubleClick += (s, args) =>
            {
                OpenSettings();
            };
        }

        private void OpenSettings()
        {
            if (_settingsWindow == null || !_settingsWindow.IsLoaded)
            {
                _settingsWindow = new SettingsWindow();
                _settingsWindow.SettingsChanged += SettingsWindow_SettingsChanged;
            }
            
            _settingsWindow.ShowDialog();
            _settingsWindow.Activate();
        }

        private void SettingsWindow_SettingsChanged(object? sender, SettingsChangedEventArgs e)
        {
            var settings = e.Settings;
            
            // Update weather cities
            if (_weatherWindow != null)
            {
                _weatherWindow.SetCities(settings.City1, settings.City2);
                _ = _weatherWindow.RefreshWeather();
            }
            
            // Toggle widget visibility
            ToggleWindowVisibility(_weatherWindow, settings.ShowWeather);
            ToggleWindowVisibility(_todoWindow, settings.ShowTodo);
            ToggleWindowVisibility(_filesWindow, settings.ShowFiles);
        }

        private void ToggleWindowVisibility(Window? window, bool shouldShow)
        {
            if (window == null) return;
            
            if (shouldShow && !window.IsVisible)
            {
                window.Show();
            }
            else if (!shouldShow && window.IsVisible)
            {
                window.Hide();
            }
        }

        private void ToggleWindow(Window? window)
        {
            if (window == null) return;

            if (window.IsVisible)
            {
                window.Hide();
            }
            else
            {
                window.Show();
                window.Activate();
            }
        }

        private void ShowAllWindows()
        {
            if (_weatherWindow != null)
            {
                _weatherWindow.Show();
                _weatherWindow.Activate();
            }
            if (_todoWindow != null)
            {
                _todoWindow.Show();
                _todoWindow.Activate();
            }
            if (_filesWindow != null)
            {
                _filesWindow.Show();
                _filesWindow.Activate();
            }
        }

        private void ExitApplication()
        {
            // Close all windows
            _weatherWindow?.Close();
            _todoWindow?.Close();
            _filesWindow?.Close();
            _settingsWindow?.Close();
            
            // Remove notify icon
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }

            // Shutdown application
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
            base.OnExit(e);
        }
    }
}
