using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;

namespace LMFinder
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static string YamlFilePath = string.Empty;

        private static void WriteReg()
        {
            var fileTypeRegInfo = new FileTypeRegInfo(".lmf")
            {
                Description = "LMFinder 配置文件",
                ExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LMFinder.exe"),
                ExtendName = ".lmf",
                IconPath = string.Empty
            };

            // 注册
            FileTypeRegister.RegisterFileType(fileTypeRegInfo);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            WriteReg();

            string[] arguments = e.Args;
            if (arguments.Length > 0 && arguments[0].EndsWith(".lmf"))
            {
                // 得到被双击文件的路径
                YamlFilePath = arguments[0];
            }
        }
    }
}
