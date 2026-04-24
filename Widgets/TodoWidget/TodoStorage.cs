using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace TodoWidget
{
    /// <summary>
    /// Priority levels for TODO items
    /// </summary>
    public enum TodoPriority
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    /// <summary>
    /// TODO item with extended fields
    /// </summary>
    public class TodoItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; } = null;
        public TodoPriority Priority { get; set; } = TodoPriority.Medium;
    }

    /// <summary>
    /// Local storage for TODO items - JSON file based
    /// </summary>
    public class TodoStorage
    {
        private readonly string _storagePath;
        private List<TodoItem> _items;

        public TodoStorage()
        {
            // Store in user's AppData folder
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var folder = Path.Combine(appDataPath, "SmartDesktopAssistant");
            
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            
            _storagePath = Path.Combine(folder, "todos.json");
            _items = new List<TodoItem>();
        }

        /// <summary>
        /// Load all TODO items
        /// </summary>
        public List<TodoItem> Load()
        {
            try
            {
                if (File.Exists(_storagePath))
                {
                    var json = File.ReadAllText(_storagePath);
                    _items = JsonConvert.DeserializeObject<List<TodoItem>>(json) ?? new List<TodoItem>();
                }
                else
                {
                    _items = new List<TodoItem>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load Error: {ex.Message}");
                _items = new List<TodoItem>();
            }
            
            return _items;
        }

        /// <summary>
        /// Save all TODO items
        /// </summary>
        public void Save(List<TodoItem> items)
        {
            try
            {
                _items = items;
                var json = JsonConvert.SerializeObject(items, Formatting.Indented);
                File.WriteAllText(_storagePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Save Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Add a new TODO item
        /// </summary>
        public void Add(TodoItem item)
        {
            _items.Insert(0, item);
            Save(_items);
        }

        /// <summary>
        /// Update a TODO item
        /// </summary>
        public void Update(TodoItem item)
        {
            var index = _items.FindIndex(i => i.Id == item.Id);
            if (index >= 0)
            {
                _items[index] = item;
                Save(_items);
            }
        }

        /// <summary>
        /// Delete a TODO item
        /// </summary>
        public void Delete(string id)
        {
            _items.RemoveAll(i => i.Id == id);
            Save(_items);
        }
    }
}
