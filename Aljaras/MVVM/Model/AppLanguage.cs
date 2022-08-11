using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private string _title = "Title:";

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
        private string _deleteScheduleNotification = "Are you sure?";

    }
}
