using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace ClockWidget
{
    public partial class ClockWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private readonly string _configPath;
        private ClockSettings _settings;

        public ClockWindow()
        {
            InitializeComponent();
            
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var folder = Path.Combine(appDataPath, "SmartDesktopAssistant");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            _configPath = Path.Combine(folder, "clock_settings.json");
            _settings = LoadSettings();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_settings.Left >= 0 && _settings.Top >= 0)
            {
                Left = _settings.Left;
                Top = _settings.Top;
            }
            UpdateTime();
            _timer.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _timer.Stop();
            _settings.Left = Left;
            _settings.Top = Top;
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

        private ClockSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    return JsonConvert.DeserializeObject<ClockSettings>(json) ?? new ClockSettings();
                }
            }
            catch { }
            return new ClockSettings();
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

        public void ShowTime()
        {
            Show();
            Activate();
        }
    }

    public class ClockSettings
    {
        public double Left { get; set; } = -1;
        public double Top { get; set; } = -1;
    }
}
