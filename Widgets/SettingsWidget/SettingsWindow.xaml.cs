using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace SettingsWidget
{
    public partial class SettingsWindow : Window
    {
        private readonly string _configPath;
        private AppSettings _settings;
        private bool _settingsChanged = false;
        
        // Event to notify main app of settings changes
        public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

        public SettingsWindow()
        {
            InitializeComponent();
            
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var folder = Path.Combine(appDataPath, "SmartDesktopAssistant");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            _configPath = Path.Combine(folder, "app_settings.json");
            _settings = LoadSettings();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettingsToUI();
        }

        private void LoadSettingsToUI()
        {
            TxtCity1.Text = string.IsNullOrEmpty(_settings.City1) ? "北京" : _settings.City1;
            TxtCity2.Text = string.IsNullOrEmpty(_settings.City2) ? "上海" : _settings.City2;
            
            ChkShowWeather.IsChecked = _settings.ShowWeather;
            ChkShowTodo.IsChecked = _settings.ShowTodo;
            ChkShowFiles.IsChecked = _settings.ShowFiles;
            
            // Check auto-start status
            ChkAutoStart.IsChecked = IsAutoStartEnabled();
            
            _settingsChanged = false;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_settingsChanged)
            {
                var result = MessageBox.Show(
                    "有未保存的更改，确定要关闭吗？",
                    "提示",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (_settingsChanged)
            {
                var result = MessageBox.Show(
                    "有未保存的更改，确定要关闭吗？",
                    "提示",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.No)
                    return;
            }
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            LoadSettingsToUI();
            Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Validate cities
            if (string.IsNullOrWhiteSpace(TxtCity1.Text))
            {
                MessageBox.Show("请输入城市1名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtCity1.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtCity2.Text))
            {
                MessageBox.Show("请输入城市2名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtCity2.Focus();
                return;
            }

            // Save settings
            _settings.City1 = TxtCity1.Text.Trim();
            _settings.City2 = TxtCity2.Text.Trim();
            _settings.ShowWeather = ChkShowWeather.IsChecked == true;
            _settings.ShowTodo = ChkShowTodo.IsChecked == true;
            _settings.ShowFiles = ChkShowFiles.IsChecked == true;
            
            SaveSettings();
            
            // Notify main app
            SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(_settings));
            
            _settingsChanged = false;
            Close();
        }

        private void ChkAutoStart_Changed(object sender, RoutedEventArgs e)
        {
            _settingsChanged = true;
            
            try
            {
                SetAutoStart(ChkAutoStart.IsChecked == true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"设置开机启动失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                ChkAutoStart.IsChecked = !ChkAutoStart.IsChecked;
            }
        }

        private bool IsAutoStartEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
                return key?.GetValue("SmartDesktopAssistant") != null;
            }
            catch
            {
                return false;
            }
        }

        private void SetAutoStart(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (key == null) return;

                if (enable)
                {
                    var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        key.SetValue("SmartDesktopAssistant", $"\"{exePath}\"");
                    }
                }
                else
                {
                    key.DeleteValue("SmartDesktopAssistant", false);
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw new Exception("需要管理员权限");
            }
        }

        private AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch { }
            return new AppSettings();
        }

        private void SaveSettings()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存设置失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void MarkAsChanged(object sender, RoutedEventArgs e)
        {
            _settingsChanged = true;
        }
    }

    public class AppSettings
    {
        public double Left { get; set; } = -1;
        public double Top { get; set; } = -1;
        public string City1 { get; set; } = "北京";
        public string City2 { get; set; } = "上海";
        public bool ShowWeather { get; set; } = true;
        public bool ShowTodo { get; set; } = true;
        public bool ShowFiles { get; set; } = true;
        public bool AutoStart { get; set; } = false;
    }

    public class SettingsChangedEventArgs : EventArgs
    {
        public AppSettings Settings { get; }

        public SettingsChangedEventArgs(AppSettings settings)
        {
            Settings = settings;
        }
    }
}
