using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Input;

namespace ElementHostTest
{
    /// <summary>
    /// 天气卡片WPF控件 - 演示WPF丰富的UI能力
    /// </summary>
    public class WeatherCardControl : Grid
    {
        private Border _mainBorder;
        private TextBlock _cityText;
        private TextBlock _temperatureText;
        private TextBlock _weatherDescText;
        private TextBlock _highLowText;
        private Button _refreshButton;
        private StackPanel _forecastPanel;
        
        private bool _isDarkTheme = false;
        public int CurrentTemperature { get; private set; } = 22;
        
        // 主题颜色
        private readonly LinearGradientBrush _lightGradient = new LinearGradientBrush
        {
            StartPoint = new Point(0.5, 0),
            EndPoint = new Point(0.5, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromRgb(0, 166, 251), 0),    // 浅蓝
                new GradientStop(Color.FromRgb(0, 107, 166), 1)    // 深蓝
            }
        };
        
        private readonly LinearGradientBrush _darkGradient = new LinearGradientBrush
        {
            StartPoint = new Point(0.5, 0),
            EndPoint = new Point(0.5, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromRgb(45, 45, 48), 0),    // 深灰
                new GradientStop(Color.FromRgb(30, 30, 30), 1)     // 更深灰
            }
        };
        
        public WeatherCardControl()
        {
            InitializeControl();
        }
        
        private void InitializeControl()
        {
            // 设置透明背景
            this.Background = Brushes.Transparent;
            
            // 主容器
            _mainBorder = new Border
            {
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(20),
                Width = 320,
                Height = 160,
                Background = _lightGradient,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 15,
                    ShadowDepth = 5,
                    Opacity = 0.3,
                    Color = Colors.Black
                }
            };
            
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            // 第一行：城市名
            _cityText = new TextBlock
            {
                Text = "北京",
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 5)
            };
            Grid.SetRow(_cityText, 0);
            mainGrid.Children.Add(_cityText);
            
            // 第二行：温度和图标
            var tempPanel = new StackPanel { Orientation = Orientation.Horizontal };
            
            _temperatureText = new TextBlock
            {
                Text = "22°",
                FontSize = 48,
                FontWeight = FontWeights.Thin,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };
            tempPanel.Children.Add(_temperatureText);
            
            var weatherInfo = new StackPanel { Margin = new Thickness(15, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            
            _weatherDescText = new TextBlock
            {
                Text = "晴朗",
                FontSize = 14,
                Foreground = Brushes.White
            };
            weatherInfo.Children.Add(_weatherDescText);
            
            _highLowText = new TextBlock
            {
                Text = "最高 26° / 最低 18°",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255))
            };
            weatherInfo.Children.Add(_highLowText);
            
            tempPanel.Children.Add(weatherInfo);
            Grid.SetRow(tempPanel, 1);
            mainGrid.Children.Add(tempPanel);
            
            // 第三行：刷新按钮
            _refreshButton = new Button
            {
                Content = "刷新数据",
                Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(15, 5, 15, 5),
                Cursor = Cursors.Hand,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 10, 0, 0)
            };
            _refreshButton.Click += OnRefreshClick;
            
            // 添加悬停动画
            _refreshButton.MouseEnter += (s, e) => 
            {
                _refreshButton.Background = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255));
            };
            _refreshButton.MouseLeave += (s, e) => 
            {
                _refreshButton.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
            };
            
            Grid.SetRow(_refreshButton, 2);
            mainGrid.Children.Add(_refreshButton);
            
            // 第四行：未来预报（简化版）
            _forecastPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 15, 0, 0) };
            
            var days = new[] { "今天", "明天", "周三", "周四" };
            var icons = new[] { "☀️", "⛅", "🌧️", "☀️" };
            var temps = new[] { "22°", "24°", "20°", "26°" };
            
            for (int i = 0; i < 4; i++)
            {
                var dayPanel = new StackPanel { Margin = new Thickness(0, 0, 20, 0), HorizontalAlignment = HorizontalAlignment.Center };
                dayPanel.Children.Add(new TextBlock 
                { 
                    Text = days[i], 
                    Foreground = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)), 
                    FontSize = 11,
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                dayPanel.Children.Add(new TextBlock 
                { 
                    Text = icons[i], 
                    FontSize = 20,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 5)
                });
                dayPanel.Children.Add(new TextBlock 
                { 
                    Text = temps[i], 
                    Foreground = Brushes.White, 
                    FontSize = 12,
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                _forecastPanel.Children.Add(dayPanel);
            }
            
            Grid.SetRow(_forecastPanel, 3);
            mainGrid.Children.Add(_forecastPanel);
            
            _mainBorder.Child = mainGrid;
            this.Children.Add(_mainBorder);
        }
        
        private void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            // 简单的动画效果
            var animation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.5,
                Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                AutoReverse = true
            };
            
            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, _temperatureText);
            Storyboard.SetTargetProperty(animation, new PropertyPath(TextBlock.OpacityProperty));
            storyboard.Begin();
            
            // 模拟更新数据
            var random = new Random();
            CurrentTemperature = random.Next(15, 30);
            _temperatureText.Text = $"{CurrentTemperature}°";
            _weatherDescText.Text = CurrentTemperature > 25 ? "炎热" : CurrentTemperature > 20 ? "晴朗" : "凉爽";
        }
        
        public void UpdateTemperature(int temp)
        {
            // 带动画的温度更新
            var animation = new DoubleAnimation
            {
                From = double.Parse(_temperatureText.Text.Replace("°", "")),
                To = temp,
                Duration = new Duration(TimeSpan.FromMilliseconds(300))
            };
            
            _temperatureText.BeginAnimation(TextBlock.TextProperty, null);
            _temperatureText.Text = $"{temp}°";
            CurrentTemperature = temp;
        }
        
        public void ToggleTheme()
        {
            _isDarkTheme = !_isDarkTheme;
            
            // 平滑过渡动画
            var animation = new Duration(TimeSpan.FromMilliseconds(300));
            
            if (_isDarkTheme)
            {
                _mainBorder.Background = _darkGradient;
                _cityText.Foreground = Brushes.White;
            }
            else
            {
                _mainBorder.Background = _lightGradient;
                _cityText.Foreground = Brushes.White;
            }
        }
        
        public string GetWidgetData()
        {
            return $"城市:{_cityText.Text}, 温度:{CurrentTemperature}°, 天气:{_weatherDescText.Text}";
        }
    }
}
