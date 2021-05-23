using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using YamlDotNet.Serialization;

namespace LMFinder
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged 成员
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler hander = PropertyChanged;
            if (hander != null)
                hander(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public ObservableCollection<TabViewModel> TabsVmList { get; set; }

        private TabViewModel _currentTabViewModel = null;
        public TabViewModel CurrentTabViewModel
        {
            get { return _currentTabViewModel; }
            set
            {
                _currentTabViewModel = value;
                OnPropertyChanged("CurrentTabViewModel");
            }
        }
    }


    public class TabViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged 成员
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler hander = PropertyChanged;
            if (hander != null)
                hander(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _title = string.Empty;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        private string _srcDir = string.Empty;
        public string SrcDir
        {
            get { return _srcDir; }
            set
            {
                _srcDir = value;
                OnPropertyChanged("SrcDir");
            }
        }

        private string _destDir = string.Empty;
        public string DestDir
        {
            get { return _destDir; }
            set
            {
                _destDir = value;
                OnPropertyChanged("DestDir");
            }
        }

        private string _fileFilter = "*.txt;";
        public string FileFilter
        {
            get { return _fileFilter; }
            set
            {
                _fileFilter = value;
                OnPropertyChanged("FileFilter");
            }
        }

        private string _excludeFilter = string.Empty;
        public string ExcludeFilter
        {
            get { return _excludeFilter; }
            set
            {
                _excludeFilter = value;
                OnPropertyChanged("ExcludeFilter");
            }
        }

        private string _hours = "12";
        public string Hours
        {
            get { return _hours; }
            set
            {
                _hours = value;
                OnPropertyChanged("Hours");
            }
        }

        private bool _isCurrent = false;
        public bool IsCurrent
        {
            get { return _isCurrent; }
            set
            {
                _isCurrent = value;
                OnPropertyChanged("IsCurrent");
            }
        }


        private string _tipsText = string.Empty;
        [YamlIgnore]
        public string TipsText
        {
            get { return _tipsText; }
            set
            {
                _tipsText = value;
                OnPropertyChanged("TipsText");
            }
        }

        private Visibility _readMeVisibility = Visibility.Visible;
        [YamlIgnore]
        public Visibility ReadMeVisibility
        {
            get { return _readMeVisibility; }
            set
            {
                _readMeVisibility = value;
                OnPropertyChanged("ReadMeVisibility");
            }
        }

        [YamlIgnore]
        public ObservableCollection<FoundItem> LmList { get; set; } = new ObservableCollection<FoundItem>();

        private bool _searchButtonIsEnabled = true;
        [YamlIgnore]
        public bool SearchButtonIsEnabled
        {
            get { return _searchButtonIsEnabled; }
            set
            {
                _searchButtonIsEnabled = value;
                OnPropertyChanged("SearchButtonIsEnabled");
            }
        }

        private bool _selectAllButtonIsEnabled = false;
        [YamlIgnore]
        public bool SelectAllButtonIsEnabled
        {
            get { return _selectAllButtonIsEnabled; }
            set
            {
                _selectAllButtonIsEnabled = value;
                OnPropertyChanged("SelectAllButtonIsEnabled");
            }
        }

        private bool _copyButtonIsEnabled = false;
        [YamlIgnore]
        public bool CopyButtonIsEnabled
        {
            get { return _copyButtonIsEnabled; }
            set
            {
                _copyButtonIsEnabled = value;
                OnPropertyChanged("CopyButtonIsEnabled");
            }
        }

        private bool _isKeepDir = true;

        /// <summary>
        /// 是否保持目录结构
        /// </summary>
        public bool IsKeepDir
        {
            get { return _isKeepDir; }
            set
            {
                _isKeepDir = value;
                OnPropertyChanged("IsKeepDir");
            }
        }

        [YamlIgnore]
        public int Type { get; set; } = 0; // 仅用来区分是否是新增按钮
    }

    public class FoundItem
    {
        public string FilePath { get; set; }
        public DateTime LastWriteTime { get; set; }

        public string FileName
        {
            get
            {
                return FilePath.Split('\\').Last();
            }
        }

        public string LastWriteTimeStr
        {
            get
            {
                return LastWriteTime.ToString("MM/dd HH:mm:ss");
            }
        }
    }
}