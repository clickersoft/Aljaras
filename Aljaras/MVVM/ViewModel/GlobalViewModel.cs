using Aljaras.Core;
using Aljaras.MVVM.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteDB;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace Aljaras.MVVM.ViewModel
{
    internal partial class GlobalViewModel : ObservableRecipient
    {

        #region Variables
        public delegate void TimerEvent(string sampleParam);
        public event TimerEvent? TimerEvt;
        WaveIn wave = new();
        WaveOut waveOut = new();
        BufferedWaveProvider provider;
        #endregion

        #region Observable 
        [ObservableProperty]
        ObservableCollection<UserNotificationMessage> notificationList  = new();

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
        private string currentAlarmTitle = "";

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
        private string isNOAlarmMessageVisible = GetVisibility.Visible.ToString();

        [ObservableProperty]
        private string isNOHolidayMessageVisible = GetVisibility.Visible.ToString();

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
        private ObservableCollection<WaveInCapabilities> micDevicesList = new();

        [ObservableProperty]
        private WaveInCapabilities selectedMicDevice = new();

        [ObservableProperty]
        private int indexOfMicDevice = 0;

        [ObservableProperty]
        private ObservableCollection<WaveOutCapabilities> speakerDevicesList = new();

        [ObservableProperty]
        private WaveOutCapabilities selectedSpeakerDevice = new();

        [ObservableProperty]
        private int indexOfSpeakerDevice = 0;

        [ObservableProperty]
        private string recordingActionVisibility = GetVisibility.Hidden.ToString();

        [ObservableProperty]
        private string emergencyActionVisibility = GetVisibility.Hidden.ToString();

        [ObservableProperty]
        private Random startRandom = new();
        #endregion

        #region RelayCommands
        [RelayCommand]
        private void StopRecording()
        {
            RecordingActionVisibility = GetVisibility.Hidden.ToString();
            RecordButtonEnabled = true;
            StopButtonEnabled = false;
            wave.StopRecording();
            wave.Dispose();
            waveOut.Dispose();
        }

        [RelayCommand]
        private void StartRecording()
        {
            if (AudioPlayer.IsEmergency || MicDevicesList == null || SpeakerDevicesList == null || !MicDevicesList.Any() || !SpeakerDevicesList.Any()) return;
            RecordingActionVisibility = GetVisibility.Visible.ToString();
            if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Audio\\Attention.mp3"))
                _ = AudioPlayer.PlayPauseAudioFile(AppDomain.CurrentDomain.BaseDirectory + "Audio\\Attention.mp3", false);
            RecordButtonEnabled = false;
            StopButtonEnabled = true;
            wave = new()
            {
                DeviceNumber = MicDevicesList.IndexOf(SelectedMicDevice)
            };
            waveOut = new()
            {
                DeviceNumber = SpeakerDevicesList.IndexOf(SelectedSpeakerDevice),
                DesiredLatency = 100
            };
            provider = new BufferedWaveProvider(wave.WaveFormat);
            waveOut.Init(provider);
            waveOut.Play();
            wave.DataAvailable += Wave_DataAvailable;
            wave.StartRecording();
        }

        [RelayCommand]
        private void DeleteNotification(UserNotificationMessage obj) {NotificationList.Remove(obj);}

        /*partial void OnNotificationMessageChanged(UserNotificationMessage value)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(5000);
                if (NotificationList != null && NotificationList.Count > 0)
                    NotificationList.Remove(value);
            });
        }*/

        [RelayCommand]
        private void Emergency()
        {
            AudioPlayer.IsEmergency = !AudioPlayer.IsEmergency;
            if (AudioPlayer.IsEmergency) 
            {
                EmergencyActionVisibility = GetVisibility.Visible.ToString();
                StopRecording();
            } else EmergencyActionVisibility = GetVisibility.Hidden.ToString();
            _ = AudioPlayer.PlayPauseAudioFile(GetUserSettings.EmergencyAudioFileLocation, AudioPlayer.IsEmergency);
        }

        [RelayCommand]
        private void ReloadDevices() { LoadDevices(); }
        #endregion

        #region Functions
        public static GlobalViewModel Instance { get; set; } = new GlobalViewModel();

        private void Wave_DataAvailable(object sender, WaveInEventArgs e) { provider.AddSamples(e.Buffer, 0, e.BytesRecorded); }

        partial void OnGetUserSettingsChanged(UserSettings value) { AudioPlayer.Repeat = GetUserSettings.RepeatEmergency;}

        partial void OnAppLangChanged(AppLanguage value) 
        { 
            CurrentAlarm = AppLang.NoMoreAlarms;
            CurrentAlarmTitle = AppLang.NoMoreAlarms;
        }

        private void LoadDevices()
        {
            MicDevicesList = new();
            SpeakerDevicesList = new();
            MicDevicesList.Clear();
            SpeakerDevicesList.Clear();
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
            if (MicDevicesList != null && MicDevicesList.Any())
            {
                SelectedMicDevice = MicDevicesList.FirstOrDefault();
                IndexOfMicDevice = MicDevicesList.IndexOf(SelectedMicDevice);
            }
            if (SpeakerDevicesList != null && SpeakerDevicesList.Any())
            {
                SelectedSpeakerDevice = SpeakerDevicesList.FirstOrDefault();
                IndexOfSpeakerDevice = SpeakerDevicesList.IndexOf(SelectedSpeakerDevice);
            }

        }

        private GlobalViewModel()
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
                        Alarm? _alr = AlarmList.FirstOrDefault(x => TrimMilliseconds(x.FullTime) == TrimMilliseconds(DateTime.Now));
                        if (_alr != null)
                        {
                            if (!AudioPlayer.IsEmergency)
                            StartAudio(_alr.AudioFileLocation);
                            NextAlarm();
                        }
                    }
                    await Task.Delay(1000);
                }
            });
        }

        public void SetAppLang()
        {
            using (var db = new LiteDatabase(@"Filename=Aljaras.jrsdb;connection=shared"))
            {
                var col = db.GetCollection<UserSettings>("UserSettings");
                UserSettings? results = col.Find(x => x.Id == 1).FirstOrDefault();
                if (results != null)
                    GetUserSettings = results;
                else 
                {
                    try
                    {
                        Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                        Assembly curAssembly = Assembly.GetExecutingAssembly();
                        key.SetValue(curAssembly.GetName().Name, Path.ChangeExtension(curAssembly.Location, ".exe"));
                        GetUserSettings.IsKeyRegistered = true;
                        col.Insert(GetUserSettings);
                    }
                    catch
                    {
                        col.Insert(GetUserSettings);
                        Instance.NotificationMessage = new()
                        {
                            BackgroundColor = MessageBackground.IndianRed.ToString(),
                            MessageText = "Setting up registry key Failed"
                        };
                    }
                } 
            }
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Languages\\" + GetUserSettings.CurrentLang + ".xml"))
            {
                XmlSerializer reader = new(typeof(UserSettings));
                reader = new XmlSerializer(typeof(AppLanguage));
                StreamReader file = new(AppDomain.CurrentDomain.BaseDirectory + "Languages\\" + GetUserSettings.CurrentLang + ".xml");
                AppLang = (AppLanguage)reader.Deserialize(file)!;
                file.Close();
            }
            if (!GetUserSettings.IsKeyRegistered)
            {
               
            }
        }

        public static DateTime TrimMilliseconds(DateTime dt) { return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0, dt.Kind);}

        void StartAudio(string _afl)
        {
            var fileLocation = new string[] { _afl, AppDomain.CurrentDomain.BaseDirectory + "Audio\\School.mp3"}.FirstOrDefault(s => !string.IsNullOrEmpty(s) && File.Exists(s)) ?? "";
            if (string.IsNullOrEmpty(fileLocation)) 
            {
                Instance.NotificationMessage = new()
                {
                    BackgroundColor = MessageBackground.IndianRed.ToString(),
                    MessageText = Instance.AppLang.NotCorrectAudio
                };
                return;
            }
            AudioPlayer.Output = null;
            _ = AudioPlayer.PlayPauseAudioFile(fileLocation, false);
        }

        public void NextAlarm()
        {
            TimeLeft = TimeSpan.Zero;
            if (AlarmList != null && AlarmList.Count > 0)
            {
                Alarm? _Nextalarm = AlarmList.FirstOrDefault(x => x.FullTime > DateTime.Now);
                if (_Nextalarm != null)
                {
                    DefaultHour = _Nextalarm.FullTime.Hour;
                    DefaultMin = _Nextalarm.FullTime.Minute;
                    CurrentAlarm = _Nextalarm.FullTime.ToString("hh:mm tt");
                    CurrentAlarmTitle = _Nextalarm.AlarmTitle;
                    TimeLeft = _Nextalarm.FullTime.Subtract(DateTime.Now);
                }
                else
                {
                    foreach (Alarm _alarm in AlarmList)
                        _alarm.FullTime = ChangeDateOnly(_alarm.FullTime).AddDays(1);
                    NextAlarm();
                }
            }
            else 
            { 
                CurrentAlarm = AppLang.NoMoreAlarms;
                CurrentAlarmTitle = AppLang.NoMoreAlarms;
            }
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 40);
            dispatcherTimer.Start();
            Start = DateTime.Now;
        }

        public static DateTime ChangeDateOnly(DateTime oldDateTime) { return DateTime.Now.Date + oldDateTime.TimeOfDay; }

        public static object GetPropValue(object src, string propName) => src.GetType().GetProperty(propName)!.GetValue(src, null)!;

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
                    IsNOHolidayMessageVisible = GetVisibility.Hidden.ToString();
                else IsNOHolidayMessageVisible = GetVisibility.Visible.ToString();

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
                        IsNOAlarmMessageVisible = GetVisibility.Hidden.ToString();
                        return;
                    }
                }
            }
            IsNOAlarmMessageVisible = GetVisibility.Visible.ToString();
        }

        private void DispatcherTimer_Tick(object? sender, EventArgs e)
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
            TimerEvt?.Invoke("parameter");
        }
        #endregion

    }
}
