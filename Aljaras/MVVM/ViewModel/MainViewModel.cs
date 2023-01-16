using Aljaras.Core;
using Aljaras.MVVM.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Policy;
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
        public string getVersion = string.Empty;
        #endregion

        #region RelayCommands
        [RelayCommand]
        void ShowActivationView()
        {
            ActivationViewModel ActivationVM = new();
            CurrentView = ActivationVM;
        }

        [RelayCommand]
        void ShowHolidaysView() 
        {
            HolidaysViewModel HolidaysVM = new();
            CurrentView = HolidaysVM; 
        }

        [RelayCommand]
        void ShowAboutMeView()
        {
            AboutMeViewModel AboutMeVM = new(); 
            CurrentView = AboutMeVM;
        }

        [RelayCommand]
        void Raya()
        {
            string url = "https://www.youtube.com/watch?v=hCQDlxgwRw4";
            Process.Start(new ProcessStartInfo() { FileName = url, UseShellExecute = true });
        }

        [RelayCommand]
        void CEDAW()
        {
            string url = "https://www.youtube.com/watch?v=NvG1EdlSiww";
            Process.Start(new ProcessStartInfo() { FileName = url, UseShellExecute = true });
        }

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
            MonitoringViewModel MonitoringVM = new();
            Global.LoadMonitoringAlarmCollectionData();
            CurrentView = MonitoringVM;
        }

        [RelayCommand]
        void ShowAlarmView() 
        {
            AlarmViewModel AlarmVM = new();
            CurrentView = AlarmVM; 
        }

        [RelayCommand]
        void ShowSettingsView() 
        {
            SettingsViewModel SettingsVM = new();
            CurrentView = SettingsVM; 
        }

        [RelayCommand]
        void ShowAboutView()
        {
            GuideViewModel GuideVM = new();
            CurrentView = GuideVM;
        }
        #endregion

        #region Functions
        public MainViewModel()
        {
            MonitoringViewModel MonitoringVM = new();
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
            if (!File.Exists(GlobalVariables.AppLocation + "Languages\\en.xml"))
            {
                AppLanguage overview = new();
                System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(AppLanguage));
                var path = GlobalVariables.AppLocation + "Languages";
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
