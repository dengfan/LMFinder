using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using YamlDotNet.Serialization;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace LMFinder
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainViewModel MVM = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = MVM;
            ReadConfig();
        }

        private void ReadConfig()
        {
            if (!string.IsNullOrEmpty(App.YamlFilePath) && File.Exists(App.YamlFilePath))
            {
                var yamlText = File.ReadAllText(App.YamlFilePath);
                var deserializer = new Deserializer();
                MVM.TabsVmList = deserializer.Deserialize<ObservableCollection<TabViewModel>>(yamlText);
            }

            if (MVM.TabsVmList != null && MVM.TabsVmList.Count > 0)
            {
                var cvm = MVM.TabsVmList.FirstOrDefault(t => t.IsCurrent);
                if (cvm != null)
                {
                    MVM.CurrentTabViewModel = cvm;
                }
            }
            else
            {
                NewTab();
            }

            var newTab = new TabViewModel
            {
                Type = 1,
                Title = "+",
            };
            MVM.TabsVmList.Add(newTab);
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        public static void SaveConfig()
        {
            var saveDg = new System.Windows.Forms.SaveFileDialog
            {
                InitialDirectory = Directory.GetCurrentDirectory(),
                Filter = "(*.lmf)|*.lmf",
                FileName = string.Concat("untitled_", DateTime.Now.ToString("yyyyMMdd")),
                AddExtension = true,
                RestoreDirectory = true
            };

            if (saveDg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    string yamlFilePath = saveDg.FileName;
                    using (TextWriter writer = File.CreateText(yamlFilePath))
                    {
                        var serializer = new Serializer();
                        var yamlText = serializer.Serialize(MVM.TabsVmList.Where(t => t.Type == 0));
                        writer.Write(yamlText);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ResetTabCurrent()
        {
            if (MVM.TabsVmList != null && MVM.TabsVmList.Count > 0)
            {
                foreach (var item in MVM.TabsVmList)
                {
                    item.IsCurrent = false;
                }
            }
        }

        private void NewTab()
        {
            ResetTabCurrent();

            int n = MVM.TabsVmList.Count(t => t.Title.StartsWith("未命名"));
            var nvm = new TabViewModel
            {
                Title = string.Concat("未命名", n + 1),
                FileFilter = "*.*;",
                Hours = "12",
                IsCurrent = true,
            };

            var i = MVM.TabsVmList.Count - 1;
            if (i < 0) i = 0;
            MVM.TabsVmList.Insert(i, nvm);
            MVM.CurrentTabViewModel = nvm;
        }

        private void TabButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (btn.DataContext is TabViewModel vm)
                {
                    switch (vm.Type)
                    {
                        case 1: // 新建一个Tab
                            {
                                NewTab();
                            }
                            break;
                        default: // 切换Tab
                            {
                                ResetTabCurrent();
                                vm.IsCurrent = true;
                                MVM.CurrentTabViewModel = vm;
                            }
                            break;
                    }
                }
            }
        }

        private void TabButton_Rename_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem btn)
            {
                if (btn.DataContext is TabViewModel vm)
                {
                    ShowDialog1(vm);
                }
            }
        }

        private void TabButton_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem btn)
            {
                if (btn.DataContext is TabViewModel vm)
                {
                    MVM.TabsVmList.Remove(vm);

                    if (MVM.TabsVmList.Count(t => t.IsCurrent) == 0)
                    {
                        var fvm = MVM.TabsVmList.FirstOrDefault();
                        if (fvm != null)
                        {
                            fvm.IsCurrent = true;
                            MVM.CurrentTabViewModel = fvm;
                        }
                    }

                    if (MVM.TabsVmList.Count(t => t.Type == 0) == 0)
                    {
                        NewTab();
                    }
                }
            }
        }

        private void ShowDialog1(TabViewModel vm)
        {
            Dialog1.DataContext = vm;
            Dialog1.Visibility = Visibility.Visible;
            Dialog1_RenameTextBox.Text = vm.Title;
            Dialog1_RenameTextBox.SelectAll();
            Dialog1_RenameTextBox.Focus();
        }

        private void Dialog1_Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TabViewModel vm)
            {
                vm.Title = Dialog1_RenameTextBox.Text.Trim();
            }

            Dialog1_RenameTextBox.Text = string.Empty;
            Dialog1.Visibility = Visibility.Collapsed;
            Dialog1.DataContext = null;
        }

        private void Dialog1_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Dialog1_RenameTextBox.Text = string.Empty;
            Dialog1.Visibility = Visibility.Collapsed;
            Dialog1.DataContext = null;
        }

        public void ShowDialog2(string descDir)
        {
            Dialog2_DescDirTextBlock.Text = descDir;
            Dialog2.Visibility = Visibility.Visible;
        }

        private void Dialog2_Confirm_Click(object sender, RoutedEventArgs e)
        {
            var descDir = Dialog2_DescDirTextBlock.Text;
            if (!string.IsNullOrEmpty(descDir))
            {
                try
                {
                    Directory.CreateDirectory(descDir);
                    if (Directory.Exists(descDir))
                    {
                        Dialog2_DescDirTextBlock.Text = "目标目录创建成功";
                        Dialog2_ConfirmButton.Visibility = Visibility.Collapsed;
                    }
                }
                catch (Exception ex)
                {
                    Dialog2_DescDirTextBlock.Text = ex.Message;
                }
            }
        }

        private void Dialog2_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Dialog2_DescDirTextBlock.Text = string.Empty;
            Dialog2_ConfirmButton.Visibility = Visibility.Visible;
            Dialog2.Visibility = Visibility.Collapsed;
            Dialog2.DataContext = null;
        }
    }
}