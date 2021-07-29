using Argus.ComponentModel;
using System;

namespace MusicCatalog.Theme
{
    public class PresetManager : Observable
    {
        internal const string DefaultPreset = "Default";

        private string _colorPreset = DefaultPreset;

        private PresetManager()
        {
        }

        public static PresetManager Current { get; } = new PresetManager();

        public string ColorPreset
        {
            get => _colorPreset;
            set
            {
                if (_colorPreset != value)
                {
                    _colorPreset = value;
                    this.OnPropertyChanged(nameof(ColorPreset));
                    ColorPresetChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler ColorPresetChanged;
    }
}
