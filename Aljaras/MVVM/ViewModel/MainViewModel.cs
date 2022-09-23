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
        public GlobalViewModel Global { get; set; } = GlobalViewModel.Instance;

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
        public GuideViewModel guideVM = new();

        [ObservableProperty]
        public ActivationViewModel activationVM = new();

        [ObservableProperty]
        public string getVersion = string.Empty;
        #endregion

        #region RelayCommands
        [RelayCommand]
        void ShowActivationView() => CurrentView = ActivationVM;

        [RelayCommand]
        void ShowHolidaysView() => CurrentView = HolidaysVM;

        [RelayCommand]
        void ShowAboutMeView() => CurrentView = AboutMeVM;

        [RelayCommand]
        static void MoveWindow() => Application.Current.MainWindow.DragMove();

        [RelayCommand]
        void ShutdownWindow()
        {
            if (Global.GetUserSettings.IsShutdownOnClose)
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
        public void ShowMonitoringView()
        { 
            Global.LoadMonitoringAlarmCollectionData();
            CurrentView = MonitoringVM;
        }

        [RelayCommand]
        void ShowAlarmView() => CurrentView = AlarmVM;

        [RelayCommand]
        void ShowSettingsView() => CurrentView = SettingsVM;

        [RelayCommand]
        void ShowAboutView() => CurrentView = GuideVM;
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
            if (!File.Exists(App.AppLocation + "Languages\\en.xml"))
            {
                AppLanguage overview = new();
                System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(AppLanguage));
                var path = App.AppLocation + "Languages";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                FileStream file = File.Create(path + "\\en.xml");
                writer.Serialize(file, overview);
                file.Close();
            }
        }
        #endregion

    }
}
