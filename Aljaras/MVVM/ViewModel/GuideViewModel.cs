using Aljaras.Core;
using Aljaras.MVVM.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aljaras.MVVM.ViewModel
{
    internal partial class GuideViewModel : ObservableRecipient
    {
        public GlobalViewModel Global { get; } = GlobalViewModel.Instance;

        [ObservableProperty]
        private List<string> guideList = new();

        [ObservableProperty]
        private string nOGuideImagesVisible = GetVisibility.Visible.ToString();

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
        public GuideViewModel()
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Guide")) return;
            DirectoryInfo GuideImagesDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "Guide");
            foreach (FileInfo aFile in GuideImagesDirectory.GetFiles("*.png"))
            {
                /*Uri uri = new Uri(aFile.FullName);
                GuideList.Add(new BitmapImage(uri));*/
                GuideList.Add(aFile.FullName);
            }
            if (GuideList.Any())
                NOGuideImagesVisible = GetVisibility.Hidden.ToString();
        }

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
