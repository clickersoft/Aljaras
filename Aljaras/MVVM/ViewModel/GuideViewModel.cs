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
        public GlobalViewModel Global { get; } = GlobalViewModel.Instance;

        [ObservableProperty]
        private List<string> guideList = new();

        [ObservableProperty]
        private string currentImage = "";

        [ObservableProperty]
        private int imageIndex = 0;

        [ObservableProperty]
        private string nOGuideImagesVisible = GetVisibility.Visible.ToString();

        #region RelayCommands
        [RelayCommand]
        private void GoNext() 
        {
            ImageIndex++;
            if (ImageIndex > GuideList.Count()-1)
                ImageIndex = 0;
            CurrentImage = GuideList[ImageIndex];
        }

        [RelayCommand]
        private void GoNBack() 
        {
            ImageIndex--;
            if (ImageIndex < 0)
                ImageIndex = GuideList.Count()-1;
            CurrentImage = GuideList[ImageIndex];
        }
        #endregion

        #region Functions
        public GuideViewModel()
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Guide")) return;
            DirectoryInfo GuideImagesDirectory = new(AppDomain.CurrentDomain.BaseDirectory + "Guide");
            foreach (FileInfo aFile in GuideImagesDirectory.GetFiles("*.png"))
            {
                /*Uri uri = new Uri(aFile.FullName);
                GuideList.Add(new BitmapImage(uri));*/
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
