using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace ElementHostTest
{
    /// <summary>
    /// 主窗体 - 演示如何在WinForms中使用ElementHost嵌入WPF控件
    /// </summary>
    public class MainForm : Form
    {
        private ElementHost _elementHost;
        private Panel _controlPanel;
        private Label _statusLabel;
        
        // 导入Windows API用于透明窗口支持
        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
        
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int LWA_ALPHA = 0x2;
        
        public MainForm()
        {
            InitializeComponent();
            InitializeElementHost();
        }
        
        private void InitializeComponent()
        {
            this.Text = "ElementHost集成测试 - 智能桌面助手";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 240, 245);
            
            // 标题标签
            var titleLabel = new Label
            {
                Text = "WPF控件嵌入WinForms测试",
                Font = new Font("Microsoft YaHei UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(titleLabel);
            
            // 说明标签
            var descLabel = new Label
            {
                Text = "下方是一个带有透明背景的WPF天气卡片控件，通过ElementHost嵌入",
                Font = new Font("Microsoft YaHei UI", 10),
                ForeColor = Color.FromArgb(100, 100, 100),
                Location = new Point(20, 55),
                AutoSize = true
            };
            this.Controls.Add(descLabel);
            
            // ElementHost容器
            _elementHost = new ElementHost
            {
                Size = new Size(350, 180),
                Location = new Point(100, 100),
                BackColor = Color.Transparent
            };
            this.Controls.Add(_elementHost);
            
            // 控制面板
            _controlPanel = new Panel
            {
                Location = new Point(20, 310),
                Size = new Size(540, 120),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            var controlTitle = new Label
            {
                Text = "控件交互测试",
                Font = new Font("Microsoft YaHei UI", 11, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            _controlPanel.Controls.Add(controlTitle);
            
            // 测试按钮1
            var btnTest1 = new Button
            {
                Text = "更新天气",
                Location = new Point(20, 45),
                Size = new Size(100, 30)
            };
            btnTest1.Click += (s, e) => UpdateWeather();
            _controlPanel.Controls.Add(btnTest1);
            
            // 测试按钮2
            var btnTest2 = new Button
            {
                Text = "切换主题",
                Location = new Point(130, 45),
                Size = new Size(100, 30)
            };
            btnTest2.Click += (s, e) => ToggleTheme();
            _controlPanel.Controls.Add(btnTest2);
            
            // 测试按钮3
            var btnTest3 = new Button
            {
                Text = "获取数据",
                Location = new Point(240, 45),
                Size = new Size(100, 30)
            };
            btnTest3.Click += (s, e) => GetWidgetData();
            _controlPanel.Controls.Add(btnTest3);
            
            // 状态标签
            _statusLabel = new Label
            {
                Text = "状态: 就绪",
                Location = new Point(20, 85),
                AutoSize = true,
                ForeColor = Color.FromArgb(60, 120, 60)
            };
            _controlPanel.Controls.Add(_statusLabel);
            
            // 说明
            var hintLabel = new Label
            {
                Text = "提示: WPF控件支持数据绑定、样式和动画效果",
                Location = new Point(20, 445),
                AutoSize = true,
                Font = new Font("Microsoft YaHei UI", 9),
                ForeColor = Color.Gray
            };
            this.Controls.Add(hintLabel);
            
            this.Controls.Add(_controlPanel);
        }
        
        private void InitializeElementHost()
        {
            // 创建WPF天气卡片控件
            var weatherCard = new WeatherCardControl();
            
            // 设置为透明背景（关键！）
            _elementHost.BackColor = Color.Transparent;
            
            // 嵌入WPF控件
            _elementHost.Child = weatherCard;
            
            _statusLabel.Text = "状态: WPF控件已加载";
            _statusLabel.ForeColor = Color.FromArgb(60, 120, 60);
        }
        
        private void UpdateWeather()
        {
            if (_elementHost.Child is WeatherCardControl card)
            {
                var random = new Random();
                var temps = new[] { 18, 22, 25, 28, 20 };
                card.UpdateTemperature(temps[random.Next(temps.Length)]);
                _statusLabel.Text = $"状态: 温度已更新为 {card.CurrentTemperature}°C";
            }
        }
        
        private void ToggleTheme()
        {
            if (_elementHost.Child is WeatherCardControl card)
            {
                card.ToggleTheme();
                _statusLabel.Text = "状态: 主题已切换";
            }
        }
        
        private void GetWidgetData()
        {
            if (_elementHost.Child is WeatherCardControl card)
            {
                var data = card.GetWidgetData();
                _statusLabel.Text = $"状态: 获取数据 - {data}";
            }
        }
        
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
