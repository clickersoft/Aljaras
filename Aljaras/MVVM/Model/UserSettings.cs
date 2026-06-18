using Aljaras.MVVM.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Aljaras.MVVM.Model
{
    public partial class UserSettings : ObservableObject
    {
        [ObservableProperty]
        private int id = 1;

        [ObservableProperty]
        private string currentLang = "en";

        [ObservableProperty]
        private string emergencyAudioFileLocation = string.Empty;

        [ObservableProperty]
        private bool repeatEmergency = true;

        [ObservableProperty]
        private bool setRegistryKey = false;

        [ObservableProperty]
        private bool isShutdownOnClose = false;

        [ObservableProperty]
        private bool isFirstTimeLaunch = true;

        [ObservableProperty]
        private bool autoBackup = false;

        [ObservableProperty]
        private DateTime lastBackupDate = DateTime.MinValue;
    }
}
