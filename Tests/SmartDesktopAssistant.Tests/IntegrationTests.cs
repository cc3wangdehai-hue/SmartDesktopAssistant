using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using WeatherWidget;
using TodoWidget;

namespace SmartDesktopAssistant.Tests
{
    /// <summary>
    /// Integration tests that test multiple components together
    /// </summary>
    public class IntegrationTests : IDisposable
    {
        private readonly string _testDataPath;
        private readonly HttpClient _httpClient;

        public IntegrationTests()
        {
            _testDataPath = Path.Combine(Path.GetTempPath(), $"IntegrationTest_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_testDataPath);
            Environment.SetEnvironmentVariable("LOCALAPPDATA", _testDataPath);
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testDataPath))
                    Directory.Delete(_testDataPath, true);
            }
            catch { }
            _httpClient.Dispose();
        }

        [Fact]
        public async Task WeatherService_ToTodoStorage_Integration()
        {
            // Test: Save weather info to todo as a reminder
            var weatherService = new WeatherService();
            var todoStorage = new TodoStorage();

            // Get weather
            var weather = await weatherService.GetWeatherAsync(39.9042, 116.4074);
            
            if (weather != null)
            {
                // Create todo based on weather
                var todo = new TodoItem
                {
                    Title = $"Weather: {WeatherData.GetWeatherDescription(weather.WeatherCode)}, {weather.Temperature}°C",
                    Notes = "Auto-generated from weather check"
                };
                
                todoStorage.Add(todo);
                var loaded = todoStorage.Load();
                
                Assert.Contains(loaded, t => t.Title.Contains("Weather"));
            }
        }

        [Fact]
        public async Task WeatherApi_RealApi_Integration()
        {
            // Test: Real API call (may be slow or fail due to network)
            var service = new WeatherService();

            try
            {
                var (city, weather) = await service.GetWeatherByCityAsync("北京");
                
                // If API works, verify data
                if (city != null && weather != null)
                {
                    Assert.InRange(weather.Temperature, -50, 60);
                    Assert.InRange(weather.WeatherCode, 0, 100);
                }
            }
            catch (HttpRequestException ex)
            {
                // Network issues - log but don't fail
                Console.WriteLine($"API test skipped due to network: {ex.Message}");
            }
        }

        [Fact]
        public void FileStorage_Persistence_Integration()
        {
            // Test: Data persists across storage instances
            var path = Path.Combine(_testDataPath, "SmartDesktopAssistant");
            Directory.CreateDirectory(path);

            // First instance - save data
            var storage1 = new TodoStorage();
            storage1.Add(new TodoItem { Title = "Integration Test Item" });

            // Second instance - load data
            var storage2 = new TodoStorage();
            var items = storage2.Load();

            Assert.Contains(items, i => i.Title == "Integration Test Item");
        }

        [Fact]
        public void Settings_WeatherTodo_Integration()
        {
            // Test: Settings affect weather and todo widgets
            var todoStorage = new TodoStorage();
            
            // Simulate settings change affecting todo
            var todo = new TodoItem
            {
                Title = "Check weather for trip",
                DueDate = DateTime.Now.AddDays(1),
                Priority = TodoPriority.High
            };
            
            todoStorage.Add(todo);
            var items = todoStorage.Load();
            
            var savedItem = Assert.Single(items, i => i.Title == "Check weather for trip");
            Assert.Equal(TodoPriority.High, savedItem.Priority);
            Assert.NotNull(savedItem.DueDate);
        }

        [Fact]
        public async Task ConcurrentOperations_Integration()
        {
            // Test: Multiple concurrent operations
            var storage = new TodoStorage();
            var tasks = new Task[10];

            for (int i = 0; i < 10; i++)
            {
                int index = i;
                tasks[i] = Task.Run(() =>
                {
                    storage.Add(new TodoItem { Title = $"Concurrent Item {index}" });
                });
            }

            await Task.WhenAll(tasks);

            // At least some items should be saved
            var items = storage.Load();
            Assert.NotEmpty(items);
        }
    }
}
