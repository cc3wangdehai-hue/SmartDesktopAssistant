using System;
using System.Threading.Tasks;
using Xunit;

namespace SmartDesktopAssistant.Tests
{
    /// <summary>
    /// Test Weather API service
    /// </summary>
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
            // Arrange
            var cityName = "北京";

            // Act
            var result = await _service.SearchCityAsync(cityName);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Beijing", result.Name, StringComparison.OrdinalIgnoreCase);
            Assert.True(result.Latitude != 0);
            Assert.True(result.Longitude != 0);
        }

        [Fact]
        public async Task SearchCityAsync_InvalidCity_ReturnsNull()
        {
            // Arrange
            var cityName = "NonExistentCity12345";

            // Act
            var result = await _service.SearchCityAsync(cityName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetWeatherAsync_ValidCoordinates_ReturnsWeatherData()
        {
            // Arrange - Beijing coordinates
            double latitude = 39.9042;
            double longitude = 116.4074;

            // Act
            var result = await _service.GetWeatherAsync(latitude, longitude);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.MaxTemps.Length > 0);
            Assert.True(result.MinTemps.Length > 0);
            Assert.True(result.Dates.Length > 0);
        }

        [Fact]
        public async Task GetWeatherByCityAsync_ValidCity_ReturnsCityAndWeather()
        {
            // Arrange
            var cityName = "上海";

            // Act
            var (city, weather) = await _service.GetWeatherByCityAsync(cityName);

            // Assert
            Assert.NotNull(city);
            Assert.NotNull(weather);
            Assert.Contains("Shanghai", city.Name, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetWeatherByCityAsync_InvalidCity_ReturnsNulls()
        {
            // Arrange
            var cityName = "InvalidCity99999";

            // Act
            var (city, weather) = await _service.GetWeatherByCityAsync(cityName);

            // Assert
            Assert.Null(city);
            Assert.Null(weather);
        }

        [Fact]
        public void WeatherData_GetWeatherDescription_KnownCode_ReturnsCorrectDescription()
        {
            // Test known weather codes
            Assert.Equal("晴", WeatherWidget.WeatherData.GetWeatherDescription(0));
            Assert.Equal("多云", WeatherWidget.WeatherData.GetWeatherDescription(2));
            Assert.Equal("阴", WeatherWidget.WeatherData.GetWeatherDescription(3));
            Assert.Equal("雨", WeatherWidget.WeatherData.GetWeatherDescription(61));
            Assert.Equal("雪", WeatherWidget.WeatherData.GetWeatherDescription(71));
            Assert.Equal("雷暴", WeatherWidget.WeatherData.GetWeatherDescription(95));
        }

        [Fact]
        public void WeatherData_GetWeatherDescription_UnknownCode_ReturnsUnknown()
        {
            // Test unknown weather code
            Assert.Equal("未知", WeatherWidget.WeatherData.GetWeatherDescription(999));
        }
    }
}
