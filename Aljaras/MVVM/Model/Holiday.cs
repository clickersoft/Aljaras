using Aljaras.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows.Media.Animation;

namespace Aljaras.MVVM.Model
{
    internal partial class Holiday : ObservableRecipient
    {
        [ObservableProperty]
        private long holidayId = 0;

        [ObservableProperty]
        private string holidayTitle = string.Empty;

        [ObservableProperty]
        private DateTime holidayDate = DateTime.Now;

        [ObservableProperty]
        private bool isHolidayActive = true;

        [ObservableProperty]
        private bool isReminderActive = false;

        partial void OnIsReminderActiveChanged(bool value) => ReminderVisibility = value ? GetVisibility.Visible.ToString() : GetVisibility.Hidden.ToString();

        [ObservableProperty]
        private DateTime reminderDate = DateTime.Now.AddDays(7);

        [ObservableProperty]
        private string reminderHour = "01";

        partial void OnReminderHourChanged(string value) => ReminderDate = DateTime.Parse(value + ":" + ReminderMinute + " " + ReminderDayTime);

        [ObservableProperty]
        private string reminderMinute = "00";

        partial void OnReminderMinuteChanged(string value) => ReminderDate = DateTime.Parse(ReminderHour + ":" + value + " " + ReminderDayTime);

        [ObservableProperty]
        private GetDayTime reminderDayTime = GetDayTime.AM;

        partial void OnReminderDayTimeChanged(GetDayTime value) => ReminderDate = DateTime.Parse(ReminderHour + ":" + ReminderMinute + " " + value);

        //[ObservableProperty]
        //private DateTime fullTime;

        [ObservableProperty]
        private string reminderAudioFileLocation = string.Empty;

        [ObservableProperty]
        private string reminderVisibility = GetVisibility.Hidden.ToString();
    }
}
