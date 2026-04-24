using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace FilesWidget
{
    public partial class FilesWindow : Window
    {
        private readonly ObservableCollection<FolderItem> _folders;
        private readonly ObservableCollection<FileItem> _files;
        private readonly string _configPath;
        private FilesSettings _settings;
        private bool _showFiles = true;

        public FilesWindow()
        {
            InitializeComponent();
            
            _folders = new ObservableCollection<FolderItem>();
            _files = new ObservableCollection<FileItem>();
            FoldersList.ItemsSource = _folders;
            
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var folder = Path.Combine(appDataPath, "SmartDesktopAssistant");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            _configPath = Path.Combine(folder, "files_settings.json");
            _settings = LoadSettings();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_settings.Left >= 0 && _settings.Top >= 0)
            {
                Left = _settings.Left;
                Top = _settings.Top;
            }
            LoadFolders();
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

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                DropOverlay.Visibility = Visibility.Visible;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            DropOverlay.Visibility = Visibility.Collapsed;
            
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (paths != null)
                {
                    foreach (var path in paths)
                    {
                        AddPath(path);
                    }
                }
            }
        }

        private void AddPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                if (Directory.Exists(path))
                {
                    // It's a folder
                    if (_folders.Any(f => f.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
                    {
                        return;
                    }
                    
                    var item = new FolderItem
                    {
                        Path = path,
                        DisplayName = Path.GetFileName(path) ?? path,
                        IsFile = false
                    };
                    _folders.Add(item);
                    SaveFolders();
                }
                else if (File.Exists(path))
                {
                    // It's a file - add to same list with IsFile = true
                    if (_folders.Any(f => f.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
                    {
                        return;
                    }
                    
                    var item = new FolderItem
                    {
                        Path = path,
                        DisplayName = Path.GetFileName(path) ?? path,
                        IsFile = true
                    };
                    _folders.Add(item);
                    SaveFolders();
                }
                UpdateEmptyState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "选择要添加的文件夹",
                ShowNewFolderButton = true,
                UseDescriptionForTitle = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AddPath(dialog.SelectedPath);
            }
        }

        private void Folder_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext != null)
            {
                if (border.DataContext is FolderItem folder)
                {
                    OpenFolder(folder.Path);
                }
                else if (border.DataContext is FileItem file)
                {
                    OpenFile(file.Path);
                }
            }
        }

        private void RemoveFolder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var path = button.Tag as string;
                if (path != null)
                {
                    var folderItem = _folders.FirstOrDefault(f => f.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
                    if (folderItem != null)
                    {
                        _folders.Remove(folderItem);
                        SaveFolders();
                        UpdateEmptyState();
                        return;
                    }
                    
                    var fileItem = _files.FirstOrDefault(f => f.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
                    if (fileItem != null)
                    {
                        _files.Remove(fileItem);
                        SaveFolders();
                        UpdateEmptyState();
                    }
                }
            }
            e.Handled = true;
        }

        private static void OpenFolder(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"\"{path}\"",
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("文件夹不存在或已被移动", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法打开文件夹: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void OpenFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("文件不存在或已被移动", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法打开文件: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFolders()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    var data = JsonConvert.DeserializeObject<FilesSettings>(json);
                    if (data != null)
                    {
                        _settings = data;
                        _folders.Clear();
                        _files.Clear();
                        
                        foreach (var folder in data.Folders.Where(f => Directory.Exists(f.Path)))
                        {
                            _folders.Add(folder);
                        }
                        
                        foreach (var file in data.Files.Where(f => File.Exists(f.Path)))
                        {
                            _files.Add(file);
                        }
                    }
                }
            }
            catch { }
            UpdateEmptyState();
        }

        private void SaveFolders()
        {
            try
            {
                _settings.Folders = _folders.ToList();
                _settings.Files = _files.ToList();
                var json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                File.WriteAllText(_configPath, json);
            }
            catch { }
        }

        private FilesSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    return JsonConvert.DeserializeObject<FilesSettings>(json) ?? new FilesSettings();
                }
            }
            catch { }
            return new FilesSettings();
        }

        private void SaveSettings()
        {
            try
            {
                _settings.Folders = _folders.ToList();
                _settings.Files = _files.ToList();
                var json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                File.WriteAllText(_configPath, json);
            }
            catch { }
        }

        private void UpdateEmptyState()
        {
            EmptyState.Visibility = (_folders.Count == 0 && _files.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ShowWindow()
        {
            Show();
            Activate();
        }
    }

    public class FolderItem
    {
        public string Path { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsFile { get; set; } = false;
    }

    public class FileItem
    {
        public string Path { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsFile { get; set; } = true;
    }

    public class FilesSettings
    {
        public double Left { get; set; } = -1;
        public double Top { get; set; } = -1;
        public System.Collections.Generic.List<FolderItem> Folders { get; set; } = new();
        public System.Collections.Generic.List<FileItem> Files { get; set; } = new();
    }
}
