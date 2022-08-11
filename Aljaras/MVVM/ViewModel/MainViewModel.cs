using Aljaras.MVVM.Model;
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
        public AlarmViewModel alarmVM = new();

        [ObservableProperty]
        public SettingsViewModel settingsVM = new();

        [ObservableProperty]
        public AboutViewModel aboutVM = new();

        [ObservableProperty]
        public string getVersion = "";
        #endregion

        #region RelayCommands
        [RelayCommand]
        static void MoveWindow(){Application.Current.MainWindow.DragMove();}

        [RelayCommand]
        static void ShutdownWindow(){Application.Current.Shutdown();}

        [RelayCommand]
        static void MaximizeWindow()
        {
            if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            else
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
        }

        [RelayCommand]
        static void MinimizeWindow(){Application.Current.MainWindow.WindowState = WindowState.Minimized;}

        [RelayCommand]
        void ShowMonitoringView(){CurrentView = MonitoringVM;}

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
                //// get deployment version
                GetVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            }
            catch (Exception)
            {
                //// you cannot read publish version when app isn't installed 
                //// (e.g. during debug)
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
