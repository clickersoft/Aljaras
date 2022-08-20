using CommunityToolkit.Mvvm.ComponentModel;

namespace Aljaras.MVVM.Model
{
    internal partial class Schedule : ObservableRecipient
    {
        [ObservableProperty]
        private long scheduleId = 0;

        [ObservableProperty]
        private string scheduleTitle = "";

        [ObservableProperty]
        private bool isScheduleActive = true;
    }
}
