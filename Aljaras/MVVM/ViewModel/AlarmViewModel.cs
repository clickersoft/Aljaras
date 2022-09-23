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
using System.Windows.Forms;

namespace Aljaras.MVVM.ViewModel
{
    internal partial class AlarmViewModel : ObservableRecipient
    {
        public GlobalViewModel Global { get; set; } = GlobalViewModel.Instance;

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
                var scheduleCol = App.db.GetCollection<Schedule>(DbTables.Schedules.ToString());
                var aLarmCol = App.db.GetCollection<Alarm>(DbTables.Alarms.ToString());
                List<Alarm> _aResult = aLarmCol.Find(x => x.ScheduleId.ToString().Contains(obj.ScheduleId.ToString())).ToList();
                obj.ScheduleId = DateTime.Now.Ticks;
                obj.ScheduleTitle += Global.AppLang.Clone;
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
            Global.LoadMonitoringAlarmCollectionData();
            Global.NewNotificationMessage(MessageBackground.MediumSeaGreen , Global.AppLang.Done);
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
                var aLarmCol = App.db.GetCollection<Alarm>(DbTables.Alarms.ToString());
                obj.AlarmId = DateTime.Now.Ticks;
                obj.AlarmTitle += Global.AppLang.Clone;
                aLarmCol.Insert(obj);
            }
            LoadAlarmCollectionData(CurrentSchedule.ScheduleId);
            Global.LoadMonitoringAlarmCollectionData();
            Global.NewNotificationMessage(MessageBackground.SeaGreen, Global.AppLang.Done);
        }

        [RelayCommand]
        private void EnableAddNewSchedule()
        {
            CurrentSchedule = new();
            EnableScheduleTitleTB = true;
        }

        [RelayCommand]
        private void NewAlarm() => CurrentAlarm = new();
        
        [RelayCommand]
        private void TimeNow()
        {
            DateTime _dt = DateTime.Now;
            CurrentAlarm.Hour = _dt.ToString("hh"); 
            CurrentAlarm.Minute = _dt.ToString("mm");
            CurrentAlarm.DayTime = (GetDayTime)Enum.Parse(typeof(GetDayTime), _dt.ToString("tt"));
        }

        [RelayCommand]
        private void SaveAlarm()
        {
            if (CurrentSchedule != null && CurrentSchedule.ScheduleId > 0)
            {
                if (CurrentAlarm != null && !string.IsNullOrEmpty(CurrentAlarm.AlarmTitle) && !string.IsNullOrWhiteSpace(CurrentAlarm.AlarmTitle))
                {
                    var fileLocation = new string[] { CurrentAlarm.AudioFileLocation, App.AppLocation + "Audio\\School.mp3" }.FirstOrDefault(s => !string.IsNullOrEmpty(s) && File.Exists(s)) ?? string.Empty;
                    if (string.IsNullOrEmpty(fileLocation))
                    {
                        Global.NewNotificationMessage(MessageBackground.IndianRed, Global.AppLang.NotCorrectAudio);
                        return;
                    }
                    fileLocation = Global.AudioOperations.MoveAudioFileToLibrary(fileLocation);
                    using (App.db)
                    {
                        CurrentAlarm.AudioFileLocation = fileLocation;
                        var col = App.db.GetCollection<Alarm>(DbTables.Alarms.ToString());
                        if (CurrentAlarm.AlarmId > 0)
                            col.Update(CurrentAlarm);
                        else
                        {
                            CurrentAlarm.AlarmId = DateTime.Now.Ticks;
                            CurrentAlarm.ScheduleId = CurrentSchedule.ScheduleId;
                            col.Insert(CurrentAlarm);
                        }
                        Global.NewNotificationMessage(MessageBackground.SeaGreen, Global.AppLang.Done);
                    }
                    LoadAlarmCollectionData(CurrentSchedule.ScheduleId);
                }
                else Global.NewNotificationMessage(MessageBackground.IndianRed, Global.AppLang.InvalidTitle);
            }
            else
            {
                Global.NewNotificationMessage(MessageBackground.IndianRed, Global.AppLang.SelectSchedule);
                return;
            }
            CurrentAlarm = new();
            Global.LoadMonitoringAlarmCollectionData();
        }

        [RelayCommand]
        private void SelectAudioFile()
        {
            OpenFileDialog openFileDialog = new();
            string path = App.AppLocation + "Audio";
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
                    var col = App.db.GetCollection<Schedule>(DbTables.Schedules.ToString());
                    if (CurrentSchedule.ScheduleId == 0)
                    {
                        _sch.ScheduleId = DateTime.Now.Ticks;
                        col.Insert(_sch);
                    }
                    else col.Update(_sch);
                }
                Global.NewNotificationMessage(MessageBackground.MediumSeaGreen, Global.AppLang.Done);
            }
            else Global.NewNotificationMessage(MessageBackground.IndianRed, Global.AppLang.InvalidTitle);
            LoadScheduleCollectionData();
            CurrentSchedule = new();
            Global.LoadMonitoringAlarmCollectionData();
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
                var col = App.db.GetCollection<Alarm>(DbTables.Alarms.ToString());
                col.Delete(obj.AlarmId);
                Global.NewNotificationMessage(MessageBackground.MediumSeaGreen, Global.AppLang.Done);
            }
            LoadAlarmCollectionData(CurrentSchedule.ScheduleId);
            CurrentAlarm = new();
            Global.LoadMonitoringAlarmCollectionData();
        }

        [RelayCommand]
        private void DeleteSchedule(Schedule obj)
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(Global.AppLang.DeleteNotification, Global.AppLang.Delete +"\"" + obj.ScheduleTitle+"\"", MessageBoxButton.YesNo,MessageBoxImage.Warning);
            if (messageBoxResult != MessageBoxResult.Yes)
                return;
                using (App.db)
                {
                    var aLarmCol = App.db.GetCollection<Alarm>(DbTables.Alarms.ToString());
                    List<Alarm> _aResult = aLarmCol.Find(x => x.ScheduleId.ToString().Contains(obj.ScheduleId.ToString())).ToList();
                    if (_aResult != null && _aResult.Count > 0)
                    foreach (var _item in _aResult)
                            aLarmCol.Delete(_item.AlarmId);
                    var scheduleCol = App.db.GetCollection<Schedule>(DbTables.Schedules.ToString());
                        scheduleCol.Delete(obj.ScheduleId);
                }
            Global.NewNotificationMessage(MessageBackground.MediumSeaGreen, Global.AppLang.Done);
            LoadScheduleCollectionData();
            AlarmList = new();
            IsNOAlarmMessageVisible = GetVisibility.Visible.ToString();
            CurrentSchedule = new();
            Global.LoadMonitoringAlarmCollectionData();
        }

        [RelayCommand]
        private void De_ActivateSchedule(Schedule obj)
        {
            using (App.db)
            {
                var scheduleCol = App.db.GetCollection<Schedule>(DbTables.Schedules.ToString());
                    scheduleCol.Update(obj);
            }            
            Global.NewNotificationMessage(MessageBackground.MediumSeaGreen, Global.AppLang.Done);
            Global.LoadMonitoringAlarmCollectionData();
        }

        [RelayCommand]
        private void De_ActivateAlarm(Alarm obj)
        {
            using (App.db)
            {
                var alarmCol = App.db.GetCollection<Alarm>(DbTables.Alarms.ToString());
                    alarmCol.Update(obj);
            }
            Global.NewNotificationMessage(MessageBackground.MediumSeaGreen, Global.AppLang.Done);
            Global.LoadMonitoringAlarmCollectionData();
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
            if (value != null && value.ScheduleId > 0)
            {
                LoadAlarmCollectionData(value.ScheduleId);
                EnableScheduleTitleTB = false;
            }else EnableScheduleTitleTB = true;
            if (CurrentAlarm == null)
                CurrentAlarm = new();
        }

            

        private void LoadScheduleCollectionData()
        {
            using (App.db)
            {
                var col = App.db.GetCollection<Schedule>(DbTables.Schedules.ToString());
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
                var col = App.db.GetCollection<Alarm>(DbTables.Alarms.ToString());
                AlarmList = col.Find(x => x.ScheduleId.ToString().Contains(_SId.ToString())).ToList().OrderBy(x => x.FullTime.TimeOfDay).ToList();
            }
            if (AlarmList != null && AlarmList.Count > 0)
                IsNOAlarmMessageVisible = GetVisibility.Hidden.ToString();
            else IsNOAlarmMessageVisible = GetVisibility.Visible.ToString();
        }
        #endregion

    }
}
