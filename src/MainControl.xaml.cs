using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Controls;

namespace LMFinder
{
    /// <summary>
    /// MainControl.xaml 的交互逻辑
    /// </summary>
    public partial class MainControl : UserControl
    {
        private TabViewModel _vm = null;
        private List<FoundItem> _container = new List<FoundItem>();

        public MainControl()
        {
            InitializeComponent();

            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)HandleKeyDownEvent);
            ReadMeTextBox.Text = @"版本：2.2.0
说明：找到最近改动过的文件，并复制到目标目录。
网址：https://github.com/dengfan/LMFinder";
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is TabViewModel)
            {
                _vm = DataContext as TabViewModel;
            }
        }

        private void SrcDirButton_Click(object sender, RoutedEventArgs e)
        {
            var folderDlg = new System.Windows.Forms.FolderBrowserDialog();
            folderDlg.ShowDialog();
            _vm.SrcDir = folderDlg.SelectedPath;
        }

        private void DestDirButton_Click(object sender, RoutedEventArgs e)
        {
            var folderDlg = new System.Windows.Forms.FolderBrowserDialog();
            folderDlg.ShowDialog();
            _vm.DestDir = folderDlg.SelectedPath;
        }

        /// <summary>
        /// 查找文件
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="container"></param>
        /// <param name="dispatcher"></param>
        private static void Find(TabViewModel vm, List<FoundItem> container, Dispatcher dispatcher)
        {
            if (string.IsNullOrEmpty(vm.SrcDir))
            {
                vm.TipsText = "请选择来源目录。";
                return;
            }

            if (!Directory.Exists(vm.SrcDir))
            {
                vm.TipsText = "来源目录不存在，请重新选择。";
                return;
            }

            if (vm.SrcDir.Length <= 4)
            {
                vm.TipsText = "请勿选择磁盘根目录。";
                return;
            }

            var fileFilter = vm.FileFilter.ToLower().Split(';');
            if (fileFilter.Length == 1 && (string.IsNullOrEmpty(fileFilter[0]) || string.IsNullOrWhiteSpace(fileFilter[0])))
            {
                vm.TipsText = "请输入文件类型，如 *.xml;*.html;*.txt";
                return;
            }

            // 要排除的后缀名文件
            var excludeExtFilterList = new List<string>();
            // 要排除的文件
            var excludeFileFilterList = new List<string>();
            // 要排除的目录
            var excludeDirFilterList = new List<string>();
            foreach (var item in vm.ExcludeFilter.ToLower().Split(';'))
            {
                var s = item.Trim();
                if (string.IsNullOrEmpty(s)) continue;

                if (s.StartsWith("*."))
                {
                    excludeExtFilterList.Add(s.Substring(1));
                }
                else
                {
                    s = s.Replace("/", "\\");
                    if (!s.StartsWith("\\"))
                    {
                        s = string.Concat("\\", s);
                    }

                    if (s.Contains("."))
                    {
                        excludeFileFilterList.Add(s);
                    }

                    if (!s.EndsWith("\\"))
                    {
                        s = string.Concat(s, "\\");
                    }
                    excludeDirFilterList.Add(s);
                }
            }

            container.Clear();
            vm.LmList.Clear();
            vm.TipsText = "正在搜索文件，请稍候……";
            vm.ReadMeVisibility = Visibility.Collapsed;
            vm.SelectAllButtonIsEnabled = false;
            vm.CopyButtonIsEnabled = false;
            vm.SearchButtonIsEnabled = false;

            var startTime = DateTime.Now.Ticks;
            if (int.TryParse(vm.Hours, out int hours))
            {
                var sdir = vm.SrcDir;
                Task.Factory.StartNew(() =>
                {
                    SearchFiles(sdir, sdir, fileFilter, excludeExtFilterList.ToArray(), excludeFileFilterList.ToArray(), excludeDirFilterList.ToArray(), hours, container);

                    dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
                    {
                        foreach (var item in container.OrderByDescending(i => i.LastWriteTime))
                        {
                            vm.LmList.Add(item);
                        }

                        var ts = new TimeSpan(DateTime.Now.Ticks - startTime);
                        vm.TipsText = string.Format("共找到 {0} 个文件，耗时 {1} 秒。", vm.LmList.Count, ts.TotalSeconds.ToString("f2"));
                        vm.SearchButtonIsEnabled = true;
                        vm.SelectAllButtonIsEnabled = true;
                        vm.CopyButtonIsEnabled = true;
                    }));
                });
            }
        }

        /// <summary>
        /// 递归找文件
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="path"></param>
        /// <param name="fileFilter"></param>
        /// <param name="excludeExtFilter"></param>
        /// <param name="excludeFileFilter"></param>
        /// <param name="excludeDirFilter"></param>
        /// <param name="hours"></param>
        /// <param name="listContainer"></param>
        private static void SearchFiles(string rootPath, string path, string[] fileFilter, string[] excludeExtFilter, string[] excludeFileFilter, string[] excludeDirFilter, int hours, List<FoundItem> listContainer)
        {
            var farr = Directory.EnumerateFiles(path);
            foreach (var f in farr)
            {
                var t = File.GetLastWriteTime(f);
                var ext = string.Concat("*", Path.GetExtension(f)).ToLower();
                var isAdd = t.AddHours(hours) >= DateTime.Now && (fileFilter.Contains(ext) || fileFilter.Contains("*.*"));
                if (isAdd)
                {
                    listContainer.Add(new FoundItem { FilePath = f.Replace(rootPath, string.Empty), LastWriteTime = t });
                }
            }

            var removeList = new List<FoundItem>();
            foreach (var item in listContainer)
            {
                if (excludeExtFilter.Length > 0 && excludeExtFilter.Any(a => item.FilePath.ToLower().EndsWith(a)))
                {
                    removeList.Add(item);
                    continue;
                }

                if (excludeFileFilter.Length > 0 && excludeFileFilter.Any(a => item.FilePath.ToLower().EndsWith(a)))
                {
                    removeList.Add(item);
                    continue;
                }

                if (excludeDirFilter.Length > 0 && excludeDirFilter.Any(a => item.FilePath.ToLower().Contains(a)))
                {
                    removeList.Add(item);
                    continue;
                }
            }

            foreach (var item in removeList)
            {
                listContainer.Remove(item);
            }

            var darr = Directory.EnumerateDirectories(path);
            foreach (var p in darr)
            {
                SearchFiles(rootPath, p, fileFilter, excludeExtFilter, excludeFileFilter, excludeDirFilter, hours, listContainer);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Find(_vm, _container, Dispatcher);
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.LmList.Count == 0)
            {
                _vm.TipsText = "没有找到可供提取的文件。";
                return;
            }

            if (string.IsNullOrEmpty(_vm.DestDir))
            {
                _vm.TipsText = "请选择目标目录。";
                return;
            }

            if (!Directory.Exists(_vm.DestDir))
            {
                _vm.TipsText = "目标目录不存在，请重新选择。";
                return;
            }

            if (FileListBox.SelectedItems.Count == 0)
            {
                _vm.TipsText = "请先选中您要复制转移的文件。";
                return;
            }

            _vm.TipsText = string.Empty;
            _vm.SelectAllButtonIsEnabled = false;
            _vm.CopyButtonIsEnabled = false;

            var lst = FileListBox.SelectedItems;
            var sdir = _vm.SrcDir;
            var ddir = _vm.DestDir;
            Task.Factory.StartNew(() =>
            {
                int count = 0;
                try
                {
                    foreach (FoundItem mi in lst)
                    {
                        var sourceFilePath = string.Concat(sdir, mi.FilePath);
                        if (File.Exists(sourceFilePath))
                        {
                            var destFilePath = string.Concat(ddir, mi.FilePath);
                            if (_vm.IsKeepDir == false)
                            {
                                destFilePath = Path.Combine(ddir, mi.FileName);
                            }
                            var di = Directory.GetParent(destFilePath);
                            if (di.Exists == false)
                            {
                                Directory.CreateDirectory(di.FullName);
                            }

                            File.Copy(sourceFilePath, destFilePath, true);
                            count++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _vm.TipsText = string.Format("{0} 已复制 {1} 个文件到目标目录。", DateTime.Now.ToString("HH:mm:ss"), count);
                    _vm.SelectAllButtonIsEnabled = true;
                    _vm.CopyButtonIsEnabled = true;
                }));
            });
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (FileListBox.Items.Count == 0)
            {
                return;
            }

            if (FileListBox.SelectedItems.Count == 0)
            {
                FileListBox.SelectAll();
            }
            else
            {
                FileListBox.UnselectAll();
            }
        }

        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null && int.TryParse(_vm.Hours, out int h) && int.TryParse(btn.Content.ToString().Substring(1), out int n))
            {
                h += n;
                _vm.Hours = h.ToString();
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                HoursTextBox.Focus();
                HoursTextBox.SelectionStart = 9;
            }), DispatcherPriority.ApplicationIdle);
        }

        private void MinusButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null && int.TryParse(_vm.Hours, out int h) && int.TryParse(btn.Content.ToString().Substring(1), out int n))
            {
                h -= n;
                if (h <= 0)
                {
                    h = 1;
                }
                _vm.Hours = h.ToString();
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                HoursTextBox.Focus();
                HoursTextBox.SelectionStart = 9;
            }), DispatcherPriority.ApplicationIdle);
        }

        private void ListFilterTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            _vm.LmList.Clear();

            var k = ListFilterTextBox.Text.ToLower();
            if (string.IsNullOrWhiteSpace(k))
            {
                foreach (var item in _container.OrderByDescending(i => i.LastWriteTime))
                {
                    _vm.LmList.Add(item);
                }
            }
            else
            {
                foreach (var item in _container.Where(i => i.FileName.ToLower().Contains(k)).OrderByDescending(i => i.LastWriteTime))
                {
                    _vm.LmList.Add(item);
                }
            }
        }

        private void ResetListFilterButton_Click(object sender, RoutedEventArgs e)
        {
            ListFilterTextBox.Text = string.Empty;
            _vm.LmList.Clear();
            foreach (var item in _container.OrderByDescending(i => i.LastWriteTime))
            {
                _vm.LmList.Add(item);
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                ListFilterTextBox.Focus();
            }), DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        /// <param name="vm"></param>
        //private static void SaveConfig(TabViewModel vm)
        //{
        //    var saveDg = new System.Windows.Forms.SaveFileDialog
        //    {
        //        InitialDirectory = Directory.GetCurrentDirectory(),
        //        Filter = "(*.lmf)|*.lmf",
        //        FileName = string.Concat("untitled_", DateTime.Now.ToString("yyyyMMdd")),
        //        AddExtension = true,
        //        RestoreDirectory = true
        //    };

        //    if (saveDg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        try
        //        {
        //            string iniFilePath = saveDg.FileName;
        //            File.Create(iniFilePath);

        //            OperateIniFile.WriteIniData("Settings", "SrcDir", vm.SrcDir, iniFilePath);
        //            OperateIniFile.WriteIniData("Settings", "DestDir", vm.DestDir, iniFilePath);
        //            OperateIniFile.WriteIniData("Settings", "FileFilter", vm.FileFilter, iniFilePath);
        //            OperateIniFile.WriteIniData("Settings", "ExcludeFilter", vm.ExcludeFilter, iniFilePath);
        //            OperateIniFile.WriteIniData("Settings", "Hours", vm.Hours, iniFilePath);
        //        }
        //        catch (Exception ex)
        //        {
        //            System.Windows.MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
        //        }
        //    }
        //}

        private void HandleKeyDownEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && e.Source is TextBox tb && tb != null)
            {
                FindButton.Focus();
                Find(_vm, _container, Dispatcher);
            }
            else if (e.Key == Key.S && (Keyboard.Modifiers & (ModifierKeys.Control)) == (ModifierKeys.Control))
            {
                e.Handled = true;

                //SaveConfig(_vm);
                MainWindow.SaveConfig();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            //SaveConfig(_vm);
            MainWindow.SaveConfig();
        }

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox tb && !string.IsNullOrEmpty(tb.Text.Trim()))
            {
                var p = tb.Text.Trim();
                if (Directory.Exists(p))
                {
                    System.Diagnostics.Process.Start(tb.Text.Trim());
                }
                else
                {
                    _vm.TipsText = "目录不存在";
                }
            }
        }

        private void FileListBox_Ignore_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem btn)
            {
                if (btn.DataContext is FoundItem itemVM && FileListBox.DataContext is TabViewModel vm)
                {
                    var exc = itemVM.FilePath;
                    if (vm.ExcludeFilter.EndsWith(";") == false)
                    {
                        exc = string.Concat(";", exc);
                    }

                    vm.ExcludeFilter += exc;
                }
            }
        }
    }
}
