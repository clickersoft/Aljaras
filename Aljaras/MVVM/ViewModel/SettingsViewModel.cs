using System.IO;
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using Aljaras.MVVM.Model;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using LiteDB;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Shapes;
using Path = System.IO.Path;
using Aljaras.Core;

namespace Aljaras.MVVM.ViewModel
{
    internal partial class SettingsViewModel : ObservableRecipient
    {

        public GlobalViewModel Global { get; } = GlobalViewModel.Instance;

        #region Observable Properties
        [ObservableProperty]
        private List<string> _lang = new();

        [ObservableProperty]
        private UserSettings userSet = new();
        #endregion

        #region RelayCommands
        [RelayCommand]
        private void ImportDataBase()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "Aljaras DataBase (*.jrsdb;*.jrsbck)|*.jrsdb;*.jrsbck;";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileExists = AppDomain.CurrentDomain.BaseDirectory + "Aljaras.jrsdb";
                if (File.Exists(fileExists))
                    File.Delete(fileExists);
                File.Copy(openFileDialog.FileName, Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Aljaras.jrsdb"));
                Global.NotificationMessage = new()
                {
                    BackgroundColor = MessageBackground.IndianRed.ToString(),
                    MessageText = Global.AppLang.Done
                };
                GlobalViewModel.Instance.NotificationList.Add(Global.NotificationMessage);
            }
            Global.LoadMonitoringAlarmCollectionData();            
        }

        [RelayCommand]
        private void ExportDataBase()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                string fileToCopy = AppDomain.CurrentDomain.BaseDirectory + "Aljaras.jrsdb";
                if (File.Exists(fileToCopy))
                {
                    string _desFile = Path.Combine(fbd.SelectedPath, Path.GetFileNameWithoutExtension(fileToCopy) + ".jrsbck");
                    File.Copy(fileToCopy, MakeUnique(_desFile).ToString());
                    Global.NotificationMessage = new()
                    {
                        BackgroundColor = MessageBackground.IndianRed.ToString(),
                        MessageText = Global.AppLang.Done
                    };
                    GlobalViewModel.Instance.NotificationList.Add(Global.NotificationMessage);
                }
                else 
                {
                    Global.NotificationMessage = new()
                    {
                        BackgroundColor = MessageBackground.IndianRed.ToString(),
                        MessageText = Global.AppLang.NoDataBase
                    };
                    GlobalViewModel.Instance.NotificationList.Add(Global.NotificationMessage);
                }
            }
        }

        [RelayCommand]
        private void DeleteDataBase()
        {
            MessageBoxResult messageBoxResult = MessageBox.Show(Global.AppLang.DeleteNotification, Global.AppLang.Delete+" " + Global.AppLang.Database, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (messageBoxResult != MessageBoxResult.Yes)
                return;
            string fileExists = AppDomain.CurrentDomain.BaseDirectory + "Aljaras.jrsdb";
            if (File.Exists(fileExists))
                File.Delete(fileExists);
            Global.LoadMonitoringAlarmCollectionData();
            Global.NotificationMessage = new()
            {
                BackgroundColor = MessageBackground.IndianRed.ToString(),
                MessageText = Global.AppLang.Done
            };
            GlobalViewModel.Instance.NotificationList.Add(Global.NotificationMessage);
        }

        [RelayCommand]
        private void SaveSettings()
        {
            using (var db = new LiteDatabase(@"Filename=Aljaras.jrsdb;connection=shared"))
            {
                var col = db.GetCollection<UserSettings>("UserSettings");
                var results = col.Find(x => x.Id == 1).FirstOrDefault();
                if (results != null)
                    col.Update(UserSet);
                else col.Insert(UserSet);
            }
            Global.GetUserSettings = UserSet;
            Global.SetAppLang();
            Global.LoadMonitoringAlarmCollectionData();
            Global.NextAlarm();
            Global.NotificationMessage = new()
            {
                BackgroundColor = MessageBackground.MediumSeaGreen.ToString(),
                MessageText = Global.AppLang.Done
            };
            GlobalViewModel.Instance.NotificationList.Add(Global.NotificationMessage);
        }

        [RelayCommand]
        private void PlayEmergencyAudioFile()
        {
            if (!Global.AudioPlayer.IsEmergency)
                Global.AudioPlayer.PlayPauseAudioFile(userSet.EmergencyAudioFileLocation, false);
        }

        [RelayCommand]
        private void SelectEmergencyAudioFile()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            string path = AppDomain.CurrentDomain.BaseDirectory + "Audio"; // this is the path that you are checking.
            if (Directory.Exists(path))
            {
                openFileDialog.InitialDirectory = path;
            }
            openFileDialog.Filter = "Audio File (*.mp3;*.wav)|*.mp3;*.wav;";
            if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            userSet.EmergencyAudioFileLocation = openFileDialog.FileName;
            if (Global.AudioPlayer.Output != null && Global.AudioPlayer.Output.PlaybackState != NAudio.Wave.PlaybackState.Stopped)
            {
                Global.AudioPlayer.Output.Stop();
                Global.AudioPlayer.Output = null;
            }
            if (!Global.AudioPlayer.IsEmergency)
                Global.AudioPlayer.PlayPauseAudioFile(openFileDialog.FileName, false);
        }
        #endregion

        #region Functions
        public SettingsViewModel()
        {
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Languages"))
            {
                FileInfo[] files = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "Languages").GetFiles("*.xml");
                foreach (FileInfo file in files)
                    _lang.Add(Path.GetFileNameWithoutExtension(file.Name));
            }
            using (var db = new LiteDatabase(@"Filename=Aljaras.jrsdb;connection=shared"))
            {
                var col = db.GetCollection<UserSettings>("UserSettings");
                var results = col.Find(x => x.Id == 1).FirstOrDefault();
                if (results != null)
                    UserSet = results;
            }
        }

        public FileInfo MakeUnique(string path)
        {
            string dir = Path.GetDirectoryName(path);
            string fileName = Path.GetFileNameWithoutExtension(path);
            string fileExt = Path.GetExtension(path);

            for (int i = 1; ; ++i)
            {
                if (!File.Exists(path))
                    return new FileInfo(path);

                path = Path.Combine(dir, fileName + " " + i + fileExt);
            }
        }
        #endregion

    }
}
