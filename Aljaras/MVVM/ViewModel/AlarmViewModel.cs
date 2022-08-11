using Aljaras.Core;
using Aljaras.MVVM.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Controls.Primitives;

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
        private bool enableScheduleTitleTB = false;

        [ObservableProperty]
        private long playPauseAlarmButton;

        [ObservableProperty]
        private Schedule currentSchedule = new();
        #endregion

        #region RelayCommands
        [RelayCommand]
        private void ReloadAlarm()
        {
            if (CurrentSchedule != null && CurrentSchedule.ScheduleId > 0)
                LoadAlarmCollectionData(CurrentSchedule.ScheduleId);
        }

        [RelayCommand]
        private void EnableAddNewSchedule()
        {
            CurrentSchedule = new Schedule();
            EnableScheduleTitleTB = true;
        }

        [RelayCommand]
        private void NewAlarm(){CurrentAlarm = new Alarm();}
        
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
                        MessageBox.Show("Not a correct audio file type or location.");
                        return;
                    }
                    // Open database (or create if doesn't exist)
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
                    MessageBox.Show("Done");
                    LoadAlarmCollectionData(CurrentSchedule.ScheduleId);
                }else MessageBox.Show("invalid title value.");
            }
            else
            {
                MessageBox.Show("Select a Schedule First.");
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
                // Open database (or create if doesn't exist)
                // LiteDatabase(@"Filename=Aljaras.jrsdb;Password='mustafa';connection=shared");
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
            }
            else
            {
                MessageBox.Show("Please Enter a valid value");
            }
            LoadScheduleCollectionData();
            EnableScheduleTitleTB = false;
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
        private void DeleteAlarm()
        {
            if (CurrentAlarm != null && CurrentAlarm.AlarmId > 0)
            {
                // Open database (or create if doesn't exist)
                using (var db = new LiteDatabase(@"Filename=Aljaras.jrsdb;connection=shared"))
                {
                    var col = db.GetCollection<Alarm>("Alarms");
                    col.Delete(CurrentAlarm.AlarmId);
                    MessageBox.Show("Done");
                    //((MainWindow)System.Windows.Application.Current.MainWindow).RefreshPage();
                }
            }
            else MessageBox.Show("Select alarm to delete");
            LoadAlarmCollectionData(CurrentSchedule.ScheduleId);
            CurrentAlarm = new();
            CallGlobal();
        }

        [RelayCommand]
        private void DeleteSchedule(Schedule obj)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show(Global.AppLang.DeleteScheduleNotification, Global.AppLang.Delete +"\"" + obj.ScheduleTitle+"\"", MessageBoxButton.YesNo,MessageBoxImage.Warning);
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
                    MessageBox.Show("Done");
                }
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
                MessageBox.Show("Done");
            }
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
                LoadAlarmCollectionData(CurrentSchedule.ScheduleId);
            EnableScheduleTitleTB = false;
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
            else
            {
                IsNOScheduleMessageVisible = "Visible";
                EnableScheduleTitleTB = true;
            }
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
