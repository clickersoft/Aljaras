using Aljaras.Core;
using Aljaras.MVVM.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Aljaras.MVVM.ViewModel
{
    internal partial class HolidaysViewModel : ObservableRecipient
    {
        public GlobalViewModel Global { get; set; } = GlobalViewModel.Instance;

        #region Observable 
        [ObservableProperty]
        private Holiday currentHoliday = new();

        [ObservableProperty]
        private List<Holiday> holidayList = new();

        [ObservableProperty]
        private string isNOHolidayMessageVisible = GetVisibility.Visible.ToString();

        [ObservableProperty]
        private TimePicker timePicker = new();
        #endregion

        #region RelayCommands
        [RelayCommand]
        private void NewHoliday() => CurrentHoliday = new();

        [RelayCommand]
        private void TimeNow()
        {
            DateTime _dt = DateTime.Now;
            CurrentHoliday.ReminderDate = _dt;
            CurrentHoliday.ReminderHour = _dt.ToString("hh");
            CurrentHoliday.ReminderMinute = _dt.ToString("mm");
            CurrentHoliday.ReminderDayTime = (GetDayTime)Enum.Parse(typeof(GetDayTime), _dt.ToString("tt"));
        }

        [RelayCommand]
        private void CloneHoliday(Holiday obj)
        {
            using (GlobalVariables.db)
            {
                var holidayCol = GlobalVariables.db.GetCollection<Holiday>(DbTables.Holidays.ToString());
                obj.HolidayId = DateTime.Now.Ticks;
                obj.HolidayTitle += Global.AppLang.Clone;
                holidayCol.Insert(obj);
            }
            LoadHolidayCollectionData();
            Global.LoadMonitoringAlarmCollectionData();
            Global.NewNotificationMessage(MessageBackground.MediumSeaGreen , Global.AppLang.Done);
        }

        [RelayCommand]
        void PlayPauseAudioFile()
        {
            if (!Global.AudioOperations.IsEmergency)
                _ = Global.AudioOperations.PlayPauseAudioFile(CurrentHoliday.ReminderAudioFileLocation, false);
        }

        [RelayCommand]
        private void SelectAudioFile()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new();
            string path = GlobalVariables.AppLocation + "Audio";
            if (Directory.Exists(path))
                openFileDialog.InitialDirectory = path;
            openFileDialog.Filter = "Audio File (*.mp3;*.wav)|*.mp3;*.wav;";
            if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            string newPath = Global.AudioOperations.MoveAudioFileToLibrary(openFileDialog.FileName);
            if (!Global.AudioOperations.IsEmergency)
                _ = Global.AudioOperations.PlayPauseAudioFile(newPath, false);
            CurrentHoliday.ReminderAudioFileLocation = newPath;
        }

        [RelayCommand]
        private void De_ActivateHoliday(Holiday obj)
        {
            using (GlobalVariables.db)
            {
                var holidayCol = GlobalVariables.db.GetCollection<Holiday>(DbTables.Holidays.ToString());
                holidayCol.Update(obj);
            }
            Global.NewNotificationMessage(MessageBackground.SeaGreen , Global.AppLang.Done);
            Global.LoadMonitoringAlarmCollectionData();
        }

        [RelayCommand]
        private void DeleteHoliday(Holiday obj)
        {
            using (GlobalVariables.db)
            {
                var col = GlobalVariables.db.GetCollection<Holiday>(DbTables.Holidays.ToString());
                col.Delete(obj.HolidayId);
                Global.NewNotificationMessage(MessageBackground.SeaGreen, Global.AppLang.Done);
            }
            LoadHolidayCollectionData();
            CurrentHoliday = new();
            Global.LoadMonitoringAlarmCollectionData();
        }

        [RelayCommand]
        private void SaveHoliday()
        {
            if (CurrentHoliday != null && !string.IsNullOrEmpty(CurrentHoliday.HolidayTitle) && !string.IsNullOrWhiteSpace(CurrentHoliday.HolidayTitle))
            {
                var fileLocation = new string[] { CurrentHoliday.ReminderAudioFileLocation, GlobalVariables.AppLocation + "Audio\\Attention.mp3" }.FirstOrDefault(s => !string.IsNullOrEmpty(s) && File.Exists(s)) ?? string.Empty;
                if (string.IsNullOrEmpty(fileLocation) || string.IsNullOrWhiteSpace(fileLocation))
                {
                    Global.NewNotificationMessage(MessageBackground.IndianRed, Global.AppLang.NotCorrectAudio);
                    return;
                }
                fileLocation = Global.AudioOperations.MoveAudioFileToLibrary(fileLocation);
                using (GlobalVariables.db)
                {
                    //CurrentHoliday.ReminderDate = ChangeDateOnly(CurrentHoliday.ReminderDate, DateTime.Parse(CurrentHoliday.ReminderHour + ":" + CurrentHoliday.ReminderMinute + " " + CurrentHoliday.ReminderDayTime));
                    CurrentHoliday.ReminderAudioFileLocation = fileLocation;
                    var col = GlobalVariables.db.GetCollection<Holiday>(DbTables.Holidays.ToString());
                    if (CurrentHoliday.HolidayId > 0)
                        col.Update(CurrentHoliday);
                    else
                    {
                        CurrentHoliday.HolidayId = DateTime.Now.Ticks;
                        //CurrentHoliday.FullTime = ChangeDateOnly(CurrentHoliday.ReminderDate, DateTime.Parse(CurrentHoliday.ReminderHour + ":" + CurrentHoliday.ReminderMinute + " " + CurrentHoliday.ReminderDayTime));
                        col.Insert(CurrentHoliday);
                    }
                }
                LoadHolidayCollectionData();
                Global.NewNotificationMessage(MessageBackground.SeaGreen, Global.AppLang.Done);
            }
            else Global.NewNotificationMessage(MessageBackground.IndianRed, Global.AppLang.InvalidTitle);
            CurrentHoliday = new();
            Global.LoadMonitoringAlarmCollectionData();
        }
        #endregion

        #region Functions
        private void LoadHolidayCollectionData()
        {
            using (GlobalVariables.db)
            {
                var col = GlobalVariables.db.GetCollection<Holiday>(DbTables.Holidays.ToString());
                HolidayList = col.Query().OrderBy(h => h.HolidayDate).ToList();
            }
            if (HolidayList != null && HolidayList.Count > 0)
                IsNOHolidayMessageVisible = GetVisibility.Hidden.ToString();
            else
            {
                IsNOHolidayMessageVisible = GetVisibility.Visible.ToString();
            }
        }

        public static DateTime ChangeDateOnly(DateTime _date, DateTime _time) => _date.Date + _time.TimeOfDay;

        public HolidaysViewModel() => LoadHolidayCollectionData();
        #endregion

    }
}
