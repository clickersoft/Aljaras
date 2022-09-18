using Aljaras.Core;
using Aljaras.MVVM.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteDB;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

namespace Aljaras.MVVM.ViewModel
{
    internal partial class SettingsViewModel : ObservableRecipient
    {
        public GlobalViewModel Global { get; set; } = GlobalViewModel.Instance;

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
            Global.AudioOperations.DisposeWave();
            OpenFileDialog openFileDialog = new() { Filter = "Aljaras DataBase (*.jrsdb;*.jrsbck)|*.jrsdb;*.jrsbck;" };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if(Path.GetExtension(openFileDialog.FileName).ToLower() == ".jrsdb")
                {
                    string fileExists = string.Concat(App.AppLocation, App.PCCurrentUserName, App.dbName);
                    if (File.Exists(fileExists))
                        File.Delete(fileExists);
                    File.Copy(openFileDialog.FileName, Path.Combine(App.AppLocation , App.PCCurrentUserName + App.dbName + ".jrsdb"));
                }
                else
                {
                    string GetAudioLocation = Path.Combine(App.AppLocation, "Audio");
                    ZipFile.ExtractToDirectory(openFileDialog.FileName, GetAudioLocation,  true);
                    string? dbSourceFile = Directory.GetFiles(GetAudioLocation, "*.jrsdb").FirstOrDefault();
                    string dbDestinationFile = App.AppLocation + App.PCCurrentUserName + "Aljaras.jrsdb";
                    if(!File.Exists(dbSourceFile))
                    {
                        Global.NewNotificationMessage(MessageBackground.IndianRed , Global.AppLang.NoDataBase);
                        return;
                    }
                    if(File.Exists(dbDestinationFile))
                        File.Delete(dbDestinationFile);
                    File.Move(dbSourceFile, dbDestinationFile);
                }
                Global.NewNotificationMessage(MessageBackground.SeaGreen, Global.AppLang.Done);
            }
            Global.LoadMonitoringAlarmCollectionData();            
        }

        [RelayCommand]
        private void LoadSample()
        {
            Global.AudioOperations.DisposeWave();
            string SampleFile = string.Concat(App.AppLocation, "Sample.jrsbck");
            if (File.Exists(SampleFile))
            { 
                string GetAudioLocation = Path.Combine(App.AppLocation, "Audio");
                ZipFile.ExtractToDirectory(SampleFile, GetAudioLocation,  true);
                string? dbSourceFile = Directory.GetFiles(GetAudioLocation, "*.jrsdb").FirstOrDefault();
                string dbDestinationFile = App.AppLocation + App.PCCurrentUserName + App.AppName + ".jrsdb";
                if (!File.Exists(dbSourceFile))
                {
                    Global.NewNotificationMessage(MessageBackground.IndianRed, Global.AppLang.NoDataBase);
                    return;
                }
                if (File.Exists(dbDestinationFile))
                    File.Delete(dbDestinationFile);
                File.Move(dbSourceFile, dbDestinationFile);
                Global.NewNotificationMessage(MessageBackground.SeaGreen, Global.AppLang.Done);
            }
            Global.LoadMonitoringAlarmCollectionData();
        }

        [RelayCommand]
        private void ExportDataBase()
        {
            FolderBrowserDialog folderBrowserDialog = new();
            if (folderBrowserDialog.ShowDialog() != DialogResult.OK) return;
            string UserAudioLibrary = string.Concat(App.AppLocation, "Audio\\", App.PCCurrentUserName);
            string dbfile = string.Concat(App.AppLocation, App.PCCurrentUserName, App.AppName, ".jrsdb");
            string DestinationPath = Path.Combine(folderBrowserDialog.SelectedPath, string.Concat(App.PCCurrentUserName , App.AppName , ".jrsbck"));
            DestinationPath = MakeUnique(DestinationPath).ToString();
            if (Directory.Exists(UserAudioLibrary) && File.Exists(dbfile))
            {
                ZipFile.CreateFromDirectory(UserAudioLibrary, DestinationPath, CompressionLevel.Fastest, true);
                ZipArchive zipArchive = ZipFile.Open(DestinationPath, ZipArchiveMode.Update);
                zipArchive.CreateEntryFromFile(dbfile, Path.GetFileName(dbfile), CompressionLevel.Fastest);
                zipArchive.Dispose();
            }
            else if (File.Exists(dbfile))
            {
                DestinationPath = Path.Combine(folderBrowserDialog.SelectedPath, Path.GetFileName(dbfile));
                File.Copy(dbfile, DestinationPath);
            }
            else
            {
                Global.NewNotificationMessage(MessageBackground.IndianRed, Global.AppLang.NoDataBase);
                return;
            }
            Global.NewNotificationMessage(MessageBackground.SeaGreen, Global.AppLang.Done);
        }

        [RelayCommand]
        private void DeleteDataBase()
        {
            MessageBoxResult messageBoxResult = MessageBox.Show(Global.AppLang.DeleteNotification, Global.AppLang.Delete+ " " + Global.AppLang.Database, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (messageBoxResult != MessageBoxResult.Yes)
                return;
            string fileExists = string.Concat(App.AppLocation, App.PCCurrentUserName, App.AppName ,".jrsdb");
            if (File.Exists(fileExists))
                File.Delete(fileExists);
            Global.LoadMonitoringAlarmCollectionData();
            Global.NewNotificationMessage(MessageBackground.SeaGreen, Global.AppLang.Done);
        }

        [RelayCommand]
        private void SaveSettings()
        {
            if (!StartUpManager.IsUserAdministrator())
            {
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(Global.AppLang.RunasAdministratorMessage, Global.AppLang.RunasAdministrator,  MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (messageBoxResult == MessageBoxResult.Yes)
                    StartUpManager.RelaunchAsAdministrator();
                else return;
            }

            using (App.db)
            {
                if (UserSet.SetRegistryKey)
                    try { StartUpManager.AddApplicationToAllUsersStartup(); }
                    catch { Global.NewNotificationMessage(MessageBackground.IndianRed, Global.AppLang.RegistryFailed); }
                else try { StartUpManager.RemoveApplicationFromAllUsersStartup(); }
                    catch { Global.NewNotificationMessage(MessageBackground.IndianRed, Global.AppLang.DeleteRegistryFailed); }
                UserSet.EmergencyAudioFileLocation = Global.AudioOperations.MoveAudioFileToLibrary(UserSet.EmergencyAudioFileLocation);
                var col = App.db.GetCollection<UserSettings>(DbTables.UserSettings.ToString());
                UserSettings? results = col.Find(x => x.Id == 1).FirstOrDefault();
                if (results != null)
                    col.Update(UserSet);
                else col.Insert(UserSet);
            }
            Global.GetUserSettings = UserSet;
            Global.SetAppLang();
            Global.LoadMonitoringAlarmCollectionData();
            Global.NextAlarm();
            Global.NewNotificationMessage(MessageBackground.MediumSeaGreen, Global.AppLang.Done);
        }

        [RelayCommand]
        private void PlayEmergencyAudioFile()
        {
            if (!Global.AudioOperations.IsEmergency)
                _ = Global.AudioOperations.PlayPauseAudioFile(UserSet.EmergencyAudioFileLocation, false);
        }

        [RelayCommand]
        private void SelectEmergencyAudioFile()
        {
            OpenFileDialog openFileDialog = new();
            string path = App.AppLocation + "Audio"; // this is the path that you are checking.
            if (Directory.Exists(path))
            {
                openFileDialog.InitialDirectory = path;
            }
            openFileDialog.Filter = "Audio File (*.mp3;*.wav)|*.mp3;*.wav;";
            if (openFileDialog.ShowDialog() !=DialogResult.OK) return;
            string newPath = Global.AudioOperations.MoveAudioFileToLibrary(openFileDialog.FileName);
            UserSet.EmergencyAudioFileLocation = newPath;
            if (Global.AudioOperations.Output != null && Global.AudioOperations.Output.PlaybackState != NAudio.Wave.PlaybackState.Stopped)
            {
                Global.AudioOperations.Output.Stop();
                Global.AudioOperations.Output = null;
            }
            if (!Global.AudioOperations.IsEmergency)
                _ = Global.AudioOperations.PlayPauseAudioFile(newPath, false);
        }
        #endregion

        #region Functions
        public SettingsViewModel()
        {
            if (Directory.Exists(App.AppLocation + "Languages"))
            {
                FileInfo[] files = new DirectoryInfo(App.AppLocation + "Languages").GetFiles("*.xml");
                foreach (FileInfo file in files)
                    _lang.Add(Path.GetFileNameWithoutExtension(file.Name));
            }
            using (App.db)
            {
                var col = App.db.GetCollection<UserSettings>(DbTables.UserSettings.ToString());
                var results = col.Find(x => x.Id == 1).FirstOrDefault();
                if (results != null)
                    UserSet = results;
            }
            if(!File.Exists(string.Concat(App.AppLocation, UserSet.EmergencyAudioFileLocation)))
            UserSet.EmergencyAudioFileLocation = GlobalViewModel.Instance.AudioOperations.MoveAudioFileToLibrary(App.AppLocation + "Audio\\Emerg.mp3");
        }

        public static FileInfo MakeUnique(string path)
        {
            string dir = Path.GetDirectoryName(path)!;
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
