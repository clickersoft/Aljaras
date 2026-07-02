using Aljaras.MVVM.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Aljaras.Core
{
    internal partial class AudioFileOperations : ObservableRecipient
    {

        #region Observable Properties
        [ObservableProperty]
        private BlockAlignReductionStream? stream = null;

        [ObservableProperty]
        private DirectSoundOut? output = null;

        [ObservableProperty]
        private bool isEmergency = false;

        [ObservableProperty]
        private bool repeat = false;

        [ObservableProperty]
        private string tmpAudio = string.Empty;

        /// <summary>Playback volume (0..1) applied to the next started clip.</summary>
        [ObservableProperty]
        private float playbackVolume = 1f;
        #endregion

        #region Functions
        public GlobalViewModel Global { get; set; } = GlobalViewModel.Instance;

        public async Task PlayPauseAudioFile(string fileLocation, bool emergency)
        {
            fileLocation = string.Concat(GlobalVariables.AppLocation, fileLocation);
            if (!File.Exists(fileLocation))
            {
                Global.NewNotificationMessage(MessageBackground.IndianRed , Global.AppLang.NotCorrectAudio);
                return;
            }
            if (TmpAudio != fileLocation || emergency)
                DisposeWave();
            TmpAudio = fileLocation;
            if (Output != null)
            {
                if (Output.PlaybackState == PlaybackState.Playing) Output.Pause();
                else if (Output.PlaybackState == PlaybackState.Paused) Output.Play();
                else if (Output.PlaybackState == PlaybackState.Stopped)
                    StartAudio(fileLocation);
            }
            else if (fileLocation != null)
            {
                if (Output != null && Output.PlaybackState == PlaybackState.Playing) Output.Stop();
                StartAudio(fileLocation);
                while (IsEmergency && Repeat)
                {
                    if (Output != null && Output.PlaybackState == PlaybackState.Stopped)
                        StartAudio(fileLocation);
                    await Task.Delay(500);
                }
                if (!Repeat)
                    GlobalViewModel.Instance.AudioOperations.isEmergency = false;
            }
        }

        private void StartAudio(string fileLocation)
        {
            WaveStream reader;
            if (fileLocation.ToLower().EndsWith(".mp3"))
                reader = new Mp3FileReader(fileLocation);
            else if (fileLocation.ToLower().EndsWith(".wav"))
                reader = new WaveFileReader(fileLocation);
            else return;
            // WaveChannel32 lets us scale amplitude per clip (0..1).
            WaveChannel32 channel = new(reader) { Volume = Math.Clamp(PlaybackVolume, 0f, 1f) };
            stream = new BlockAlignReductionStream(channel);
            Output = new DirectSoundOut();
            Output.Init(stream);
            Output.Play();
        }

        public void DisposeWave()
        {
            if (output != null)
            {
                if (output.PlaybackState == PlaybackState.Playing) output.Stop();
                output.Dispose();
                output = null;
            }
            if (stream != null)
            {
                stream.Dispose();
                stream = null;
            }
        }

        public string MoveAudioFileToLibrary(string OriginalAudioFileLocation)
        {
            string UserAudioLibraryPath = string.Concat(GlobalVariables.AppLocation, "Audio\\", GlobalVariables.PCCurrentUserName, "\\");
            if (!Directory.Exists(UserAudioLibraryPath))
                Directory.CreateDirectory(UserAudioLibraryPath);
            string DestinationAudioFilePath = string.Concat(UserAudioLibraryPath, Path.GetFileName(OriginalAudioFileLocation));
            if (!File.Exists(OriginalAudioFileLocation))
                return "Audio File Not Found...";
            if (!File.Exists(DestinationAudioFilePath))
                File.Copy(OriginalAudioFileLocation, DestinationAudioFilePath, true);
            return string.Concat("Audio\\", GlobalVariables.PCCurrentUserName, "\\", Path.GetFileName(OriginalAudioFileLocation));
        }
        #endregion
    }
}
