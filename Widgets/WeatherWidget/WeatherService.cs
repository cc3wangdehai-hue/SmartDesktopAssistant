using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WeatherWidget
{
    /// <summary>
    /// Open-Meteo Weather API Service
    /// Free weather API: https://open-meteo.com/
    /// </summary>
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private const string GeocodingBaseUrl = "https://geocoding-api.open-meteo.com/v1/search";
        private const string WeatherBaseUrl = "https://api.open-meteo.com/v1/forecast";

        public WeatherService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Search for city by name
        /// </summary>
        public async Task<CityInfo?> SearchCityAsync(string cityName)
        {
            try
            {
                var url = $"{GeocodingBaseUrl}?name={Uri.EscapeDataString(cityName)}&count=1&language=zh";
                var response = await _httpClient.GetStringAsync(url);
                var json = JObject.Parse(response);

                var results = json["results"];
                if (results == null || !results.HasValues)
                    return null;

                var first = results[0];
                return new CityInfo
                {
                    Name = first["name"]?.ToString() ?? "",
                    Country = first["country"]?.ToString() ?? "",
                    Latitude = first["latitude"]?.Value<double>() ?? 0,
                    Longitude = first["longitude"]?.Value<double>() ?? 0,
                    Timezone = first["timezone"]?.ToString() ?? "Asia/Shanghai"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SearchCity Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get weather data for a city
        /// </summary>
        public async Task<WeatherData?> GetWeatherAsync(double latitude, double longitude, string timezone = "Asia/Shanghai")
        {
            try
            {
                var url = $"{WeatherBaseUrl}?latitude={latitude}&longitude={longitude}" +
                         $"&current=temperature_2m,weather_code,wind_speed_10m" +
                         $"&daily=temperature_2m_max,temperature_2m_min,weather_code" +
                         $"&timezone={Uri.EscapeDataString(timezone)}" +
                         $"&forecast_days=7";

                var response = await _httpClient.GetStringAsync(url);
                var json = JObject.Parse(response);

                var current = json["current"];
                var daily = json["daily"];

                if (current == null || daily == null)
                    return null;

                return new WeatherData
                {
                    Temperature = current["temperature_2m"]?.Value<double>() ?? 0,
                    WeatherCode = current["weather_code"]?.Value<int>() ?? 0,
                    WindSpeed = current["wind_speed_10m"]?.Value<double>() ?? 0,
                    MaxTemps = daily["temperature_2m_max"]?.ToObject<double[]>() ?? Array.Empty<double>(),
                    MinTemps = daily["temperature_2m_min"]?.ToObject<double[]>() ?? Array.Empty<double>(),
                    DailyWeatherCodes = daily["weather_code"]?.ToObject<int[]>() ?? Array.Empty<int>(),
                    Dates = daily["time"]?.ToObject<string[]>() ?? Array.Empty<string>()
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetWeather Error: {ex.Message}");
                return null;
            }
        }

        // Hardcoded default cities for fallback (accurate coordinates)
        private static readonly Dictionary<string, (double lat, double lon, string tz)> DefaultCities = new()
        {
            { "北京", (39.9075, 116.39723, "Asia/Shanghai") },
            { "Beijing", (39.9075, 116.39723, "Asia/Shanghai") },
            { "上海", (31.2304, 121.4737, "Asia/Shanghai") },
            { "Shanghai", (31.2304, 121.4737, "Asia/Shanghai") },
            { "Tashkent", (41.2995, 69.2401, "Asia/Tashkent") },
            { "塔什干", (41.2995, 69.2401, "Asia/Tashkent") }
        };

        /// <summary>
        /// Get weather by city name
        /// </summary>
        public async Task<(CityInfo? city, WeatherData? weather)> GetWeatherByCityAsync(string cityName)
        {
            // Try hardcoded default first
            if (DefaultCities.TryGetValue(cityName, out var coords))
            {
                var city = new CityInfo
                {
                    Name = cityName,
                    Country = cityName.Contains("北京") || cityName == "Beijing" ? "中国" : 
                              cityName.Contains("上海") || cityName == "Shanghai" ? "中国" : "Uzbekistan",
                    Latitude = coords.lat,
                    Longitude = coords.lon,
                    Timezone = coords.tz
                };
                var weather = await GetWeatherAsync(coords.lat, coords.lon, coords.tz);
                return (city, weather);
            }

            // Fallback to API search
            var searchedCity = await SearchCityAsync(cityName);
            if (searchedCity == null)
                return (null, null);

            var searchedWeather = await GetWeatherAsync(searchedCity.Latitude, searchedCity.Longitude, searchedCity.Timezone);
            return (searchedCity, searchedWeather);
        }
    }

    /// <summary>
    /// City information
    /// </summary>
    public class CityInfo
    {
        public string Name { get; set; } = "";
        public string Country { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Timezone { get; set; } = "Asia/Shanghai";
    }

    /// <summary>
    /// Weather data
    /// </summary>
    public class WeatherData
    {
        public double Temperature { get; set; }
        public int WeatherCode { get; set; }
        public double WindSpeed { get; set; }
        public double[] MaxTemps { get; set; } = Array.Empty<double>();
        public double[] MinTemps { get; set; } = Array.Empty<double>();
        public int[] DailyWeatherCodes { get; set; } = Array.Empty<int>();
        public string[] Dates { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Get weather description from WMO weather code
        /// </summary>
        public static string GetWeatherDescription(int code)
        {
            return code switch
            {
                0 => "晴",
                1 => "晴间多云",
                2 => "多云",
                3 => "阴",
                45 or 48 => "雾",
                51 or 53 or 55 => "小雨",
                56 or 57 => "冻雨",
                61 or 63 or 65 => "雨",
                66 or 67 => "雨夹雪",
                71 or 73 or 75 => "雪",
                77 => "雪粒",
                80 or 81 or 82 => "阵雨",
                85 or 86 => "阵雪",
                95 => "雷暴",
                96 or 99 => "雷暴伴冰雹",
                _ => "未知"
            };
        }

        /// <summary>
        /// Get weather icon path from code
        /// </summary>
        public static string GetWeatherIcon(int code)
        {
            return code switch
            {
                0 => "M12,3V1L8,5L12,9V7A6,6 0 0,1 18,13H16A4,4 0 0,0 12,17V15",
                1 or 2 => "M12,7A5,5 0 0,1 17,12A5,5 0 0,1 12,17A5,5 0 0,1 7,12A5,5 0 0,1 12,7M12,9A3,3 0 0,0 9,12H12V9M3,12A9,9 0 0,0 12,21L13.5,19.5C12.9,19.15 12.5,18.5 12.5,17.75A4.75,4.75 0 0,1 17.25,13H15.5A2.25,2.25 0 0,0 13.25,10.75A2.25,2.25 0 0,0 10.5,13H8.5A5,5 0 0,0 3,18A5,5 0 0,0 8,23H16A5,5 0 0,0 21,18",
                3 => "M12,18A6,6 0 0,1 6,12C6,9.5 7.5,7.5 10,7H12V5A3,3 0 0,0 6,2H8A5,5 0 0,1 13,7V9.5C15.5,10.5 17,12.5 17,15A5,5 0 0,1 12,20",
                45 or 48 => "M3,15H5A3,3 0 0,1 8,12V9A3,3 0 0,0 5,6H3A3,3 0 0,0 0,9V12A3,3 0 0,1 3,15M3,21H5A3,3 0 0,0 8,18V17A3,3 0 0,1 5,14H3A3,3 0 0,1 0,17V18A3,3 0 0,0 3,21M13,15H15A3,3 0 0,1 18,12V9A3,3 0 0,0 15,6H13A3,3 0 0,0 10,9V12A3,3 0 0,1 13,15M13,21H15A3,3 0 0,0 18,18V17A3,3 0 0,1 15,14H13A3,3 0 0,1 10,17V18A3,3 0 0,0 13,21M21,15H19A3,3 0 0,1 16,12V9A3,3 0 0,0 13,6H11A3,3 0 0,0 8,9V12A3,3 0 0,1 11,15H13V17H16A3,3 0 0,1 19,20H21V18A3,3 0 0,1 24,15M21,21H19A3,3 0 0,0 16,18V17A3,3 0 0,1 19,14H21A3,3 0 0,1 24,17V18A3,3 0 0,0 21,21",
                51 or 53 or 55 or 61 or 63 or 65 or 80 or 81 or 82 => "M12,2A9,9 0 0,0 3,11C3,14.03 4.53,16.82 7,18.47V22H9V18.47C11.47,16.82 13,14.03 13,11A9,9 0 0,0 12,2M9,8A2,2 0 0,1 11,10A2,2 0 0,1 9,12A2,2 0 0,1 7,10A2,2 0 0,1 9,8M15,8A2,2 0 0,1 17,10A2,2 0 0,1 15,12A2,2 0 0,1 13,10A2,2 0 0,1 15,8M12,14L8,18H16L12,14Z",
                71 or 73 or 75 or 77 or 85 or 86 => "M3,15H5A3,3 0 0,1 8,12V9A3,3 0 0,0 5,6H3A3,3 0 0,0 0,9V12A3,3 0 0,1 3,15M3,21H5A3,3 0 0,0 8,18V17A3,3 0 0,1 5,14H3A3,3 0 0,1 0,17V18A3,3 0 0,0 3,21M13,15H15A3,3 0 0,1 18,12V9A3,3 0 0,0 15,6H13A3,3 0 0,0 10,9V12A3,3 0 0,1 13,15M13,21H15A3,3 0 0,0 18,18V17A3,3 0 0,1 15,14H13A3,3 0 0,1 10,17V18A3,3 0 0,0 13,21M21,15H19A3,3 0 0,1 16,12V9A3,3 0 0,0 13,6H11A3,3 0 0,0 8,9V12A3,3 0 0,1 11,15H13V17H16A3,3 0 0,1 19,20H21V18A3,3 0 0,1 24,15M21,21H19A3,3 0 0,0 16,18V17A3,3 0 0,1 19,14H21A3,3 0 0,1 24,17V18A3,3 0 0,0 21,21",
                95 or 96 or 99 => "M17,18A5,5 0 0,1 12,23A5,5 0 0,1 7,18C7,15.5 9,14 12,14C15,14 17,15.5 17,18M12,6A1,1 0 0,0 11,7A1,1 0 0,0 12,8A1,1 0 0,0 13,7A1,1 0 0,0 12,6M17,12A1,1 0 0,0 16,13A1,1 0 0,0 17,14A1,1 0 0,0 18,13A1,1 0 0,0 17,12M7,12A1,1 0 0,0 6,13A1,1 0 0,0 7,14A1,1 0 0,0 8,13A1,1 0 0,0 7,12Z",
                _ => "M12,18A6,6 0 0,1 6,12C6,9.5 7.5,7.5 10,7H12V5A3,3 0 0,0 6,2H8A5,5 0 0,1 13,7V9.5C15.5,10.5 17,12.5 17,15A5,5 0 0,1 12,20"
            };
        }
    }
}
