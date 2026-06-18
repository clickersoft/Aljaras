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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace Aljaras.MVVM.ViewModel
{
    internal partial class GlobalViewModel : ObservableRecipient
    {
        public static GlobalViewModel Instance { get; set; } = new GlobalViewModel();

        #region Variables
        public delegate void TimerEvent(string sampleParam);
        public event TimerEvent? TimerEvt;
        WaveInEvent wave = new();
        WaveOut waveOut = new();
        BufferedWaveProvider? provider;
        #endregion

        #region Observable 
        [ObservableProperty]
        ObservableCollection<UserNotificationMessage> notificationList = new();

        [ObservableProperty]
        UserNotificationMessage notificationMessage = new();

        [ObservableProperty]
        private List<Holiday> holidayList = new();

        [ObservableProperty]
        private List<Alarm> alarmList = new();

        [ObservableProperty]
        AudioFileOperations audioOperations = new();

        [ObservableProperty]
        UserSettings getUserSettings = new();

        [ObservableProperty]
        public AppLanguage appLang = new();

        [ObservableProperty]
        private string systemTime = string.Empty;

        [ObservableProperty]
        private string alertStartTime = string.Empty;

        [ObservableProperty]
        private string alertEndTime = string.Empty;

        [ObservableProperty]
        private string currentAlarm = string.Empty;

        [ObservableProperty]
        private string currentAlarmTitle = string.Empty;

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
        private string showTimeLeft = string.Empty;

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
            if (AudioOperations.IsEmergency || MicDevicesList == null || SpeakerDevicesList == null || !MicDevicesList.Any() || !SpeakerDevicesList.Any()) return;
            RecordingActionVisibility = GetVisibility.Visible.ToString();
            if (File.Exists(GlobalVariables.AppLocation + "Audio\\Attention.mp3"))
                _ = AudioOperations.PlayPauseAudioFile(AudioOperations.MoveAudioFileToLibrary(GlobalVariables.AppLocation + "Audio\\Attention.mp3"), false);
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
            wave.DataAvailable += Wave_DataAvailable!;
            wave.StartRecording();
        }

        [RelayCommand]
        private void DeleteNotification(UserNotificationMessage obj) => NotificationList.Remove(obj);

        [RelayCommand]
        private void Emergency()
        {
            AudioOperations.IsEmergency = !AudioOperations.IsEmergency;
            if (AudioOperations.IsEmergency)
            {
                EmergencyActionVisibility = GetVisibility.Visible.ToString();
                StopRecording();
            }
            else EmergencyActionVisibility = GetVisibility.Hidden.ToString();
            if (!File.Exists(string.Concat(GlobalVariables.AppLocation, GetUserSettings.EmergencyAudioFileLocation)))
                GetUserSettings.EmergencyAudioFileLocation = AudioOperations.MoveAudioFileToLibrary(GlobalVariables.AppLocation + "Audio\\Emerg.mp3");
            _ = AudioOperations.PlayPauseAudioFile(GetUserSettings.EmergencyAudioFileLocation, AudioOperations.IsEmergency);
        }

        [RelayCommand]
        private void ReloadDevices() => LoadDevices();
        #endregion

        #region Functions
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

        private void Wave_DataAvailable(object sender, WaveInEventArgs e) => provider!.AddSamples(e.Buffer, 0, e.BytesRecorded);

        partial void OnGetUserSettingsChanged(UserSettings value) => AudioOperations.Repeat = value.RepeatEmergency;

        partial void OnAppLangChanged(AppLanguage value)
        {
            CurrentAlarm = value.NoMoreAlarms;
            CurrentAlarmTitle = value.NoMoreAlarms;
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
            LoadUIInfo();
            NotificationList = new();
            DateTime lastBeat = DateTime.Now;
            Task.Run(async () =>
            {
                while (true)
                {
                    DateTime now = DateTime.Now;
                    Application.Current.Dispatcher.Invoke(() => SystemTime = now.ToLongTimeString());
                    if (AlarmList != null && AlarmList.Count > 0)
                    {
                        // Fire every alarm whose time elapsed since the last beat, not only an
                        // exact-second match, so a delayed tick can no longer skip a bell.
                        List<Alarm> dueAlarms = AlarmList
                            .Where(x => x.FullTime > lastBeat && x.FullTime <= now)
                            .OrderBy(x => x.FullTime)
                            .ToList();
                        if (dueAlarms.Count > 0)
                        {
                            if (!AudioOperations.IsEmergency)
                                foreach (Alarm due in dueAlarms)
                                    StartAudio(due.AudioFileLocation);
                            LoadMonitoringAlarmCollectionData();
                        }
                    }
                    lastBeat = now;
                    await Task.Delay(1000);
                }
            });
        }

        public void LoadUIInfo()
        {
            LoadDevices();
            SetAppSettings();
            LoadMonitoringAlarmCollectionData();
        }

        public void SetAppSettings()
        {
            try
            {
                GetUserSettings = new();
                {
                    var col = GlobalVariables.db.GetCollection<UserSettings>(DbTables.UserSettings.ToString());
                    UserSettings? results = col.Find(x => x.Id == 1).FirstOrDefault();
                    if (results != null)
                        GetUserSettings = results;
                    else col.Insert(GetUserSettings);
                    if (GetUserSettings.IsFirstTimeLaunch)
                    {
                        if (!StartUpManager.IsUserAdministrator())
                        {
                            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(AppLang.RunasAdministratorMessage, AppLang.RunasAdministrator, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            if (messageBoxResult == MessageBoxResult.Yes)
                                StartUpManager.RelaunchAsAdministrator();
                        }
                        else
                        {
                            try { StartUpManager.AddApplicationToAllUsersStartup(); }
                            catch { NewNotificationMessage(MessageBackground.IndianRed, AppLang.RegistryFailed); }
                            ShortcutManager.CreateDesktopShortcut(true);
                            GetUserSettings.IsFirstTimeLaunch = false;
                            col.Update(GetUserSettings);
                        }
                    }
                }
                if (File.Exists(GlobalVariables.AppLocation + "Languages\\" + GetUserSettings.CurrentLang + ".xml"))
                {
                    XmlSerializer reader = new(typeof(UserSettings));
                    reader = new XmlSerializer(typeof(AppLanguage));
                    StreamReader file = new(GlobalVariables.AppLocation + "Languages\\" + GetUserSettings.CurrentLang + ".xml");
                    AppLang = (AppLanguage)reader.Deserialize(file)!;
                    file.Close();
                }
            }
            catch { }
        }

        public static DateTime TrimMilliseconds(DateTime dt) => new(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0, dt.Kind);

        void StartAudio(string _afl)
        {
            var fileLocation = new string[] { _afl, GlobalVariables.AppLocation + "Audio\\School.mp3" }.FirstOrDefault(s => !string.IsNullOrEmpty(s) && File.Exists(s)) ?? string.Empty;
            if (string.IsNullOrEmpty(fileLocation))
            {
                NotificationMessage = new()
                {
                    BackgroundColor = MessageBackground.IndianRed.ToString(),
                    MessageText = AppLang.NotCorrectAudio
                };
                return;
            }
            _afl = AudioOperations.MoveAudioFileToLibrary(_afl);
            AudioOperations.Output = null;
            _ = AudioOperations.PlayPauseAudioFile(fileLocation, false);
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

        public static DateTime ChangeDateOnly(DateTime oldDateTime) => DateTime.Now.Date + oldDateTime.TimeOfDay;

        public static object GetPropValue(object src, string propName) => src.GetType().GetProperty(propName)!.GetValue(src, null)!;

        public void LoadMonitoringAlarmCollectionData()
        {
            ScheduleList = new();
            AlarmList = new();
            HolidayList = new();
                {
                    var holidayCollection = GlobalVariables.db.GetCollection<Holiday>(DbTables.Holidays.ToString());
                    HolidayList = holidayCollection.Find(h => h.HolidayDate >= DateTime.Now && h.IsHolidayActive).OrderBy(x => x.HolidayDate).ToList();
                    if (HolidayList != null && HolidayList.Count > 0)
                    {
                        List<Holiday> _tmp = HolidayList.FindAll(h => h.IsReminderActive && h.ReminderDate >= DateTime.Now);
                        if (_tmp != null && _tmp.Count > 0)
                            foreach (var item in _tmp)
                            {
                                Alarm _alr = new()
                                {
                                    AlarmTitle = item.HolidayTitle,
                                    FullTime = item.ReminderDate,
                                    AudioFileLocation = item.ReminderAudioFileLocation
                                };
                                AlarmList.Add(_alr);
                            }
                    }
                    if (HolidayList != null && HolidayList.Count > 0)
                        IsNOHolidayMessageVisible = GetVisibility.Hidden.ToString();
                    else IsNOHolidayMessageVisible = GetVisibility.Visible.ToString();
                    var scheduleCollection = GlobalVariables.db.GetCollection<Schedule>(DbTables.Schedules.ToString());
                    ScheduleList = scheduleCollection.Find(x => x.IsScheduleActive == true).ToList();
                    if (ScheduleList != null && ScheduleList.Count > 0)
                    {
                        foreach (Schedule item in ScheduleList)
                        {
                            var alarmCollection = GlobalVariables.db.GetCollection<Alarm>(DbTables.Alarms.ToString());
                            List<Alarm> result = alarmCollection.Find(x => x.ScheduleId == item.ScheduleId && x.IsAlarmActive == true).ToList();
                            if (result != null && result.Count > 0)
                            {
                                foreach (Alarm _item in result)
                                    if ((bool)GetPropValue(_item, DateTime.Now.DayOfWeek.ToString()[..3]))
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
                            NextAlarm();
                            return;
                        }
                    }
                    NextAlarm();
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

        partial void OnNotificationMessageChanged(UserNotificationMessage value)
        {
            Task.Run(async () =>
            {
                await Task.Delay(3000);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (NotificationList != null && NotificationList.Count > 0)
                        NotificationList.Remove(value);
                }));
            });
        }

        public void NewNotificationMessage(MessageBackground background, string messageText)
        {
            NotificationMessage = new()
            {
                BackgroundColor = background.ToString(),
                MessageText = messageText
            };
            NotificationList.Add(NotificationMessage);
        }
        #endregion
    }
}
