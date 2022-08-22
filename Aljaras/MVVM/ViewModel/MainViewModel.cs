using Aljaras.MVVM.Model;
using Aljaras.MVVM.View;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteDB;
using System;
using System.IO;
using System.Windows;

namespace Aljaras.MVVM.ViewModel
{
    internal partial class MainViewModel : ObservableRecipient
    {
        public GlobalViewModel Global { get; } = GlobalViewModel.Instance;

        #region Observable Properties
        [ObservableProperty]
        private object currentView = new();

        [ObservableProperty]
        public MonitoringViewModel monitoringVM = new();

        [ObservableProperty]
        public AboutMeViewModel aboutMeVM = new();

        [ObservableProperty]
        public AlarmViewModel alarmVM = new();

        [ObservableProperty]
        public HolidaysViewModel holidaysVM = new();

        [ObservableProperty]
        public SettingsViewModel settingsVM = new();

        [ObservableProperty]
        public AboutViewModel aboutVM = new();

        [ObservableProperty]
        public string getVersion = "";
        #endregion

        #region RelayCommands
        [RelayCommand]
        void ShowHolidaysView() { CurrentView = HolidaysVM; }

        [RelayCommand]
        void ShowAboutMeView() { CurrentView = AboutMeVM; }

        [RelayCommand]
        static void MoveWindow(){Application.Current.MainWindow.DragMove();}

        [RelayCommand]
        static void ShutdownWindow()
        {
            if (GlobalViewModel.Instance.GetUserSettings.IsShutdownOnClose)
                Application.Current.Shutdown();
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
            Application.Current.MainWindow.Hide();
        }

        [RelayCommand]
        static void MaximizeWindow()
        {
            if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            else Application.Current.MainWindow.WindowState = WindowState.Maximized;
        }

        [RelayCommand]
        static void MinimizeWindow(){Application.Current.MainWindow.WindowState = WindowState.Minimized;}

        [RelayCommand]
        void ShowMonitoringView()
        { 
            GlobalViewModel.Instance.LoadMonitoringAlarmCollectionData();
            GlobalViewModel.Instance.NextAlarm();
            CurrentView = MonitoringVM;
        }

        [RelayCommand]
        void ShowAlarmView(){CurrentView = AlarmVM;}

        [RelayCommand]
        void ShowSettingsView(){CurrentView = SettingsVM;}

        [RelayCommand]
        void ShowAboutView(){CurrentView = AboutVM;}
        #endregion

        #region Functions
        public MainViewModel()
        {
            CurrentView = MonitoringVM;
            Application.Current.MainWindow.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            try
            {
                GetVersion = System.Reflection.Assembly.GetEntryAssembly()!.GetName().Version!.ToString();
            }
            catch (Exception)
            {
                GetVersion = "not configured";
            }
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Languages\\en.xml"))
            {
                AppLanguage overview = new AppLanguage();
                System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(AppLanguage));
                var path = AppDomain.CurrentDomain.BaseDirectory + "Languages";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                FileStream file = File.Create(path + "\\en.xml");
                writer.Serialize(file, overview);
                file.Close();
            }
        }
        #endregion

    }
}
