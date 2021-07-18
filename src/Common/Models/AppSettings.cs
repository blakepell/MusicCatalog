/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System.Collections.ObjectModel;
using Configs;
using LocalAppDataFolder;
using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using Argus.Collections;
using MusicCatalog.Common.Models;

namespace MusicCatalog
{
    public class AppSettings : ConfigsTools, INotifyPropertyChanged
    {
        [JsonProperty("musicDirectoryList", Required = Required.Default)]
        public SpecialObservableCollection<IndexDirectory> MusicDirectoryList { get; set; } = new();

        private string _databaseFilename = "MusicCatalog.db";

        [JsonProperty("databaseFilename", Required = Required.AllowNull)]
        public string DatabaseFilename
        {
            get => _databaseFilename;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Set(ref _databaseFilename, "MusicCatalog.db");
                }
                else
                {
                    Set(ref _databaseFilename, value);
                }
            }
        }

        [JsonIgnore]
        public string DatabaseConnectionString => $"Data Source={Path.Combine(this.LocalAppData.AssemblyFolderPath, this.DatabaseFilename)}";

        [JsonIgnore]
        public LocalAppData LocalAppData { get; } = new();

        /// <summary>
        /// Event that is fired when the property is changed via Set as part of INotifyPropertyChanged.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Used to set the value of a property and fire PropertyChanged event of INotifyPropertyChanged.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storage"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        private void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// OnPropertyChanged
        /// </summary>
        /// <param name="propertyName"></param>
        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}