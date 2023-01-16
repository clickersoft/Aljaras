using Aljaras.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace Aljaras.MVVM.ViewModel
{
    partial class ActivationViewModel : ObservableRecipient
    {
        public GlobalViewModel Global { get; set; } = GlobalViewModel.Instance;

        [ObservableProperty]
        private string generatedKey = LicenseKeyGenerator.GenerateLicenseKey(Environment.MachineName);

        [ObservableProperty]
        private string activationKey = string.Empty;

        [ObservableProperty]
        private bool isActivationEnabled = !LicenseKeyGenerator.IsProductActivated();

        [RelayCommand]
        private void CopyKey() => Clipboard.SetText(GeneratedKey);

        [RelayCommand]
        private void PasteKey() => ActivationKey = Clipboard.GetText();

        [RelayCommand]
        private void SaveKey()
        {
            if (ActivationKey == LicenseKeyGenerator.GenerateLicenseKey(GeneratedKey))
            {
                using (StreamWriter writetext = new(GlobalVariables.AppName + ".key"))
                {
                    writetext.WriteLine(GeneratedKey);
                    writetext.WriteLine(ActivationKey);
                }
                Global.ActivationMessage = Global.AppLang.Activated;
                Global.ProductActivated = GetVisibility.Hidden.ToString();
                IsActivationEnabled = false;
                Global.NewNotificationMessage(MessageBackground.MediumSeaGreen, Global.AppLang.Done);
                return;
            }
            Global.NewNotificationMessage(MessageBackground.IndianRed, Global.AppLang.ActivationFailed);
        }

        public ActivationViewModel()
        {
            string? keyFile = Directory.GetFiles(GlobalVariables.AppLocation, "*.key").FirstOrDefault();
            if (!string.IsNullOrEmpty(keyFile))
            {
                string? line = File.ReadLines(keyFile).ElementAtOrDefault(1);
                if (!string.IsNullOrEmpty(line))
                    ActivationKey = line;
                if (ActivationKey == LicenseKeyGenerator.GenerateLicenseKey(GeneratedKey))
                {
                    IsActivationEnabled = false;
                    Global.ProductActivated = GetVisibility.Hidden.ToString();
                    return;
                }
                Global.NewNotificationMessage(MessageBackground.IndianRed, Global.AppLang.ActivationFileCorrupted);
                return;
            }
            Global.NewNotificationMessage(MessageBackground.IndianRed, Global.AppLang.NotActivated);
        }
    }
}
