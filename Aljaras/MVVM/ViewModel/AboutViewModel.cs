using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Aljaras.MVVM.ViewModel
{
    internal partial class AboutViewModel : ObservableRecipient
    {
        public GlobalViewModel Global { get; } = GlobalViewModel.Instance;

        #region RelayCommands
        [RelayCommand]
        private void RunAboutFile() 
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "aljaras.rtf"))
             RunFileThroughCmd(AppDomain.CurrentDomain.BaseDirectory + "aljaras.rtf");
            else MessageBox.Show("File Not Found");
        }

        [RelayCommand]
        private void RunTutorialFile() 
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "tutorial.rtf"))
                RunFileThroughCmd(AppDomain.CurrentDomain.BaseDirectory + "tutorial.rtf");
            else MessageBox.Show("File Not Found");
        }
        #endregion

        #region Functions
        public AboutViewModel(){}

        private void RunFileThroughCmd(string FileLocation)
        {
            Process process = new();
            ProcessStartInfo startInfo = new();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.UseShellExecute = true;
            startInfo.CreateNoWindow = true;
            //It is important that the argument begins with /C, otherwise it won't work.
            startInfo.Arguments = "/C" + FileLocation;
            process.StartInfo = startInfo;
            process.Start();
        }
        #endregion

    }
}
