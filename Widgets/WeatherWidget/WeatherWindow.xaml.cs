using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace WeatherWidget
{
    public partial class WeatherWindow : Window
    {
        private readonly WeatherService _weatherService;
        private readonly DispatcherTimer _timer;
        private readonly string _configPath;
        private WeatherSettings _settings;
        private string _city1 = "北京";
        private string _city2 = "上海";
        private bool _isLoading = false;
        private bool _isUpdating = false; // Prevent re-entry

        public WeatherWindow()
        {
            InitializeComponent();
            
            _weatherService = new WeatherService();
            
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(60)  // Changed from 1s to 60s to reduce CPU usage
            };
            _timer.Tick += Timer_Tick;
            
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var folder = Path.Combine(appDataPath, "SmartDesktopAssistant");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            _configPath = Path.Combine(folder, "weather_settings.json");
            _settings = LoadSettings();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_settings.Left >= 0 && _settings.Top >= 0)
            {
                Left = _settings.Left;
                Top = _settings.Top;
            }
            _city1 = string.IsNullOrEmpty(_settings.City1) ? "北京" : _settings.City1;
            _city2 = string.IsNullOrEmpty(_settings.City2) ? "上海" : _settings.City2;
            
            UpdateTime();
            _timer.Start();
            
            // Add delay to ensure UI is ready
            await System.Threading.Tasks.Task.Delay(100);
            _ = RefreshWeather();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _timer.Stop();
            _settings.Left = Left;
            _settings.Top = Top;
            _settings.City1 = _city1;
            _settings.City2 = _city2;
            SaveSettings();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            _ = RefreshWeather();
        }

        private async void Retry_Click(object sender, MouseButtonEventArgs e)
        {
            await RefreshWeather();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            UpdateTime();
        }

        private void UpdateTime()
        {
            var now = DateTime.Now;
            TxtTime.Text = now.ToString("HH:mm");
            TxtDate.Text = $"{now.Month}/{now.Day} {GetWeekdayText(now.DayOfWeek)}";
        }

        private static string GetWeekdayText(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Sunday => "星期日",
                DayOfWeek.Monday => "星期一",
                DayOfWeek.Tuesday => "星期二",
                DayOfWeek.Wednesday => "星期三",
                DayOfWeek.Thursday => "星期四",
                DayOfWeek.Friday => "星期五",
                DayOfWeek.Saturday => "星期六",
                _ => "星期日"
            };
        }

        public async System.Threading.Tasks.Task RefreshWeather()
        {
            // Prevent re-entry
            if (_isLoading || _isUpdating) return;
            _isLoading = true;
            _isUpdating = true;
            LoadingBorder.Visibility = Visibility.Visible;
            ErrorBorder.Visibility = Visibility.Collapsed;

            try
            {
                var (city1, weather1) = await _weatherService.GetWeatherByCityAsync(_city1);
                if (city1 != null && weather1 != null)
                {
                    TxtCity1.Text = city1.Name;
                    TxtTemp1.Text = $"{(int)Math.Round(weather1.Temperature)}°";
                    TxtDesc1.Text = WeatherData.GetWeatherDescription(weather1.WeatherCode);
                    if (weather1.MaxTemps.Length > 0) TxtHigh1.Text = $"{(int)Math.Round(weather1.MaxTemps[0])}°";
                    if (weather1.MinTemps.Length > 0) TxtLow1.Text = $"{(int)Math.Round(weather1.MinTemps[0])}°";
                }
                else
                {
                    TxtCity1.Text = _city1 + " (未找到)";
                }

                var (city2, weather2) = await _weatherService.GetWeatherByCityAsync(_city2);
                if (city2 != null && weather2 != null)
                {
                    TxtCity2.Text = city2.Name;
                    TxtTemp2.Text = $"{(int)Math.Round(weather2.Temperature)}°";
                    TxtDesc2.Text = WeatherData.GetWeatherDescription(weather2.WeatherCode);
                    if (weather2.MaxTemps.Length > 0) TxtHigh2.Text = $"{(int)Math.Round(weather2.MaxTemps[0])}°";
                    if (weather2.MinTemps.Length > 0) TxtLow2.Text = $"{(int)Math.Round(weather2.MinTemps[0])}°";
                }
                else
                {
                    TxtCity2.Text = _city2 + " (未找到)";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RefreshWeather Error: {ex.Message}");
                ErrorBorder.Visibility = Visibility.Visible;
                // 显示错误信息
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner: {ex.InnerException.Message}");
                }
            }
            finally
            {
                LoadingBorder.Visibility = Visibility.Collapsed;
                _isLoading = false;
                _isUpdating = false;
            }
        }

        public void SetCities(string city1, string city2)
        {
            _city1 = city1;
            _city2 = city2;
        }

        public string GetCity1() => _city1;
        public string GetCity2() => _city2;

        private WeatherSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    return JsonConvert.DeserializeObject<WeatherSettings>(json) ?? new WeatherSettings();
                }
            }
            catch { }
            return new WeatherSettings();
        }

        private void SaveSettings()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                File.WriteAllText(_configPath, json);
            }
            catch { }
        }
    }

    public class WeatherSettings
    {
        public double Left { get; set; } = -1;
        public double Top { get; set; } = -1;
        public string City1 { get; set; } = "";
        public string City2 { get; set; } = "";
    }
}
