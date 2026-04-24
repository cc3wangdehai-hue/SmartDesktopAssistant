using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using WeatherWidget;
using TodoWidget;

namespace PerformanceTests
{
    /// <summary>
    /// Performance benchmarks for core functionality
    /// Run locally with: dotnet run -c Release
    /// </summary>
    [MemoryDiagnoser]
    public class PerformanceBenchmarks
    {
        private WeatherService? _weatherService;
        private TodoStorage? _todoStorage;

        [GlobalSetup]
        public void Setup()
        {
            _weatherService = new WeatherService();
            _todoStorage = new TodoStorage();
        }

        [Benchmark(Description = "Weather API call")]
        public async Task WeatherApiCall()
        {
            await _weatherService!.GetWeatherAsync(39.9042, 116.4074);
        }

        [Benchmark(Description = "City search")]
        public async Task CitySearch()
        {
            await _weatherService!.SearchCityAsync("北京");
        }

        [Benchmark(Description = "Todo save operation")]
        public void TodoSave()
        {
            var item = new TodoItem { Title = "Performance Test Item" };
            _todoStorage!.Add(item);
        }

        [Benchmark(Description = "Todo load operation")]
        public void TodoLoad()
        {
            _todoStorage!.Load();
        }

        [Benchmark(Description = "Weather description lookup")]
        public void WeatherDescription()
        {
            for (int i = 0; i < 100; i++)
            {
                WeatherData.GetWeatherDescription(i % 100);
            }
        }
    }

    /// <summary>
    /// Memory usage tests
    /// </summary>
    public class MemoryTests
    {
        [Fact]
        public void TodoStorage_MemoryUsage_ShouldBeReasonable()
        {
            var storage = new TodoStorage();
            var initialMemory = GC.GetTotalMemory(true);

            // Add 1000 items
            for (int i = 0; i < 1000; i++)
            {
                storage.Add(new TodoItem { Title = $"Item {i}" });
            }

            var afterMemory = GC.GetTotalMemory(true);
            var memoryIncrease = afterMemory - initialMemory;

            // Should not exceed 10MB for 1000 items
            Assert.True(memoryIncrease < 10 * 1024 * 1024, 
                $"Memory increased by {memoryIncrease / 1024}KB for 1000 items");
        }
    }
}
