using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SmartDesktopAssistant.Tests
{
    public class TodoStorageTests : IDisposable
    {
        private readonly string _originalLocalAppData;
        private readonly string _testFolder;
        private readonly TodoWidget.TodoStorage _storage;

        public TodoStorageTests()
        {
            // Save original environment
            _originalLocalAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA") ?? "";
            
            // Create unique test folder for each test instance
            _testFolder = Path.Combine(Path.GetTempPath(), $"TodoTest_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_testFolder);
            
            // Set test environment
            Environment.SetEnvironmentVariable("LOCALAPPDATA", _testFolder);
            _storage = new TodoWidget.TodoStorage();
        }

        public void Dispose()
        {
            // Restore original environment
            Environment.SetEnvironmentVariable("LOCALAPPDATA", _originalLocalAppData);
            
            // Cleanup test folder
            try
            {
                if (Directory.Exists(_testFolder))
                    Directory.Delete(_testFolder, true);
            }
            catch { }
        }

        [Fact]
        public void Load_EmptyStorage_ReturnsEmptyList()
        {
            var items = _storage.Load();
            Assert.NotNull(items);
            Assert.Empty(items);
        }

        [Fact]
        public void Add_NewItem_StoresItem()
        {
            var item = new TodoWidget.TodoItem
            {
                Title = "Test TODO",
                Priority = TodoWidget.TodoPriority.High
            };

            _storage.Add(item);
            var loaded = _storage.Load();

            Assert.Single(loaded);
            Assert.Equal("Test TODO", loaded[0].Title);
        }

        [Fact]
        public void Update_ExistingItem_UpdatesItem()
        {
            var item = new TodoWidget.TodoItem { Title = "Original" };
            _storage.Add(item);

            item.Title = "Updated";
            item.IsCompleted = true;
            _storage.Update(item);
            
            var loaded = _storage.Load();

            Assert.Single(loaded);
            Assert.Equal("Updated", loaded[0].Title);
            Assert.True(loaded[0].IsCompleted);
        }

        [Fact]
        public void Delete_ExistingItem_RemovesItem()
        {
            var item = new TodoWidget.TodoItem { Title = "To Delete" };
            _storage.Add(item);

            _storage.Delete(item.Id);
            var loaded = _storage.Load();

            Assert.Empty(loaded);
        }

        [Fact]
        public void Save_MultipleItems_PersistsAll()
        {
            var items = new[]
            {
                new TodoWidget.TodoItem { Title = "Item 1" },
                new TodoWidget.TodoItem { Title = "Item 2" },
                new TodoWidget.TodoItem { Title = "Item 3" }
            }.ToList();

            _storage.Save(items);
            var loaded = _storage.Load();

            Assert.Equal(3, loaded.Count);
        }

        [Fact]
        public void TodoItem_DefaultValues_AreCorrect()
        {
            var item = new TodoWidget.TodoItem();

            Assert.NotEqual(Guid.Empty, Guid.Parse(item.Id));
            Assert.Equal(string.Empty, item.Title);
            Assert.False(item.IsCompleted);
            Assert.Equal(TodoWidget.TodoPriority.Medium, item.Priority);
        }
    }
}
