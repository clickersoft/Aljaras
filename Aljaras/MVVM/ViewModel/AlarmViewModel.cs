using Aljaras.Core;
using Aljaras.MVVM.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Shapes;
using Path = System.IO.Path;

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
        private string isNOScheduleMessageVisible = GetVisibility.Visible.ToString();

        [ObservableProperty]
        private string isNOAlarmMessageVisible = GetVisibility.Visible.ToString();

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
            using (App.db)
            {
                var scheduleCol = App.db.GetCollection<Schedule>("Schedules");
                var aLarmCol = App.db.GetCollection<Alarm>("Alarms");
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
        private void PlayAlarmAudio(Alarm obj)
        {
            if (!Global.AudioOperations.IsEmergency)
                _ = Global.AudioOperations.PlayPauseAudioFile(obj.AudioFileLocation, false);
        }

        [RelayCommand]
        private void CloneAlarm(Alarm obj)
        {
            using (App.db)
            {
                var aLarmCol = App.db.GetCollection<Alarm>("Alarms");
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
                    fileLocation = Global.AudioOperations.MoveAudioFileToLibrary(fileLocation);
                    using (App.db)
                    {
                        CurrentAlarm.FullTime = DateTime.Parse(CurrentAlarm.Hour + ":" + CurrentAlarm.Minute + " " + CurrentAlarm.DayTime);
                        CurrentAlarm.AudioFileLocation = fileLocation;

                        var col = App.db.GetCollection<Alarm>("Alarms");
                        if (CurrentAlarm.AlarmId > 0)
                            col.Update(CurrentAlarm);
                        else
                        {
                            CurrentAlarm.AlarmId = DateTime.Now.Ticks;
                            CurrentAlarm.ScheduleId = CurrentSchedule.ScheduleId;
                            col.Insert(CurrentAlarm);
                        }
                        Global.NotificationMessage = new()
                        {
                            BackgroundColor = MessageBackground.SeaGreen.ToString(),
                            MessageText = Global.AppLang.Done
                        };
                        GlobalViewModel.Instance.NotificationList.Add(Global.NotificationMessage);
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
            OpenFileDialog openFileDialog = new();
            string path = AppDomain.CurrentDomain.BaseDirectory + "Audio";
            if (Directory.Exists(path))
                openFileDialog.InitialDirectory = path;
            openFileDialog.Filter = "Audio File (*.mp3;*.wav)|*.mp3;*.wav;";
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;
            string newPath = Global.AudioOperations.MoveAudioFileToLibrary(openFileDialog.FileName);
            if (!Global.AudioOperations.IsEmergency)
                _ = Global.AudioOperations.PlayPauseAudioFile(newPath, false);
            CurrentAlarm.AudioFileLocation = newPath;
        }

        [RelayCommand]
        private void PlayPauseAudioFile()
        {
            if (!Global.AudioOperations.IsEmergency)
                _ = Global.AudioOperations.PlayPauseAudioFile(CurrentAlarm.AudioFileLocation, false);
        }

        [RelayCommand]
        private void SaveSchedule()
        {
            if (CurrentSchedule != null && !string.IsNullOrEmpty(CurrentSchedule.ScheduleTitle) && !string.IsNullOrWhiteSpace(CurrentSchedule.ScheduleTitle))
            {
                using (App.db)
                {
                    Schedule _sch = CurrentSchedule;
                    // Get a collection (or create, if doesn't exist)
                    var col = App.db.GetCollection<Schedule>("Schedules");
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
            using (App.db)
            {
                var col = App.db.GetCollection<Alarm>("Alarms");
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
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(Global.AppLang.DeleteNotification, Global.AppLang.Delete +"\"" + obj.ScheduleTitle+"\"", MessageBoxButton.YesNo,MessageBoxImage.Warning);
            if (messageBoxResult != MessageBoxResult.Yes)
                return;
                using (App.db)
                {
                    var aLarmCol = App.db.GetCollection<Alarm>("Alarms");
                    List<Alarm> _aResult = aLarmCol.Find(x => x.ScheduleId.ToString().Contains(obj.ScheduleId.ToString())).ToList();
                    if (_aResult != null && _aResult.Count > 0)
                    foreach (var _item in _aResult)
                            aLarmCol.Delete(_item.AlarmId);
                    var scheduleCol = App.db.GetCollection<Schedule>("Schedules");
                        scheduleCol.Delete(obj.ScheduleId);
                }
            Global.NotificationMessage = new()
            {
                BackgroundColor = MessageBackground.IndianRed.ToString(),
                MessageText = Global.AppLang.Done
            };
            GlobalViewModel.Instance.NotificationList.Add(Global.NotificationMessage);
            LoadScheduleCollectionData();
                AlarmList = new();
                IsNOAlarmMessageVisible = GetVisibility.Visible.ToString();
            CurrentSchedule = new();
            CallGlobal();
        }

        [RelayCommand]
        private void De_ActivateSchedule(Schedule obj)
        {
            using (App.db)
            {
                var scheduleCol = App.db.GetCollection<Schedule>("Schedules");
                    scheduleCol.Update(obj);
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
            using (App.db)
            {
                var alarmCol = App.db.GetCollection<Alarm>("Alarms");
                    alarmCol.Update(obj);
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
            using (App.db)
            {
                var col = App.db.GetCollection<Schedule>("Schedules");
                ScheduleList = col.Query().ToList();
            }
            if (ScheduleList != null && ScheduleList.Count > 0)
                IsNOScheduleMessageVisible = GetVisibility.Hidden.ToString();
            else IsNOScheduleMessageVisible = GetVisibility.Visible.ToString();
        }

        private void LoadAlarmCollectionData(long _SId)
        {
            AlarmList = new();
            using (App.db)
            {
                var col = App.db.GetCollection<Alarm>("Alarms");
                AlarmList = col.Find(x => x.ScheduleId.ToString().Contains(CurrentSchedule.ScheduleId.ToString())).ToList().OrderBy(x => x.FullTime.TimeOfDay).ToList();;
            }
            if (AlarmList != null && AlarmList.Count > 0)
                IsNOAlarmMessageVisible = GetVisibility.Hidden.ToString();
            else IsNOAlarmMessageVisible = GetVisibility.Visible.ToString();
        }
        #endregion

    }
}
