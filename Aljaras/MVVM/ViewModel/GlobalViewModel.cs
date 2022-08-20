using Aljaras.Core;
using Aljaras.MVVM.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteDB;
using NAudio.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace Aljaras.MVVM.ViewModel
{
    internal partial class GlobalViewModel : ObservableRecipient
    {
        #region Variables
        public delegate void TimerEvent(string sampleParam);
        public event TimerEvent? TimerEvt;
        #endregion

        #region Observable 
        [ObservableProperty]
        ObservableCollection<UserNotificationMessage> notificationList  = new ObservableCollection<UserNotificationMessage>();

        [ObservableProperty]
        UserNotificationMessage notificationMessage = new();

        [ObservableProperty]
        private List<Holiday> holidayList = new();

        [ObservableProperty]
        private List<Alarm> alarmList = new();

        [ObservableProperty]
        AudioFilePlayer audioPlayer = new();

        [ObservableProperty]
        UserSettings getUserSettings = new();

        [ObservableProperty]
        private AppLanguage appLang = new();

        [ObservableProperty]
        private string systemTime = "";

        [ObservableProperty]
        private string alertStartTime = "";

        [ObservableProperty]
        private string alertEndTime = "";

        [ObservableProperty]
        private string currentAlarm = "";

        [ObservableProperty]
        int defaultHour = 0;

        [ObservableProperty]
        int defaultMin = 0;

        [ObservableProperty]
        int defaultSec = 1;

        [ObservableProperty]
        private List<Schedule> scheduleList = new();

        [ObservableProperty]
        private List<Alarm> reminderList = new();

        [ObservableProperty]
        private string isNOAlarmMessageVisible = "";

        [ObservableProperty]
        private string isNOHolidayMessageVisible = "";

        [ObservableProperty]
        private string showTimeLeft = "";

        [ObservableProperty]
        DispatcherTimer dispatcherTimer = new();

        [ObservableProperty]
        DateTime start = new();

        [ObservableProperty]
        private TimeSpan timeLeft = new();

        [ObservableProperty]
        private bool recordButtonEnabled = true;

        [ObservableProperty]
        private bool stopButtonEnabled = false;

        [ObservableProperty]
        private List<WaveInCapabilities> micDevicesList = new();

        [ObservableProperty]
        private WaveInCapabilities selectedMicDevice = new();

        [ObservableProperty]
        private List<WaveOutCapabilities> speakerDevicesList = new();

        [ObservableProperty]
        private WaveOutCapabilities selectedSpeakerDevice = new();

        [ObservableProperty]
        private string recordingActionVisibility = "Hidden";

        [ObservableProperty]
        private string emergencyActionVisibility = "Hidden";

        [ObservableProperty]
        private Random startRandom = new Random();
        #endregion

        #region RelayCommands

        WaveIn wave;
        WaveOut waveOut;
        BufferedWaveProvider provider;

        [RelayCommand]
        private void StopRecording()
        {
            RecordingActionVisibility = "Hidden";
            RecordButtonEnabled = true;
            StopButtonEnabled = false;
            wave.StopRecording();
            wave.Dispose();
            waveOut.Dispose();
        }

        [RelayCommand]
        private void StartRecording()
        {
            if (AudioPlayer.IsEmergency) return;
            RecordingActionVisibility = "Visible";
            if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Audio\\Attention.mp3"))
            AudioPlayer.PlayPauseAudioFile(AppDomain.CurrentDomain.BaseDirectory + "Audio\\Attention.mp3", false);
            RecordButtonEnabled = false;
            StopButtonEnabled = true;

            wave = new WaveIn();
            wave.DeviceNumber = MicDevicesList.IndexOf(SelectedMicDevice); 

            waveOut = new WaveOut();
            waveOut.DeviceNumber = SpeakerDevicesList.IndexOf(SelectedSpeakerDevice);
            waveOut.DesiredLatency = 100;

            provider = new BufferedWaveProvider(wave.WaveFormat);
            waveOut.Init(provider);
            waveOut.Play();

            wave.DataAvailable += Wave_DataAvailable;
            wave.StartRecording();
        }

        private void Wave_DataAvailable(object sender, WaveInEventArgs e)
        {
            provider.AddSamples(e.Buffer, 0, e.BytesRecorded);
        }

        [RelayCommand]
        private void PushToast()
        {
            NotificationMessage = new()
            {
                BackgroundColor = ((MessageBackground)startRandom.Next(16)).ToString(),
                MessageText = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 10)
                .Select(s => s[startRandom.Next(s.Length)]).ToArray())
            };
            NotificationList.Add(NotificationMessage);
        }
       

        [RelayCommand]
        private void DeleteNotification(UserNotificationMessage obj)
        {
            if(NotificationList.Contains(obj))
                NotificationList.Remove(obj);
        }

        partial void OnNotificationMessageChanged(UserNotificationMessage value)
        {
            /*_ = Task.Run(async () =>
            {
                await Task.Delay(5000);
                if (NotificationList != null && NotificationList.Count > 0)
                    NotificationList.Remove(value);
            });*/
        }

        [RelayCommand]
        private void Emergency()
        {
            AudioPlayer.IsEmergency = !AudioPlayer.IsEmergency;
            if (AudioPlayer.IsEmergency)
                EmergencyActionVisibility = "Visible";
            else EmergencyActionVisibility = "Hidden";
            _ = AudioPlayer.PlayPauseAudioFile(GetUserSettings.EmergencyAudioFileLocation, AudioPlayer.IsEmergency);
        }
        #endregion

        #region Functions
        public static GlobalViewModel Instance { get; set; } = new GlobalViewModel();

        partial void OnGetUserSettingsChanged(UserSettings value)
        {
            AudioPlayer.Repeat = GetUserSettings.RepeatEmergency;
        }

        partial void OnAppLangChanged(AppLanguage value)
        {
            CurrentAlarm = AppLang.NoMoreAlarms;
        }

        private void LoadDevices()
        {
            for (int deviceId = 0; deviceId < WaveIn.DeviceCount; deviceId++)
            {
                var deviceInfo = WaveIn.GetCapabilities(deviceId);
              
                MicDevicesList.Add(deviceInfo);
            }

            for (int deviceId = 0; deviceId < WaveOut.DeviceCount; deviceId++)
            {
                var deviceInfo = WaveOut.GetCapabilities(deviceId);
                SpeakerDevicesList.Add(deviceInfo);
            }
        }

        public GlobalViewModel()
        {
            SetAppLang();
            LoadMonitoringAlarmCollectionData();
            NextAlarm();
            LoadDevices();
            NotificationList = new();
            Task.Run(async () =>
            {
                while (true)
                {
                    Application.Current.Dispatcher.Invoke(() => SystemTime = DateTime.Now.ToLongTimeString());
                    if (AlarmList != null && AlarmList.Count > 0)
                    {
                        Alarm _alr = AlarmList.FirstOrDefault(x => TrimMilliseconds(x.FullTime) == TrimMilliseconds(DateTime.Now));
                        if (_alr != null)
                        {
                            if (!AudioPlayer.IsEmergency)
                            StartAudio(_alr.AudioFileLocation);
                            NextAlarm();
                        }
                    }
                    // don't run again for at least 1000 milliseconds
                    await Task.Delay(1000);
                }
            });
        }

        public void SetAppLang()
        {
            using (var db = new LiteDatabase(@"Filename=Aljaras.jrsdb;connection=shared"))
            {
                // Get a collection (or create, if doesn't exist)
                var col = db.GetCollection<UserSettings>("UserSettings");
                UserSettings results = col.Find(x => x.Id == 1).FirstOrDefault();
                if (results != null)
                    GetUserSettings = results;
                else col.Insert(GetUserSettings);
            }

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Languages\\" + GetUserSettings.CurrentLang + ".xml"))
            {
                XmlSerializer reader = new XmlSerializer(typeof(UserSettings));
                reader = new XmlSerializer(typeof(AppLanguage));
                StreamReader file = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "Languages\\" + GetUserSettings.CurrentLang + ".xml");
                AppLang = (AppLanguage)reader.Deserialize(file);
                file.Close();
            }
        }

        public static DateTime TrimMilliseconds(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0, dt.Kind);
        }

        void StartAudio(string _afl)
        {
            var fileLocation = new string[] { _afl, AppDomain.CurrentDomain.BaseDirectory + "Audio\\School.mp3"}.FirstOrDefault(s => !string.IsNullOrEmpty(s) && File.Exists(s)) ?? "";
            if (string.IsNullOrEmpty(fileLocation)) 
            {
                MessageBox.Show("Not a correct audio file type or location.");
                return;
            }
            AudioPlayer.Output = null;
            AudioPlayer.PlayPauseAudioFile(fileLocation, false);
        }

        public void NextAlarm()
        {
            TimeLeft = TimeSpan.Zero;
            if (AlarmList != null && AlarmList.Count > 0)
            {
                Alarm _Nextalarm = AlarmList.FirstOrDefault(x => x.FullTime > DateTime.Now);
                if (_Nextalarm != null && _Nextalarm.FullTime != null)
                {
                    DefaultHour = _Nextalarm.FullTime.Hour;
                    DefaultMin = _Nextalarm.FullTime.Minute;
                    CurrentAlarm = _Nextalarm.FullTime.ToString("hh:mm tt");
                    TimeLeft = _Nextalarm.FullTime.Subtract(DateTime.Now);
                }
                else
                {
                    foreach (Alarm _alarm in AlarmList)
                        _alarm.FullTime = ChangeDateOnly(_alarm.FullTime).AddDays(1);
                    NextAlarm();
                }
            }
            else CurrentAlarm = AppLang.NoMoreAlarms;
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 40);
            dispatcherTimer.Start();
            Start = DateTime.Now;
        }

        public static DateTime ChangeDateOnly(DateTime oldDateTime)
        {
            return DateTime.Now.Date + oldDateTime.TimeOfDay;
        }

        public static object GetPropValue(object src, string propName) => src.GetType().GetProperty(propName).GetValue(src, null);

        public void LoadMonitoringAlarmCollectionData()
        {
            ScheduleList = new();
            AlarmList = new();
            HolidayList = new();
            using (var db = new LiteDatabase(@"Filename=Aljaras.jrsdb;connection=shared"))
            {
                var holidayCollection = db.GetCollection<Holiday>("Holidays");
                HolidayList = holidayCollection.Find(h => h.HolidayDate > DateTime.Now && h.IsHolidayActive).OrderBy(x => x.HolidayDate).ToList();
                if(HolidayList!= null && HolidayList.Count>0)
                {
                    List<Holiday> _tmp = HolidayList.FindAll(h => h.IsReminderActive && h.ReminderDate.Date == DateTime.Now.Date);
                    if (_tmp != null && _tmp.Count > 0)
                        foreach (var item in _tmp)
                        {
                            Alarm _alr = new()
                            {
                                AlarmTitle = item.HolidayTitle,
                                FullTime = item.FullTime,
                                AudioFileLocation = item.ReminderAudioFileLocation
                            };
                            AlarmList.Add(_alr);                            
                        }
                }
                if (HolidayList != null && HolidayList.Count > 0)
                    IsNOHolidayMessageVisible = "Hidden";

                var scheduleCollection = db.GetCollection<Schedule>("Schedules");
                ScheduleList = scheduleCollection.Find(x => x.IsScheduleActive == true).ToList();
                if (ScheduleList != null && ScheduleList.Count > 0)
                {
                    foreach (Schedule item in ScheduleList)
                    {
                        var alarmCollection = db.GetCollection<Alarm>("Alarms");
                        List<Alarm> result = alarmCollection.Find(x => x.ScheduleId.ToString().Contains(item.ScheduleId.ToString()) && x.IsAlarmActive == true).ToList();
                        if (result != null && result.Count > 0)
                        { 
                            foreach (Alarm _item in result)
                            if ((bool)GetPropValue(_item, DateTime.Now.DayOfWeek.ToString().Substring(0, 3)))
                            {
                                _item.FullTime = ChangeDateOnly(_item.FullTime);
                                AlarmList.Add(_item);
                            }
                            AlarmList = AlarmList.OrderBy(x => x.FullTime).ToList();
                        } 
                    }
                    if (AlarmList != null && AlarmList.Count > 0)
                    {
                        IsNOAlarmMessageVisible = "Hidden";
                        return;
                    }
                }
            }
            IsNOAlarmMessageVisible = "Visible";
            IsNOHolidayMessageVisible = "Visible";
        }

        private void dispatcherTimer_Tick(object? sender, EventArgs e)
        {
            TimeSpan diff = Start - DateTime.Now;
            TimeSpan display = (TimeLeft + diff);
            if (TimeLeft == TimeSpan.Zero) { ShowTimeLeft = "00:00:00"; return; };
            if (display > new TimeSpan(0, 0, 10))
            {
                ShowTimeLeft = $"{display.Hours:D2}:{display.Minutes:D2}:{display.Seconds:D2}";
            }
            else if (display <= new TimeSpan(0, 0, 1))
            {
                Start = DateTime.Now - new TimeSpan(0, 10, 0);
            }
            else if (display <= new TimeSpan(0, 1, 0))
            {
                ShowTimeLeft = $"{display.Hours:D2}:{display.Minutes:D2}:{display.Seconds:D2}";
            }
            if (TimerEvt != null)
            {
                TimerEvt("sample parameter");
            }
        }
        #endregion

    }
}
