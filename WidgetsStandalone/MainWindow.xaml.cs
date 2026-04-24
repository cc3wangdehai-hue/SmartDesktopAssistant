using System.Windows;
using WeatherWidget;
using TodoWidget;

namespace WidgetsStandalone
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Access controls via XAML x:Name
            WeatherCtrl.SetCities("北京", "上海");
            TodoCtrl.LoadItems();
        }
    }
}
