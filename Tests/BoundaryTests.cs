using System;
using System.Collections.Generic;
using Xunit;
using TodoWidget;
using WeatherWidget;

namespace SmartDesktopAssistant.Tests
{
    /// <summary>
    /// Boundary and Edge Case Tests
    /// Tests for unusual inputs, extreme values, and error conditions
    /// </summary>
    public class BoundaryTests
    {
        #region Todo Item Boundary Tests

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public void TodoItem_EmptyOrWhitespaceTitle_ShouldHandle(string title)
        {
            var storage = new TodoStorage();
            var item = new TodoItem { Title = title };
            
            storage.Add(item);
            var loaded = storage.Load();
            
            // Should store without crashing
            Assert.NotEmpty(loaded);
        }

        [Fact]
        public void TodoItem_VeryLongTitle_ShouldHandle()
        {
            var storage = new TodoStorage();
            var longTitle = new string('A', 10000); // 10,000 characters
            var item = new TodoItem { Title = longTitle };
            
            storage.Add(item);
            var loaded = storage.Load();
            
            Assert.Single(loaded);
            Assert.Equal(longTitle, loaded[0].Title);
        }

        [Fact]
        public void TodoItem_ChineseCharacters_ShouldHandle()
        {
            var storage = new TodoStorage();
            var item = new TodoItem { Title = "这是一个中文标题测试测试测试" };
            
            storage.Add(item);
            var loaded = storage.Load();
            
            Assert.Single(loaded);
            Assert.Contains("中文", loaded[0].Title);
        }

        [Fact]
        public void TodoItem_EmojiInTitle_ShouldHandle()
        {
            var storage = new TodoStorage();
            var item = new TodoItem { Title = "测试 Emoji 🎉🔥💯😀" };
            
            storage.Add(item);
            var loaded = storage.Load();
            
            Assert.Single(loaded);
            Assert.Contains("🎉", loaded[0].Title);
        }

        [Fact]
        public void TodoItem_SpecialCharacters_ShouldHandle()
        {
            var storage = new TodoStorage();
            var specialChars = "Test\twith\nnewlines\r\nand\ttabs\\slashes/and\"quotes";
            var item = new TodoItem { Title = specialChars };
            
            storage.Add(item);
            var loaded = storage.Load();
            
            Assert.Single(loaded);
        }

        [Fact]
        public void TodoItem_HtmlLikeContent_ShouldStoreAsIs()
        {
            var storage = new TodoStorage();
            var item = new TodoItem { 
                Title = "<script>alert('xss')</script>",
                Notes = "<b>Bold</b> & <i>italic</i>"
            };
            
            storage.Add(item);
            var loaded = storage.Load();
            
            Assert.Contains("<script>", loaded[0].Title);
            Assert.Contains("<b>", loaded[0].Notes);
        }

        [Fact]
        public void TodoItem_FutureDueDate_ShouldHandle()
        {
            var storage = new TodoStorage();
            var item = new TodoItem { 
                Title = "Future task",
                DueDate = DateTime.Now.AddYears(10) // 10 years in future
            };
            
            storage.Add(item);
            var loaded = storage.Load();
            
            Assert.NotNull(loaded[0].DueDate);
            Assert.True(loaded[0].DueDate > DateTime.Now.AddYears(5));
        }

        [Fact]
        public void TodoItem_PastDueDate_ShouldHandle()
        {
            var storage = new TodoStorage();
            var item = new TodoItem { 
                Title = "Overdue task",
                DueDate = DateTime.Now.AddDays(-100)
            };
            
            storage.Add(item);
            var loaded = storage.Load();
            
            Assert.NotNull(loaded[0].DueDate);
            Assert.True(loaded[0].DueDate < DateTime.Now);
        }

        [Fact]
        public void TodoStorage_ManyItems_ShouldPerformWell()
        {
            var storage = new TodoStorage();
            var startTime = DateTime.Now;
            
            // Add 1000 items
            for (int i = 0; i < 1000; i++)
            {
                storage.Add(new TodoItem { Title = $"Item {i}" });
            }
            
            var addDuration = (DateTime.Now - startTime).TotalMilliseconds;
            
            // Should complete in reasonable time
            Assert.True(addDuration < 5000, $"Adding 1000 items took {addDuration}ms");
            
            var loaded = storage.Load();
            Assert.Equal(1000, loaded.Count);
        }

        #endregion

        #region Weather Service Boundary Tests

        [Theory]
        [InlineData(-90)]
        [InlineData(90)]
        [InlineData(0)]
        [InlineData(45.5)]
        public void WeatherService_ValidLatitude_ShouldWork(double latitude)
        {
            var service = new WeatherService();
            // Should not throw
            var ex = Record.ExceptionAsync(() => service.GetWeatherAsync(latitude, 0));
            Assert.Null(ex.Result);
        }

        [Theory]
        [InlineData(-180)]
        [InlineData(180)]
        [InlineData(0)]
        public void WeatherService_ValidLongitude_ShouldWork(double longitude)
        {
            var service = new WeatherService();
            var ex = Record.ExceptionAsync(() => service.GetWeatherAsync(0, longitude));
            Assert.Null(ex.Result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("NonExistentCity12345xyz")]
        public async Task WeatherService_InvalidCity_ShouldReturnNull(string city)
        {
            var service = new WeatherService();
            var result = await service.SearchCityAsync(city);
            Assert.Null(result);
        }

        [Fact]
        public void WeatherDescription_AllCodes_ShouldReturnValidString()
        {
            // Test all possible weather codes (0-99)
            for (int code = 0; code < 100; code++)
            {
                var desc = WeatherData.GetWeatherDescription(code);
                Assert.NotNull(desc);
                Assert.NotEmpty(desc);
            }
        }

        #endregion

        #region DateTime Boundary Tests

        [Fact]
        public void TodoItem_MinDate_ShouldHandle()
        {
            var item = new TodoItem
            {
                Title = "Min date test",
                CreatedAt = DateTime.MinValue,
                DueDate = DateTime.MinValue
            };
            
            Assert.Equal(DateTime.MinValue, item.CreatedAt);
        }

        [Fact]
        public void TodoItem_MaxDate_ShouldHandle()
        {
            var item = new TodoItem
            {
                Title = "Max date test",
                CreatedAt = DateTime.MaxValue,
                DueDate = DateTime.MaxValue
            };
            
            Assert.Equal(DateTime.MaxValue, item.CreatedAt);
        }

        #endregion
    }
}
