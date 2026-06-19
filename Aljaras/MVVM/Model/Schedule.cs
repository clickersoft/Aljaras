using Aljaras.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Aljaras.MVVM.Model
{
    internal partial class Schedule : ObservableRecipient
    {
        [ObservableProperty]
        private long scheduleId = 0;

        [ObservableProperty]
        private string scheduleTitle = string.Empty;

        [ObservableProperty]
        private bool isScheduleActive = true;

        /// <summary>
        /// While this is in the future the schedule's alarms are skipped, then
        /// it auto-resumes. Legacy rows deserialize to MinValue (not suspended).
        /// </summary>
        [ObservableProperty]
        private DateTime suspendedUntil = DateTime.MinValue;

        public bool IsSuspended => SuspendedUntil > DateTime.Now;

        public string SuspendedVisibility => IsSuspended ? GetVisibility.Visible.ToString() : GetVisibility.Collapsed.ToString();

        /// <summary>When true the schedule only runs between StartDate and EndDate (e.g. a term).</summary>
        [ObservableProperty]
        private bool useDateRange = false;

        [ObservableProperty]
        private DateTime startDate = DateTime.Now.Date;

        [ObservableProperty]
        private DateTime endDate = DateTime.Now.Date.AddMonths(4);

        public bool IsWithinDateRange => !UseDateRange || (DateTime.Now.Date >= StartDate.Date && DateTime.Now.Date <= EndDate.Date);
    }
}
