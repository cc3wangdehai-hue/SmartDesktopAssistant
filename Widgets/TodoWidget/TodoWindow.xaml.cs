using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Newtonsoft.Json;

namespace TodoWidget
{
    /// <summary>
    /// Converters for visibility binding
    /// </summary>
    public class EmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class TodoWindow : Window
    {
        private readonly TodoStorage _storage;
        private readonly ObservableCollection<TodoItem> _todoItems;
        private readonly string _configPath;
        private TodoSettings _settings;
        private bool _syncEnabled = false;

        public TodoWindow()
        {
            InitializeComponent();
            
            _storage = new TodoStorage();
            _todoItems = new ObservableCollection<TodoItem>();
            TodoList.ItemsSource = _todoItems;
            
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var folder = Path.Combine(appDataPath, "SmartDesktopAssistant");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            _configPath = Path.Combine(folder, "todo_settings.json");
            _settings = LoadSettings();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_settings.Left >= 0 && _settings.Top >= 0)
            {
                Left = _settings.Left;
                Top = _settings.Top;
            }
            LoadItems();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _settings.Left = Left;
            _settings.Top = Top;
            SaveSettings();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        public void LoadItems()
        {
            _todoItems.Clear();
            var items = _storage.Load();
            // Sort: uncompleted first, then by priority, then by creation time
            var sorted = items
                .OrderByDescending(i => !i.IsCompleted)
                .ThenByDescending(i => i.Priority)
                .ThenByDescending(i => i.CreatedAt);
            foreach (var item in sorted)
            {
                _todoItems.Add(item);
            }
            UpdateEmptyState();
        }

        private void UpdateEmptyState()
        {
            EmptyState.Visibility = _todoItems.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            ShowAddDialog();
        }

        private void ShowAddDialog()
        {
            var dialog = new AddTodoDialog();
            dialog.Owner = this;
            
            if (dialog.ShowDialog() == true)
            {
                var item = new TodoItem
                {
                    Title = dialog.TaskTitle,
                    Notes = dialog.TaskNotes,
                    DueDate = dialog.TaskDueDate,
                    Priority = dialog.TaskPriority,
                    CreatedAt = DateTime.Now
                };

                _storage.Add(item);
                _todoItems.Insert(0, item);
                UpdateEmptyState();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string id)
            {
                var item = _todoItems.FirstOrDefault(i => i.Id == id);
                if (item != null)
                {
                    _storage.Delete(id);
                    _todoItems.Remove(item);
                    UpdateEmptyState();
                }
            }
        }

