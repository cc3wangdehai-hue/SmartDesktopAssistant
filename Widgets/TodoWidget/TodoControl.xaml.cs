using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TodoWidget
{
    /// <summary>
    /// TODO Widget Control - 360 Style
    /// </summary>
    public partial class TodoControl : UserControl
    {
        private readonly TodoStorage _storage;
        private readonly ObservableCollection<TodoItem> _todoItems;
        private bool _syncEnabled = false;

        public TodoControl()
        {
            InitializeComponent();
            
            _storage = new TodoStorage();
            _todoItems = new ObservableCollection<TodoItem>();
            TodoList.ItemsSource = _todoItems;
            
            Loaded += TodoControl_Loaded;
        }

        private void TodoControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadItems();
        }

        /// <summary>
        /// Load items from storage
        /// </summary>
        public void LoadItems()
        {
            _todoItems.Clear();
            var items = _storage.Load();
            
            // Sort: uncompleted first, then by creation time
            var sorted = items
                .OrderByDescending(i => i.IsCompleted == false)
                .ThenByDescending(i => i.CreatedAt);
            
            foreach (var item in sorted)
            {
                _todoItems.Add(item);
            }
            
            UpdateEmptyState();
        }

        /// <summary>
        /// Enable/disable Lark sync
        /// </summary>
        public void SetLarkSyncEnabled(bool enabled)
        {
            _syncEnabled = enabled;
            BtnSync.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateEmptyState()
        {
            EmptyState.Visibility = _todoItems.Count == 0 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        /// <summary>
        /// Add new TODO item
        /// </summary>
        public void AddItem(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                TxtInput.Focus();
                return;
            }

            var item = new TodoItem
            {
                Title = title.Trim(),
                CreatedAt = DateTime.Now,
                Priority = TodoPriority.Medium
            };

            _storage.Add(item);
            _todoItems.Insert(0, item);
            UpdateEmptyState();
            
            TxtInput.Clear();
            TxtInput.Focus();
        }

        /// <summary>
        /// Delete TODO item
        /// </summary>
        public void DeleteItem(string id)
        {
            var item = _todoItems.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                _storage.Delete(id);
                _todoItems.Remove(item);
                UpdateEmptyState();
            }
        }

        /// <summary>
        /// Toggle TODO completed status
        /// </summary>
        public void ToggleCompleted(string id)
        {
            var item = _todoItems.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                item.IsCompleted = !item.IsCompleted;
                _storage.Update(item);
                
                // Reorder list
                if (item.IsCompleted)
                {
                    _todoItems.Remove(item);
                    _todoItems.Add(item);
                }
            }
        }

        // Event Handlers
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            AddItem(TxtInput.Text);
        }

        private void TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddItem(TxtInput.Text);
                e.Handled = true;
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string id)
            {
                DeleteItem(id);
            }
        }

        private void Todo_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag is string id)
            {
                ToggleCompleted(id);
            }
        }

        private async void Sync_Click(object sender, RoutedEventArgs e)
        {
            if (!_syncEnabled)
            {
                MessageBox.Show("飞书同步功能将在第四期实现", "提示", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // TODO: Implement Lark sync in Phase 4
            MessageBox.Show("飞书同步功能正在开发中...", "提示",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Get all TODO items
        /// </summary>
        public ObservableCollection<TodoItem> GetItems()
        {
            return _todoItems;
        }

        /// <summary>
        /// Get count of uncompleted items
        /// </summary>
        public int GetUncompletedCount()
        {
            return _todoItems.Count(i => !i.IsCompleted);
        }
    }
}
