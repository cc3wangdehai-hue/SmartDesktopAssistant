using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TodoWidget;
using WeatherWidget;

namespace SmartDesktopAssistant.Tests
{
    /// <summary>
    /// Test Data Generator - Generate large datasets for testing
    /// Usage: Run locally to create test data files
    /// </summary>
    public static class TestDataGenerator
    {
        private static readonly Random _random = new Random(42); // Fixed seed for reproducibility

        #region Todo Test Data

        public static List<TodoItem> GenerateTodoItems(int count = 1000)
        {
            var items = new List<TodoItem>();
            var titles = new[]
            {
                "完成报告", "参加会议", "回复邮件", "代码审查", "更新文档",
                "修复Bug", "添加功能", "优化性能", "测试部署", "客户沟通",
                "数据备份", "系统维护", "安全检查", "版本发布", "团队协作"
            };

            var notes = new[]
            {
                "重要且紧急", "可以推迟", "需要协调", "待确认", "已完成部分",
                "", "高优先级", "低优先级", "本周完成", "下月截止"
            };

            for (int i = 0; i < count; i++)
            {
                var item = new TodoItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = $"{titles[_random.Next(titles.Length)]} #{i + 1}",
                    Notes = notes[_random.Next(notes.Length)],
                    IsCompleted = _random.Next(10) < 3, // 30% completed
                    CreatedAt = DateTime.Now.AddDays(-_random.Next(30)),
                    Priority = (TodoPriority)_random.Next(3),
                    DueDate = _random.Next(10) < 5 ? DateTime.Now.AddDays(_random.Next(1, 30)) : null
                };
                items.Add(item);
            }

            return items;
        }

        public static void GenerateTodoTestData(string outputPath, int count = 1000)
        {
            var items = GenerateTodoItems(count);
            var json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(outputPath, json);
            Console.WriteLine($"Generated {count} todo items to {outputPath}");
        }

        #endregion

        #region Weather Test Data

        public static List<CityInfo> GenerateCityTestData(int count = 100)
        {
            var cities = new List<CityInfo>();
            var cityNames = new[]
            {
                "北京", "上海", "广州", "深圳", "杭州", "成都", "武汉", "西安",
                "南京", "苏州", "天津", "重庆", "青岛", "大连", "厦门", "长沙"
            };

            for (int i = 0; i < count; i++)
            {
                cities.Add(new CityInfo
                {
                    Name = $"{cityNames[i % cityNames.Length]}{(i >= cityNames.Length ? i / cityNames.Length : "")}",
                    Country = "China",
                    Latitude = 20 + _random.NextDouble() * 25, // 20-45 N
                    Longitude = 100 + _random.NextDouble() * 35, // 100-135 E
                    Timezone = "Asia/Shanghai"
                });
            }

            return cities;
        }

        public static WeatherData GenerateWeatherTestData()
        {
            return new WeatherData
            {
                Temperature = -10 + _random.NextDouble() * 45, // -10 to 35
                WeatherCode = _random.Next(0, 100),
                WindSpeed = _random.NextDouble() * 30,
                MaxTemps = GenerateArray(7, -10, 40),
                MinTemps = GenerateArray(7, -20, 30),
                DailyWeatherCodes = GenerateArray(7, 0, 99),
                Dates = GenerateDates(7)
            };
        }

        private static double[] GenerateArray(int count, double min, double max)
        {
            var arr = new double[count];
            for (int i = 0; i < count; i++)
                arr[i] = min + _random.NextDouble() * (max - min);
            return arr;
        }

        private static int[] GenerateArray(int count, int min, int max)
        {
            var arr = new int[count];
            for (int i = 0; i < count; i++)
                arr[i] = _random.Next(min, max);
            return arr;
        }

        private static string[] GenerateDates(int count)
        {
            var dates = new string[count];
            for (int i = 0; i < count; i++)
                dates[i] = DateTime.Now.AddDays(i).ToString("yyyy-MM-dd");
            return dates;
        }

        #endregion

        #region Boundary Test Data

        public static IEnumerable<object[]> GetBoundaryStrings()
        {
            yield return new object[] { "" }; // Empty
            yield return new object[] { " " }; // Single space
            yield return new object[] { "   " }; // Multiple spaces
            yield return new object[] { new string('A', 1000) }; // 1000 chars
            yield return new object[] { new string('中', 500) }; // 500 Chinese chars
            yield return new object[] { new string('\n', 100) }; // Newlines
            yield return new object[] { "<script>alert('xss')</script>" }; // XSS attempt
            yield return new object[] { "'; DROP TABLE todos; --" }; // SQL injection attempt
            yield return new object[] { "😀🎉🔥💯" }; // Emojis
            yield return new object[] { "\0\0\0" }; // Null chars
            yield return new object[] { "Test\r\n\twith\0special\tchars" }; // Mixed special
        }

        public static IEnumerable<object[]> GetBoundaryNumbers()
        {
            yield return new object[] { 0 };
            yield return new object[] { -1 };
            yield return new object[] { int.MaxValue };
            yield return new object[] { int.MinValue };
            yield return new object[] { double.MaxValue };
            yield return new object[] { double.MinValue };
            yield return new object[] { double.NaN };
            yield return new object[] { double.PositiveInfinity };
            yield return new object[] { double.NegativeInfinity };
        }

        #endregion

        #region Stress Test Data

        public static void GenerateStressTestData(string folder, int todoCount = 10000)
        {
            Directory.CreateDirectory(folder);
            
            // Large todo dataset
            GenerateTodoTestData(Path.Combine(folder, "stress_todos.json"), todoCount);
            
            // Multiple settings files
            for (int i = 0; i < 10; i++)
            {
                var settings = new
                {
                    City1 = $"City{i}",
                    City2 = $"City{i + 100}",
                    ShowWeather = i % 2 == 0,
                    ShowTodo = true,
                    ShowFiles = i % 3 == 0
                };
                File.WriteAllText(
                    Path.Combine(folder, $"settings_{i}.json"),
                    JsonSerializer.Serialize(settings)
                );
            }

            Console.WriteLine($"Stress test data generated in {folder}");
        }

        #endregion
    }
}
