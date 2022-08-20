using CommunityToolkit.Mvvm.ComponentModel;

namespace Aljaras.MVVM.Model
{
    public partial class AppLanguage : ObservableRecipient
    {
        [ObservableProperty]
        private string _apptitle = "Aljaras: School Bell System Version";

        [ObservableProperty]
        private string _flowDirection = "LeftToRight";

        [ObservableProperty]
        private string _menu = "Menu";

        [ObservableProperty]
        private string _monitoring = "Monitoring";

        [ObservableProperty]
        private string _alarm = "Alarm";

        [ObservableProperty]
        private string _settings = "Settings";

        [ObservableProperty]
        private string _about = "About";

        [ObservableProperty]
        private string _help = "Help";

        [ObservableProperty]
        private string _upcomingAlerts = "Upcoming Alerts";

        [ObservableProperty]
        private string _scheduleList = "Schedule List";

        [ObservableProperty]
        private string _alarmList = "Alarm List";

        [ObservableProperty]
        private string _alertInformation = "Alert Information";

        [ObservableProperty]
        private string _title = "Title";

        [ObservableProperty]
        private string _days = "Days";

        [ObservableProperty]
        private string _fri = "Fri";

        [ObservableProperty]
        private string _sat = "Sat";

        [ObservableProperty]
        private string _sun = "Sun";

        [ObservableProperty]
        private string _mon = "Mon";

        [ObservableProperty]
        private string _tue = "Tue";

        [ObservableProperty]
        private string _wed = "Wed";

        [ObservableProperty]
        private string _thu = "Thu";

        [ObservableProperty]
        private string _new = "New";
        
        [ObservableProperty]
        private string _edit = "Edit";

        [ObservableProperty]
        private string _save = "Save";

        [ObservableProperty]
        private string _delete = "Delete";

        [ObservableProperty]
        private string _timeNow = "TimeNow";

        [ObservableProperty]
        private string _playStop = "Play/Stop";

        [ObservableProperty]
        private string _hour = "Hour";

        [ObservableProperty]
        private string _minute = "Minute";

        [ObservableProperty]
        private string _daytime = "Daytime";

        [ObservableProperty]
        private string _time = "Time";

        [ObservableProperty]
        private string _audioFile = "AudioFile";

        [ObservableProperty]
        private string _language = "Language";

        [ObservableProperty]
        private string _guide = "Guide";

        [ObservableProperty]
        private string _monitoringNOAlarmMessage = "There are no upcoming alarm click the alarm icon ⏰ in the menu to get started.";

        [ObservableProperty]
        private string _nOScheduleMessage = "You haven't set up any schedules add new title and click the save button to get started.";

        [ObservableProperty]
        private string _nOAlarmMessage = "You haven't set up any alarm click the save button above to get started.";

        [ObservableProperty]
        private string _systemTime = "System Time:";

        [ObservableProperty]
        private string _nextAlarm = "Next Alarm:";

        [ObservableProperty]
        private string _timeLeft = "Time Left:";

        [ObservableProperty]
        private string _emergency = "Emergency";

        [ObservableProperty]
        private string _selectFile = "Select File";

        [ObservableProperty]
        private string _repeatEmergency = "Repeat Emergency";

        [ObservableProperty]
        private string _enableRepeat = "Enable Repeat";

        [ObservableProperty]
        private string _noMoreAlarms = "No more alarms...";

        [ObservableProperty]
        private string _deleteNotification = "Are you sure?";

        [ObservableProperty]
        private string _database = "Database";

        [ObservableProperty]
        private string _databaseOperations = "Database Operations";

        [ObservableProperty]
        private string _import = "Import";

        [ObservableProperty]
        private string _export = "Export";

        [ObservableProperty]
        private string _done = "Done";

        [ObservableProperty]
        private string _noDataBase = "There is no DataBase";

        [ObservableProperty]
        private string _notCorrectAudio = "Not a correct audio file type";

        [ObservableProperty]
        private string _invalidTitle = "invalid title value";

        [ObservableProperty]
        private string _selectSchedule = "Select a Schedule First";

        [ObservableProperty]
        private string _holidays = "Holidays";

        [ObservableProperty]
        private string _holidayInformation = "Holiday Information";

        [ObservableProperty]
        private string _holidayDate = "Holiday Date";

        [ObservableProperty]
        private string _enableReminder = "Enable Reminder";

        [ObservableProperty]
        private string _reminderDate = "Reminder Date";

        [ObservableProperty]
        private string _list = "List";

        [ObservableProperty]
        private string _clone = "Clone";

        [ObservableProperty]
        private string _nOHolidayMessage = "You haven't set up any 🌴 Holiday.";

        [ObservableProperty]
        private string _intercom = "Intercom";

        [ObservableProperty]
        private string _recordingDevices = "Recording Devices";

        [ObservableProperty]
        private string _playbackDevices = "Playback Devices";

        [ObservableProperty]
        private string _start = "Start";

        [ObservableProperty]
        private string _stop = "Stop";

        [ObservableProperty]
        private string _itemsCount = "Items Count";
    }
}
