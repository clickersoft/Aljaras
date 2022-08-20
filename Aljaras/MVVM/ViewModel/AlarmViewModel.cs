using Aljaras.Core;
using Aljaras.MVVM.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace Aljaras.MVVM.ViewModel
{
    internal partial class AlarmViewModel : ObservableRecipient
    {
        public GlobalViewModel Global { get; } = GlobalViewModel.Instance;

        #region Observable Properties
        [ObservableProperty]
        private List<Schedule> scheduleList = new();

        [ObservableProperty]
        private List<Alarm> alarmList = new();

        [ObservableProperty]
        private Alarm currentAlarm = new();

        [ObservableProperty]
        private TimePicker timePicker = new();

        [ObservableProperty]
        private string isNOScheduleMessageVisible;

        [ObservableProperty]
        private string isNOAlarmMessageVisible;

        [ObservableProperty]
        private bool enableScheduleTitleTB = true;

        [ObservableProperty]
        private long playPauseAlarmButton;

        [ObservableProperty]
        private Schedule currentSchedule = new();
        #endregion

        #region RelayCommands
        [RelayCommand]
        private void CloneSchedule(Schedule obj)
        {
            using (var db = new LiteDatabase(@"Filename=Aljaras.jrsdb;connection=shared"))
            {
                var scheduleCol = db.GetCollection<Schedule>("Schedules");
                var aLarmCol = db.GetCollection<Alarm>("Alarms");
                List<Alarm> _aResult = aLarmCol.Find(x => x.ScheduleId.ToString().Contains(obj.ScheduleId.ToString())).ToList();
                obj.ScheduleId = DateTime.Now.Ticks;
                obj.ScheduleTitle = obj.ScheduleTitle + "Clone";
                if (_aResult != null && _aResult.Count > 0)
                    foreach (var _item in _aResult)
                    {
                        _item.AlarmId = DateTime.Now.Ticks;
                        _item.ScheduleId = obj.ScheduleId;
                    }

                scheduleCol.Insert(obj);
                aLarmCol.Insert(_aResult);
            }
            LoadScheduleCollectionData();
            CallGlobal();
            Global.NotificationMessage = new()
            {
                BackgroundColor = MessageBackground.MediumSeaGreen.ToString(),
                MessageText = Global.AppLang.Done
            };
            GlobalViewModel.Instance.NotificationList.Add(Global.NotificationMessage);
        }

        [RelayCommand]
        private void CloneAlarm(Alarm obj)
        {
            using (var db = new LiteDatabase(@"Filename=Aljaras.jrsdb;connection=shared"))
            {
                var aLarmCol = db.GetCollection<Alarm>("Alarms");
                Alarm tmp = obj;
                tmp.AlarmId = DateTime.Now.Ticks;
                tmp.AlarmTitle = obj.AlarmTitle + "Clone";
                aLarmCol.Insert(tmp);
            }
            LoadAlarmCollectionData(CurrentSchedule.ScheduleId);
            CallGlobal();
            Global.NotificationMessage = new()
            {
                BackgroundColor = MessageBackground.SeaGreen.ToString(),
                MessageText = Global.AppLang.Done
            };
            GlobalViewModel.Instance.NotificationList.Add(Global.NotificationMessage);
        }
        [RelayCommand]
        private void EnableAddNewSchedule()
        {
            CurrentSchedule = new();
            EnableScheduleTitleTB = true;
        }

        [RelayCommand]
        private void NewAlarm(){CurrentAlarm = new();}
        
        [RelayCommand]
        private void TimeNow()
        {
            DateTime _dt = DateTime.Now;
            CurrentAlarm.Hour = _dt.ToString("hh"); 
            CurrentAlarm.Minute = _dt.ToString("mm");
            CurrentAlarm.DayTime = _dt.ToString("tt");
        }

        [RelayCommand]
        private void SaveAlarm()
        {
            if (CurrentSchedule != null && CurrentSchedule.ScheduleId > 0)
            {
                if (CurrentAlarm != null && !string.IsNullOrEmpty(CurrentAlarm.AlarmTitle) && !string.IsNullOrWhiteSpace(CurrentAlarm.AlarmTitle))
                {
                    var fileLocation = new string[] { CurrentAlarm.AudioFileLocation, AppDomain.CurrentDomain.BaseDirectory + "Audio\\School.mp3" }.FirstOrDefault(s => !string.IsNullOrEmpty(s) && File.Exists(s)) ?? "";
                    if (string.IsNullOrEmpty(fileLocation))
                    {
                        Global.NotificationMessage = new()
                        {
                            BackgroundColor = MessageBackground.IndianRed.ToString(),
                            MessageText = Global.AppLang.NotCorrectAudio
                        };
                        GlobalViewModel.Instance.NotificationList.Add(Global.NotificationMessage);
                        return;
                    }
                    using (var db = new LiteDatabase(@"Filename=Aljaras.jrsdb;connection=shared"))
                    {
                        CurrentAlarm.FullTime = DateTime.Parse(CurrentAlarm.Hour + ":" + CurrentAlarm.Minute + " " + CurrentAlarm.DayTime);
                        CurrentAlarm.AudioFileLocation = fileLocation;

                        var col = db.GetCollection<Alarm>("Alarms");
                        if (CurrentAlarm.AlarmId > 0)
                            col.Update(CurrentAlarm);
                        else
                        {
                            CurrentAlarm.AlarmId = DateTime.Now.Ticks;
                            CurrentAlarm.ScheduleId = CurrentSchedule.ScheduleId;
                            col.Insert(CurrentAlarm);
                        }
                    }
                    LoadAlarmCollectionData(CurrentSchedule.ScheduleId);
                }
                else 
                {
                Global.NotificationMessage = new()
                    {
                        BackgroundColor = MessageBackground.IndianRed.ToString(),
                        MessageText = Global.AppLang.InvalidTitle
                    };
                    GlobalViewModel.Instance.NotificationList.Add(Global.NotificationMessage);
                }
            }
            else
            {
                Global.NotificationMessage = new()
                {
                    BackgroundColor = MessageBackground.IndianRed.ToString(),
                    MessageText = Global.AppLang.SelectSchedule
                };
                GlobalViewModel.Instance.NotificationList.Add(Global.NotificationMessage);
                return;
            }
            CurrentAlarm = new();
            CallGlobal();
        }

        [RelayCommand]
        private void SelectAudioFile()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            string path = AppDomain.CurrentDomain.BaseDirectory + "Audio"; // this is the path that you are checking.
            if (Directory.Exists(path))
                openFileDialog.InitialDirectory = path;
            openFileDialog.Filter = "Audio File (*.mp3;*.wav)|*.mp3;*.wav;";
            if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            if(!Global.AudioPlayer.IsEmergency)
            Global.AudioPlayer.PlayPauseAudioFile(openFileDialog.FileName, false);
            CurrentAlarm.AudioFileLocation = openFileDialog.FileName;
        }

        [RelayCommand]
        void PlayPauseAudioFile()
        {
            if (!Global.AudioPlayer.IsEmergency)
                Global.AudioPlayer.PlayPauseAudioFile(CurrentAlarm.AudioFileLocation, false);
        }

        [RelayCommand]
        private void SaveSchedule()
        {
            if (CurrentSchedule != null && !string.IsNullOrEmpty(CurrentSchedule.ScheduleTitle) && !string.IsNullOrWhiteSpace(CurrentSchedule.ScheduleTitle))
            {
                using (var db = new LiteDatabase(@"Filename=Aljaras.jrsdb;connection=shared"))
                {
                    Schedule _sch = CurrentSchedule;
                    // Get a collection (or create, if doesn't exist)
                    var col = db.GetCollection<Schedule>("Schedules");
                    if (CurrentSchedule.ScheduleId.ToString() == "0")
                    {
                        _sch.ScheduleId = DateTime.Now.Ticks;
                        col.Insert(_sch);
                    }
                    else col.Update(_sch);
                }
                Global.NotificationMessage = new()
                {
                    BackgroundColor = MessageBackground.MediumSeaGreen.ToString(),
                    MessageText = Global.AppLang.Done
                };
                GlobalViewModel.Instance.NotificationList.Add(Global.NotificationMessage);
            }
            else 
            {
                Global.NotificationMessage = new()
                {
                    BackgroundColor = MessageBackground.IndianRed.ToString(),
                    MessageText = Global.AppLang.InvalidTitle
                };
                GlobalViewModel.Instance.NotificationList.Add(Global.NotificationMessage);
            }
            LoadScheduleCollectionData();
            CurrentSchedule = new();
            CallGlobal();
        }

        [RelayCommand]
        private void EditSchedule(Schedule obj)
        {
            CurrentSchedule = obj;
            EnableScheduleTitleTB = true;
        }
  
        [RelayCommand]
        private void DeleteAlarm(Alarm obj)
        {
            using (var db = new LiteDatabase(@"Filename=Aljaras.jrsdb;connection=shared"))
            {
                var col = db.GetCollection<Alarm>("Alarms");
                col.Delete(obj.AlarmId);
                Global.NotificationMessage = new()
                {
                    BackgroundColor = MessageBackground.IndianRed.ToString(),
                    MessageText = Global.AppLang.Done
                };
                GlobalViewModel.Instance.NotificationList.Add(Global.NotificationMessage);
            }
            LoadAlarmCollectionData(CurrentSchedule.ScheduleId);
            CurrentAlarm = new();
            CallGlobal();
        }

        [RelayCommand]
        private void DeleteSchedule(Schedule obj)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show(Global.AppLang.DeleteNotification, Global.AppLang.Delete +"\"" + obj.ScheduleTitle+"\"", MessageBoxButton.YesNo,MessageBoxImage.Warning);
            if (messageBoxResult != MessageBoxResult.Yes)
                return;
                using (var db = new LiteDatabase(@"Filename=Aljaras.jrsdb;connection=shared"))
                {
                    var aLarmCol = db.GetCollection<Alarm>("Alarms");
                    List<Alarm> _aResult = aLarmCol.Find(x => x.ScheduleId.ToString().Contains(obj.ScheduleId.ToString())).ToList();
                    if (_aResult != null && _aResult.Count > 0)
                    foreach (var _item in _aResult)
                            aLarmCol.Delete(_item.AlarmId);

                    var scheduleCol = db.GetCollection<Schedule>("Schedules");
                    Schedule _sResults = scheduleCol.Find(x => x.ScheduleId.ToString().Contains(obj.ScheduleId.ToString())).FirstOrDefault();
                    if (_sResults != null)
                        scheduleCol.Delete(_sResults.ScheduleId);
                }
            Global.NotificationMessage = new()
            {
                BackgroundColor = MessageBackground.IndianRed.ToString(),
                MessageText = Global.AppLang.Done
            };
            GlobalViewModel.Instance.NotificationList.Add(Global.NotificationMessage);
            LoadScheduleCollectionData();
                AlarmList = new();
                IsNOAlarmMessageVisible = "Visible";
            CurrentSchedule = new();
            CallGlobal();
        }

        [RelayCommand]
        private void De_ActivateSchedule(Schedule obj)
        {
            using (var db = new LiteDatabase(@"Filename=Aljaras.jrsdb;connection=shared"))
            {
                var scheduleCol = db.GetCollection<Schedule>("Schedules");
                Schedule _sResults = scheduleCol.Find(x => x.ScheduleId.ToString().Contains(obj.ScheduleId.ToString())).FirstOrDefault();
                _sResults.IsScheduleActive = obj.IsScheduleActive;
                if (_sResults != null)
                    scheduleCol.Update(_sResults);
            }
            Global.NotificationMessage = new()
            {
                BackgroundColor = MessageBackground.IndianRed.ToString(),
                MessageText = Global.AppLang.Done
            };
            GlobalViewModel.Instance.NotificationList.Add(Global.NotificationMessage);
            CallGlobal();
        }

        [RelayCommand]
        private void De_ActivateAlarm(Alarm obj)
        {
            using (var db = new LiteDatabase(@"Filename=Aljaras.jrsdb;connection=shared"))
            {
                var alarmCol = db.GetCollection<Alarm>("Alarms");
                Alarm _aResults = alarmCol.Find(x => x.AlarmId.ToString().Contains(obj.AlarmId.ToString())).FirstOrDefault();
                _aResults.IsAlarmActive = obj.IsAlarmActive;
                if (_aResults != null)
                    alarmCol.Update(_aResults);
            }
            Global.NotificationMessage = new()
            {
                BackgroundColor = MessageBackground.IndianRed.ToString(),
                MessageText = Global.AppLang.Done
            };
            GlobalViewModel.Instance.NotificationList.Add(Global.NotificationMessage);
            CallGlobal();
        }
        #endregion

        #region Functions
        public AlarmViewModel()
        {
            LoadScheduleCollectionData();
            LoadAlarmCollectionData(CurrentSchedule.ScheduleId);
        }

        partial void OnCurrentScheduleChanged(Schedule value)
        {
            if (CurrentSchedule != null && CurrentSchedule.ScheduleId > 0)
            {
                LoadAlarmCollectionData(CurrentSchedule.ScheduleId);
                EnableScheduleTitleTB = false;
            }else EnableScheduleTitleTB = true;

            if (CurrentAlarm == null)
                CurrentAlarm = new();
        }

        private static void CallGlobal()
        {
            GlobalViewModel.Instance.LoadMonitoringAlarmCollectionData();
            GlobalViewModel.Instance.NextAlarm();
        }

        private void LoadScheduleCollectionData()
        {
            using (var db = new LiteDatabase(@"Filename=Aljaras.jrsdb;connection=shared"))
            {
                var col = db.GetCollection<Schedule>("Schedules");
                ScheduleList = col.Query().ToList();
            }
            if (ScheduleList != null && ScheduleList.Count > 0)
                IsNOScheduleMessageVisible = "Hidden";
            else IsNOScheduleMessageVisible = "Visible";
        }

        private void LoadAlarmCollectionData(long _SId)
        {
            AlarmList = new();
            using (var db = new LiteDatabase(@"Filename=Aljaras.jrsdb;connection=shared"))
            {
                var col = db.GetCollection<Alarm>("Alarms");
                AlarmList = col.Find(x => x.ScheduleId.ToString().Contains(CurrentSchedule.ScheduleId.ToString())).ToList().OrderBy(x => x.FullTime).ToList();;
            }
            if (AlarmList != null && AlarmList.Count > 0)
                IsNOAlarmMessageVisible = "Hidden";
            else IsNOAlarmMessageVisible = "Visible";
        }
        #endregion
    }
}