        private void Todo_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.CheckBox cb && cb.Tag is string id)
            {
                var item = _todoItems.FirstOrDefault(i => i.Id == id);
                if (item != null)
                {
                    _storage.Update(item);
                    if (item.IsCompleted)
                    {
                        // Move completed items to bottom
                        var index = _todoItems.IndexOf(item);
                        _todoItems.RemoveAt(index);
                        _todoItems.Add(item);
                    }
                }
            }
        }

        private void Sync_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement Lark sync
            MessageBox.Show("同步功能将在第四期实现", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void SetLarkSyncEnabled(bool enabled)
        {
            _syncEnabled = enabled;
            BtnSync.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
        }

        private TodoSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    return JsonConvert.DeserializeObject<TodoSettings>(json) ?? new TodoSettings();
                }
            }
            catch { }
            return new TodoSettings();
        }

        private void SaveSettings()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                File.WriteAllText(_configPath, json);
            }
            catch { }
        }
    }

    public class TodoSettings
    {
        public double Left { get; set; } = -1;
        public double Top { get; set; } = -1;
    }

    /// <summary>
    /// Add TODO dialog with extended fields
    /// </summary>
    public class AddTodoDialog : Window
    {
        public string TaskTitle { get; private set; } = string.Empty;
        public string TaskNotes { get; private set; } = string.Empty;
        public DateTime? TaskDueDate { get; private set; } = null;
        public TodoPriority TaskPriority { get; private set; } = TodoPriority.Medium;

        private TextBox _txtTitle = null!;
        private TextBox _txtNotes = null!;
        private DatePicker _dpDueDate = null!;
        private ComboBox _cmbPriority = null!;
        private CheckBox _chkNoDueDate = null!;

        public AddTodoDialog()
        {
            Title = "添加任务";
            Width = 320;
            Height = 360;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.ToolWindow;
            Background = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F5F5F5"));

            var grid = new Grid { Margin = new Thickness(20) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Title label
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Title input
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Notes label
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Notes input
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Priority label
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Priority input
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Due date
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Buttons
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Spacer

            // Title label
            var lblTitle = new Label { Content = "任务名称 *", FontWeight = FontWeights.SemiBold, Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#333")) };
            Grid.SetRow(lblTitle, 0);
            grid.Children.Add(lblTitle);

            // Title input
            _txtTitle = new TextBox { Padding = new Thickness(10, 8, 10, 8), FontSize = 13, Margin = new Thickness(0, 4, 0, 12) };
            _txtTitle.Template = CreateTextBoxTemplate();
            Grid.SetRow(_txtTitle, 1);
            grid.Children.Add(_txtTitle);

            // Notes label
            var lblNotes = new Label { Content = "备注", FontWeight = FontWeights.SemiBold, Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#333")) };
            Grid.SetRow(lblNotes, 2);
            grid.Children.Add(lblNotes);

            // Notes input
            _txtNotes = new TextBox { Padding = new Thickness(10, 8, 10, 8), FontSize = 12, Height = 50, TextWrapping = TextWrapping.Wrap, AcceptsReturn = true, Margin = new Thickness(0, 4, 0, 12), VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            _txtNotes.Template = CreateTextBoxTemplate();
            Grid.SetRow(_txtNotes, 3);
            grid.Children.Add(_txtNotes);

            // Priority label
            var lblPriority = new Label { Content = "优先级", FontWeight = FontWeights.SemiBold, Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#333")) };
            Grid.SetRow(lblPriority, 4);
            grid.Children.Add(lblPriority);

            // Priority input
            _cmbPriority = new ComboBox { Margin = new Thickness(0, 4, 0, 12), Padding = new Thickness(10, 8, 10, 8), FontSize = 12 };
            _cmbPriority.Items.Add(new ComboBoxItem { Content = "🔴 高优先级", Tag = TodoPriority.High });
            _cmbPriority.Items.Add(new ComboBoxItem { Content = "🟡 中优先级", Tag = TodoPriority.Medium, IsSelected = true });
            _cmbPriority.Items.Add(new ComboBoxItem { Content = "🟢 低优先级", Tag = TodoPriority.Low });
            Grid.SetRow(_cmbPriority, 5);
            grid.Children.Add(_cmbPriority);

            // Due date
            var dueDatePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 16) };
            
            _chkNoDueDate = new CheckBox { Content = "无截止日期", IsChecked = true, Margin = new Thickness(0, 0, 12, 0), VerticalContentAlignment = VerticalAlignment.Center };
            _chkNoDueDate.Checked += (s, e) => { _dpDueDate.IsEnabled = false; };
            _chkNoDueDate.Unchecked += (s, e) => { _dpDueDate.IsEnabled = true; };
            dueDatePanel.Children.Add(_chkNoDueDate);
            
            _dpDueDate = new DatePicker { IsEnabled = false, MinWidth = 120 };
            dueDatePanel.Children.Add(_dpDueDate);
            Grid.SetRow(dueDatePanel, 6);
            grid.Children.Add(dueDatePanel);

            // Buttons
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 8, 0, 0) };
            
            var btnCancel = new Button { Content = "取消", Width = 80, Height = 32, Margin = new Thickness(0, 0, 8, 0) };
            btnCancel.Click += (s, e) => { DialogResult = false; Close(); };
            buttonPanel.Children.Add(btnCancel);

            var btnAdd = new Button { Content = "添加", Width = 80, Height = 32, Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E53935")), Foreground = System.Windows.Media.Brushes.White, BorderThickness = new Thickness(0) };
            btnAdd.Click += BtnAdd_Click;
            buttonPanel.Children.Add(btnAdd);
            Grid.SetRow(buttonPanel, 7);
            grid.Children.Add(buttonPanel);

            Content = grid;
        }

        private ControlTemplate CreateTextBoxTemplate()
        {
            var template = new ControlTemplate(typeof(TextBox));
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, System.Windows.Media.Brushes.White);
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            border.SetValue(Border.BorderBrushProperty, new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#DDD")));

            var scrollViewer = new FrameworkElementFactory(typeof(ScrollViewer));
            scrollViewer.SetValue(ScrollViewer.NameProperty, "PART_ContentHost");
            scrollViewer.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);

            border.AppendChild(scrollViewer);
            template.VisualTree = border;

            var focusTrigger = new Trigger
            {
                Property = System.Windows.Controls.Primitives.TextBoxBase.IsFocusedProperty,
                Value = true
            };
            focusTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, 
                new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E53935"))));
            template.Triggers.Add(focusTrigger);

            return template;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtTitle.Text))
            {
                MessageBox.Show("请输入任务名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                _txtTitle.Focus();
                return;
            }

            TaskTitle = _txtTitle.Text.Trim();
            TaskNotes = _txtNotes.Text?.Trim() ?? string.Empty;
            TaskPriority = (_cmbPriority.SelectedItem as ComboBoxItem)?.Tag is TodoPriority p ? p : TodoPriority.Medium;
            TaskDueDate = (_chkNoDueDate.IsChecked == true) ? null : _dpDueDate.SelectedDate;

            DialogResult = true;
            Close();
        }
    }
}
