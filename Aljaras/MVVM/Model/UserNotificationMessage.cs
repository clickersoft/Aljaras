using Aljaras.Core;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Aljaras.MVVM.Model
{
    public partial class UserNotificationMessage : ObservableRecipient
    {
        [ObservableProperty]
        private string _backgroundColor = GetVisibility.Hidden.ToString();

        [ObservableProperty]
        private string _messageText = "";
    }
}
