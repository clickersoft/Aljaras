using Aljaras.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Aljaras.MVVM.Model
{
    internal partial class Alarm : ObservableRecipient
    {
        [ObservableProperty]
        private long alarmId = 0;

        [ObservableProperty]
        private string alarmTitle = string.Empty;

        [ObservableProperty]
        private long scheduleId = 0;

        [ObservableProperty]
        private bool sun = true;

        [ObservableProperty]
        private bool mon = true;

        [ObservableProperty]
        private bool tue = true;

        [ObservableProperty]
        private bool wed = true;

        [ObservableProperty]
        private bool thu = true;

        [ObservableProperty]
        private bool fri = true;

        [ObservableProperty]
        private bool sat = true;

        [ObservableProperty]
        private string hour = "01";

        partial void OnHourChanged(string value) => FullTime = DateTime.Parse(value + ":" + Minute + " " + DayTime);

        [ObservableProperty]
        private string minute = "00";

        partial void OnMinuteChanged(string value) => FullTime = DateTime.Parse(Hour + ":" + value + " " + DayTime);

        [ObservableProperty]
        private GetDayTime dayTime = GetDayTime.AM;

        partial void OnDayTimeChanged(GetDayTime value) => FullTime = DateTime.Parse(Hour + ":" + Minute + " " + value);

        [ObservableProperty]
        private DateTime fullTime;

        [ObservableProperty]
        private string audioFileLocation = string.Empty;

        [ObservableProperty]
        private bool isAlarmActive = true;

        [ObservableProperty]
        private int volume = 100;

        /// <summary>
        /// Playback volume as a 0..1 fraction. Alarms saved before this field
        /// existed deserialize as 0; treat that (and any non-positive value) as
        /// full volume so legacy alarms keep ringing at 100%.
        /// </summary>
        public float VolumeFraction => Volume <= 0 ? 1f : Math.Min(Volume, 100) / 100f;
    }
}
