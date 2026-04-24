using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;

namespace UIAutomationTests.PageObjects
{
    /// <summary>
    /// Page Object for Weather Widget
    /// </summary>
    public class WeatherPage : BasePage
    {
        public WeatherPage(AutomationBase automation, Window window) : base(automation, window) { }

        // Elements
        public Label? TemperatureLabel => FindLabel("TxtTemp");
        public Label? WeatherDescLabel => FindLabel("TxtWeather");
        public Label? CityLabel => FindLabel("TxtCity");
        public Button? RefreshButton => FindButton("BtnRefresh");
        public Label? LoadingLabel => FindLabel("TxtLoading");

        // Actions
        public void Refresh()
        {
            RefreshButton?.Click();
            System.Threading.Thread.Sleep(3000); // Wait for API
        }

        public string GetCurrentTemperature()
        {
            return TemperatureLabel?.Text ?? "";
        }

        public string GetWeatherDescription()
        {
            return WeatherDescLabel?.Text ?? "";
        }

        public bool IsLoading
        {
            get
            {
                var loading = LoadingLabel?.Text ?? "";
                return loading.Contains("加载中") || loading.Contains("Loading");
            }
        }

        public void WaitForWeatherLoaded(int timeoutMs = 30000)
        {
            var start = DateTime.Now;
            while ((DateTime.Now - start).TotalMilliseconds < timeoutMs)
            {
                if (!IsLoading) return;
                System.Threading.Thread.Sleep(500);
            }
            throw new TimeoutException("Weather did not load within timeout");
        }
    }
}
