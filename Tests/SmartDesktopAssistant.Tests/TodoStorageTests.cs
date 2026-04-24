using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SmartDesktopAssistant.Tests
{
    /// <summary>
    /// Test TODO storage functionality
    /// </summary>
    public class TodoStorageTests : IDisposable
    {
        private readonly string _testFolder;
        private readonly TodoWidget.TodoStorage _storage;

        public TodoStorageTests()
        {
            // Use a unique test folder for each test run
            _testFolder = Path.Combine(Path.GetTempPath(), $"TodoTest_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_testFolder);
            
            // Set environment to use test folder
            Environment.SetEnvironmentVariable("LOCALAPPDATA", _testFolder);
            _storage = new TodoWidget.TodoStorage();
        }

        public void Dispose()
        {
            // Cleanup test folder
            try
            {
                if (Directory.Exists(_testFolder))
                {
                    Directory.Delete(_testFolder, true);
                }
            }
            catch { }
        }

        [Fact]
        public void Load_EmptyStorage_ReturnsEmptyList()
        {
            // Act
            var items = _storage.Load();

            // Assert
            Assert.NotNull(items);
            Assert.Empty(items);
        }

        [Fact]
        public void Add_NewItem_StoresItem()
        {
            // Arrange
            var item = new TodoWidget.TodoItem
            {
                Title = "Test TODO",
                Priority = TodoWidget.TodoPriority.High
            };

            // Act
            _storage.Add(item);
            var loaded = _storage.Load();

            // Assert
            Assert.Single(loaded);
            Assert.Equal("Test TODO", loaded[0].Title);
            Assert.Equal(TodoWidget.TodoPriority.High, loaded[0].Priority);
        }

        [Fact]
        public void Update_ExistingItem_UpdatesItem()
        {
            // Arrange
            var item = new TodoWidget.TodoItem
            {
                Title = "Original Title",
                Priority = TodoWidget.TodoPriority.Low
            };
            _storage.Add(item);

            // Act
            item.Title = "Updated Title";
            item.Priority = TodoWidget.TodoPriority.High;
            item.IsCompleted = true;
            _storage.Update(item);
            var loaded = _storage.Load();

            // Assert
            Assert.Single(loaded);
            Assert.Equal("Updated Title", loaded[0].Title);
            Assert.Equal(TodoWidget.TodoPriority.High, loaded[0].Priority);
            Assert.True(loaded[0].IsCompleted);
        }

        [Fact]
        public void Delete_ExistingItem_RemovesItem()
        {
            // Arrange
            var item = new TodoWidget.TodoItem { Title = "To Delete" };
            _storage.Add(item);
            var id = item.Id;

            // Act
            _storage.Delete(id);
            var loaded = _storage.Load();

            // Assert
            Assert.Empty(loaded);
        }

        [Fact]
        public void Save_MultipleItems_PersistsAllItems()
        {
            // Arrange
            var items = new[]
            {
                new TodoWidget.TodoItem { Title = "Item 1", Priority = TodoWidget.TodoPriority.High },
                new TodoWidget.TodoItem { Title = "Item 2", Priority = TodoWidget.TodoPriority.Medium },
                new TodoWidget.TodoItem { Title = "Item 3", Priority = TodoWidget.TodoPriority.Low }
            }.ToList();

            // Act
            _storage.Save(items);
            var loaded = _storage.Load();

            // Assert
            Assert.Equal(3, loaded.Count);
        }

        [Fact]
        public void TodoItem_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var item = new TodoWidget.TodoItem();

            // Assert
            Assert.NotEqual(Guid.Empty, Guid.Parse(item.Id));
            Assert.Equal(string.Empty, item.Title);
            Assert.Equal(string.Empty, item.Notes);
            Assert.False(item.IsCompleted);
            Assert.Equal(TodoWidget.TodoPriority.Medium, item.Priority);
            Assert.Null(item.DueDate);
        }

        [Fact]
        public void Add_CompleteWorkflow_WorksCorrectly()
        {
            // Arrange
            var item1 = new TodoWidget.TodoItem { Title = "Task 1", Priority = TodoWidget.TodoPriority.High };
            var item2 = new TodoWidget.TodoItem { Title = "Task 2", Priority = TodoWidget.TodoPriority.Low };

            // Act - Add items
            _storage.Add(item1);
            _storage.Add(item2);
            var loaded = _storage.Load();

            // Assert - Both items exist
            Assert.Equal(2, loaded.Count);

            // Act - Complete one item
            item1.IsCompleted = true;
            _storage.Update(item1);
            loaded = _storage.Load();

            // Assert - Item is completed
            var completedItem = loaded.First(i => i.Id == item1.Id);
            Assert.True(completedItem.IsCompleted);

            // Act - Delete one item
            _storage.Delete(item2.Id);
            loaded = _storage.Load();

            // Assert - Only one item left
            Assert.Single(loaded);
            Assert.Equal(item1.Id, loaded[0].Id);
        }
    }
}
