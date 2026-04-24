using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace WeatherWidget
{
    /// <summary>
    /// Weather Widget Control - 360 Style
    /// </summary>
    public partial class WeatherControl : UserControl
    {
        private readonly WeatherService _weatherService;
        private readonly DispatcherTimer _refreshTimer;
        private string _city1 = "北京";
        private string _city2 = "上海";
        private bool _isLoading1 = false;
        private bool _isLoading2 = false;

        // For drag functionality
        private Point _dragStartPoint;
        private bool _isDragging = false;

        public WeatherControl()
        {
            InitializeComponent();
            _weatherService = new WeatherService();
            
            // Auto refresh every 30 minutes
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(30)
            };
            _refreshTimer.Tick += async (s, e) => await RefreshAllAsync();
            
            // Initial load
            Loaded += async (s, e) =>
            {
                await RefreshAllAsync();
                _refreshTimer.Start();
            };
            
            Unloaded += (s, e) => _refreshTimer.Stop();
        }

        /// <summary>
        /// Set cities to display
        /// </summary>
        public void SetCities(string city1, string city2 = "")
        {
            _city1 = city1;
            _city2 = city2;
            TxtCity1.Text = city1;
            TxtCity2.Text = city2;
        }

        private async System.Threading.Tasks.Task RefreshAllAsync()
        {
            await RefreshCity1Async();
            if (!string.IsNullOrEmpty(_city2))
            {
                await RefreshCity2Async();
            }
        }

        /// <summary>
        /// Public method to refresh weather data
        /// </summary>
        public async void RefreshWeather()
        {
            await RefreshAllAsync();
        }

        private async System.Threading.Tasks.Task RefreshCity1Async()
        {
            if (_isLoading1) return;
            _isLoading1 = true;

            ShowLoading1(true);

            try
            {
                var (city, weather) = await _weatherService.GetWeatherByCityAsync(_city1);
                
                if (city != null && weather != null)
                {
                    UpdateUI1(city, weather);
                    ShowError1(false);
                }
                else
                {
                    ShowError1(true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"City1 Error: {ex.Message}");
                ShowError1(true);
            }
            finally
            {
                ShowLoading1(false);
                _isLoading1 = false;
            }
        }

        private async System.Threading.Tasks.Task RefreshCity2Async()
        {
            if (_isLoading2 || string.IsNullOrEmpty(_city2)) return;
            _isLoading2 = true;

            ShowLoading2(true);

            try
            {
                var (city, weather) = await _weatherService.GetWeatherByCityAsync(_city2);
                
                if (city != null && weather != null)
                {
                    UpdateUI2(city, weather);
                    ShowError2(false);
                }
                else
                {
                    ShowError2(true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"City2 Error: {ex.Message}");
                ShowError2(true);
            }
            finally
            {
                ShowLoading2(false);
                _isLoading2 = false;
            }
        }

        private void UpdateUI1(CityInfo city, WeatherData weather)
        {
            TxtCity1.Text = city.Name;
            TxtCountry1.Text = city.Country;
            TxtTemp1.Text = $"{(int)Math.Round(weather.Temperature)}°";
            TxtDesc1.Text = WeatherData.GetWeatherDescription(weather.WeatherCode);
            
            if (weather.MaxTemps.Length > 0)
            {
                TxtHigh1.Text = $"{(int)Math.Round(weather.MaxTemps[0])}°";
            }
            if (weather.MinTemps.Length > 0)
            {
                TxtLow1.Text = $"{(int)Math.Round(weather.MinTemps[0])}°";
            }
        }

        private void UpdateUI2(CityInfo city, WeatherData weather)
        {
            TxtCity2.Text = city.Name;
            TxtCountry2.Text = city.Country;
            TxtTemp2.Text = $"{(int)Math.Round(weather.Temperature)}°";
            TxtDesc2.Text = WeatherData.GetWeatherDescription(weather.WeatherCode);
            
            if (weather.MaxTemps.Length > 0)
            {
                TxtHigh2.Text = $"{(int)Math.Round(weather.MaxTemps[0])}°";
            }
            if (weather.MinTemps.Length > 0)
            {
                TxtLow2.Text = $"{(int)Math.Round(weather.MinTemps[0])}°";
            }
        }

        private void ShowLoading1(bool show)
        {
            LoadingBorder1.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowLoading2(bool show)
        {
            LoadingBorder2.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowError1(bool show)
        {
            ErrorBorder1.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowError2(bool show)
        {
            ErrorBorder2.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        // Event handlers
        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (sender == BtnRefresh1)
            {
                await RefreshCity1Async();
            }
            else if (sender == BtnRefresh2)
            {
                await RefreshCity2Async();
            }
        }

        private async void Retry_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                var parent = element.Parent as FrameworkElement;
                if (parent?.Name == "ErrorBorder1" || (parent?.Parent as FrameworkElement)?.Name == "ErrorBorder1")
                {
                    await RefreshCity1Async();
                }
                else
                {
                    await RefreshCity2Async();
                }
            }
        }

        private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Allow dragging the widget if needed
            _isDragging = false;
        }

        /// <summary>
        /// Change card background gradient
        /// </summary>
        public void SetCard1Gradient(GradientBrush brush)
        {
            Card1.Background = brush;
        }

        public void SetCard2Gradient(GradientBrush brush)
        {
            Card2.Background = brush;
        }
    }
}
