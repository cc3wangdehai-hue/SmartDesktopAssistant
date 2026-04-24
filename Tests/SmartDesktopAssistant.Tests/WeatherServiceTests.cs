using System;
using System.Threading.Tasks;
using Xunit;

namespace SmartDesktopAssistant.Tests
{
    public class WeatherServiceTests
    {
        private readonly WeatherWidget.WeatherService _service;

        public WeatherServiceTests()
        {
            _service = new WeatherWidget.WeatherService();
        }

        [Fact]
        public async Task SearchCityAsync_ValidCity_ReturnsCityInfo()
        {
            var result = await _service.SearchCityAsync("北京");
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.Name));
            Assert.True(result.Latitude != 0);
            Assert.True(result.Longitude != 0);
        }

        [Fact]
        public async Task SearchCityAsync_InvalidCity_ReturnsNull()
        {
            var result = await _service.SearchCityAsync("NonExistentCity12345");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetWeatherAsync_ValidCoordinates_ReturnsWeatherData()
        {
            var result = await _service.GetWeatherAsync(39.9042, 116.4074);
            Assert.NotNull(result);
            Assert.True(result.MaxTemps.Length > 0);
            Assert.True(result.MinTemps.Length > 0);
        }

        [Fact]
        public async Task GetWeatherByCityAsync_ValidCity_ReturnsCityAndWeather()
        {
            var (city, weather) = await _service.GetWeatherByCityAsync("上海");
            Assert.NotNull(city);
            Assert.NotNull(weather);
        }

        [Fact]
        public async Task GetWeatherByCityAsync_InvalidCity_ReturnsNulls()
        {
            var (city, weather) = await _service.GetWeatherByCityAsync("InvalidCity99999");
            Assert.Null(city);
            Assert.Null(weather);
        }

        [Theory]
        [InlineData(0, "晴")]
        [InlineData(2, "多云")]
        [InlineData(3, "阴")]
        [InlineData(61, "雨")]
        [InlineData(71, "雪")]
        [InlineData(95, "雷暴")]
        public void WeatherData_GetWeatherDescription_KnownCode_ReturnsCorrect(int code, string expected)
        {
            Assert.Equal(expected, WeatherWidget.WeatherData.GetWeatherDescription(code));
        }

        [Fact]
        public void WeatherData_GetWeatherDescription_UnknownCode_ReturnsUnknown()
        {
            Assert.Equal("未知", WeatherWidget.WeatherData.GetWeatherDescription(999));
        }
    }
}
