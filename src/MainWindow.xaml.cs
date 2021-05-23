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

            if (MVM.TabsVmList == null)
            {
                MVM.TabsVmList = new ObservableCollection<TabViewModel>();

                var tvm1 = new TabViewModel
                {
                    Title = "未命名1",
                    FileFilter = "*.xaml;",
                    IsCurrent = true,
                };
                MVM.TabsVmList.Add(tvm1);

                var tvm2 = new TabViewModel
                {
                    Title = "未命名2",
                    FileFilter = "*.cs;",
                    Hours = "2",
                };
                MVM.TabsVmList.Add(tvm2);

                for (int i = 1; i < 20; i++)
                {
                    var tvm = new TabViewModel
                    {
                        Title = "未命名" + i,
                        FileFilter = "*.txt;",
                        Hours = i.ToString(),
                    };
                    MVM.TabsVmList.Add(tvm);
                }

                //var cvm = MVM.TabsVmList.FirstOrDefault(t => t.IsCurrent);
                //if (cvm == null)
                //{
                //    cvm = MVM.TabsVmList.First();
                //}

                MVM.CurrentTabViewModel = tvm1;
                //MainControl.DataContext = tvm1;
            }

            //var newTab = new TabViewModel
            //{
            //    Type = 1,
            //    Title = "+",
            //};
            //MVM.TabsVmList.Add(newTab);
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

        private void TabButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (btn.DataContext is TabViewModel vm)
                {
                    switch (vm.Type)
                    {
                        case 1: // 新建一个Tab

                            break;
                        default:
                            //ResetTabCurrent();
                            MVM.CurrentTabViewModel = vm;
                            //MainControl.DataContext = vm;
                            //MVM.CurrentTabViewModel.IsCurrent = true;
                            break;
                    }
                }
            }
        }
    }
}