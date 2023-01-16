using Aljaras.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Aljaras.MVVM.ViewModel
{
    internal partial class GuideViewModel : ObservableRecipient
    {
        public GlobalViewModel Global { get; set; } = GlobalViewModel.Instance;

        #region Observable 
        [ObservableProperty]
        private List<string> guideList = new();

        [ObservableProperty]
        private string currentImage = string.Empty;

        [ObservableProperty]
        private int imageIndex = 0;

        [ObservableProperty]
        private string nOGuideImagesVisible = GetVisibility.Visible.ToString();
        #endregion

        #region RelayCommands
        [RelayCommand]
        private void GoNext() 
        {
            ImageIndex++;
            if (ImageIndex > GuideList.Count - 1)
                ImageIndex = 0;
            CurrentImage = GuideList[ImageIndex];
        }

        [RelayCommand]
        private void GoNBack() 
        {
            ImageIndex--;
            if (ImageIndex < 0)
                ImageIndex = GuideList.Count - 1;
            CurrentImage = GuideList[ImageIndex];
        }
        #endregion

        #region Functions
        public GuideViewModel()
        {
            string dir = GlobalVariables.AppLocation + "Guide";
            if (!Directory.Exists(dir)) return;
            DirectoryInfo GuideImagesDirectory = new(dir);
            foreach (FileInfo aFile in GuideImagesDirectory.GetFiles("*.png"))
            {
                GuideList.Add(aFile.FullName);
            }
            if (GuideList.Any())
            {
                CurrentImage = GuideList[ImageIndex];
                NOGuideImagesVisible = GetVisibility.Hidden.ToString();
            }
        }
        #endregion

    }
}
