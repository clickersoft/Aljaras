using Aljaras.MVVM.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NAudio.Wave;
using System.Threading.Tasks;

namespace Aljaras.Core
{
    internal partial class AudioFilePlayer : ObservableRecipient
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
        private string tmpAudio = "";
        #endregion

        #region Functions
        public GlobalViewModel Global { get; set; } = GlobalViewModel.Instance;

        public async Task PlayPauseAudioFile(string fileLocation, bool emergency)
        {
            if (TmpAudio != fileLocation || emergency) 
                DisposeWave();
                    TmpAudio = fileLocation;
            if (Output != null)
            {
                if (Output.PlaybackState == PlaybackState.Playing) Output.Pause();
                else if (Output.PlaybackState == PlaybackState.Paused) Output.Play();
                else if (Output.PlaybackState == PlaybackState.Stopped)
                    StartAudio(fileLocation);
            } else if (fileLocation != null)
                {
                    if (Output != null && Output.PlaybackState == PlaybackState.Playing) Output.Stop();
                    StartAudio(fileLocation);
                    while (IsEmergency && Repeat)
                    {
                        if (Output != null && Output.PlaybackState == PlaybackState.Stopped)
                        StartAudio(fileLocation);
                        await Task.Delay(500);
                    }
                    if(!Repeat)
                    GlobalViewModel.Instance.AudioPlayer.isEmergency = false;
                }
        }

        private void StartAudio(string fileLocation)
        {
            WaveStream pcm;
            if (fileLocation.ToLower().EndsWith(".mp3"))
                pcm = WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(fileLocation));
            else if (fileLocation.ToLower().EndsWith(".wav"))
                pcm = new WaveChannel32(new WaveFileReader(fileLocation));
            else return;
            stream = new BlockAlignReductionStream(pcm);
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
        #endregion

    }
}
