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
        private string emergencyAudioFileLocation = AppDomain.CurrentDomain.BaseDirectory + "Audio\\Emerg.mp3";

        [ObservableProperty]
        private bool repeatEmergency = true;

        [ObservableProperty]
        private bool isKeyRegistered = false;

        [ObservableProperty]
        private bool isShutdownOnClose = false;
    }
}
