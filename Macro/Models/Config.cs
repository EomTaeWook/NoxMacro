﻿using Macro.Infrastructure;
using System.ComponentModel;
using Utils;
using Utils.Document;

namespace Macro.Models
{
    public interface IConfig
    {
        Language Language { get; }
        string SavePath { get; }
        int Period { get; }
        int ItemDelay { get; }
        int Similarity { get; }
        bool SearchImageResultDisplay { get; }
        int DragDelay { get; set; }
        bool VersionCheck { get; }
        InitialTab InitialTab { get; set; }
    }

    public class Config : IConfig, INotifyPropertyChanged
    {
        private Language _language = Language.Kor;
        private string _savePath = "";
        private int _period = ConstHelper.MinPeriod;
        private int _ItemDelay = ConstHelper.MinItemDelay;
        private int _similarity = ConstHelper.DefaultSimilarity;
        private bool _searchImageResultDisplay = true;
        private int _dragDelay = ConstHelper.MinItemDelay;
        private bool _versionCheck = true;
        private InitialTab _initialTab = Infrastructure.InitialTab.Common;

        public Language Language
        {
            get => _language;
            set
            {
                _language = value;
                OnPropertyChanged("Language");
            }
        }

        public string SavePath
        {
            get => _savePath;
            set
            {
                _savePath = value;
                OnPropertyChanged("SavePath");
            }
        }
        public int Period
        {
            get => _period;
            set
            {
                _period = value;
                OnPropertyChanged("Period");
            }
        }
        public int ItemDelay
        {
            get => _ItemDelay;
            set
            {
                _ItemDelay = value;
                OnPropertyChanged("ItemDelay");
            }
        }
        public int Similarity
        {
            get => _similarity;
            set
            {
                _similarity = value;
                OnPropertyChanged("Similarity");
            }
        }
        public bool SearchImageResultDisplay
        {
            get => _searchImageResultDisplay;
            set
            {
                _searchImageResultDisplay = value;
                OnPropertyChanged("SearchImageResultDisplay");
            }
        }
        public bool VersionCheck
        {
            get => _versionCheck;
            set
            {
                _versionCheck = value;
                OnPropertyChanged("VersionCheck");
            }
        }
        public int DragDelay
        {
            get => _dragDelay;
            set
            {
                _dragDelay = value;
                OnPropertyChanged("DragDelay");
            }
        }
        public InitialTab InitialTab
        {
            get => _initialTab;
            set
            {
                _initialTab = value;
                OnPropertyChanged("InitialTab");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
