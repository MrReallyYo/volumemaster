﻿using System.ComponentModel;

namespace VolumeMaster.volume
{
    public class VolumeControl : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        private VolumeProvider volumeProvider;
        private string searchName;
        private string executableName;
        private string? customDisplayName;

        private AVolume? _volume;

        public VolumeControl(VolumeProvider volumeProvider, string searchName = "", string? customDisplayName = null)
        {
            if (String.IsNullOrEmpty(searchName) && String.IsNullOrEmpty(executableName))
            {
                throw new ArgumentException("We need at least something here.");
            }

            this.volumeProvider = volumeProvider;
            this.searchName = searchName;
            this.customDisplayName = customDisplayName;

            volumeProvider.VolumeAdded += VolumeProvider_VolumeAdded;
            volumeProvider.VolumeUpdated += VolumeProvider_VolumeUpdated;
            volumeProvider.VolumeRemoved += VolumeProvider_VolumeRemoved;

            foreach (AVolume volume in volumeProvider.Volumes)
            {
                VolumeProvider_VolumeAdded(volumeProvider, volume);
            }

        }

        private void VolumeProvider_VolumeRemoved(object? sender, AVolume volume)
        {
            if (this.volume?.Identifier == volume.Identifier)
            {
                this.volume = null;
            }
        }

        private void VolumeProvider_VolumeUpdated(object? sender, AVolume volume)
        {
            if (this.volume?.Identifier == volume.Identifier)
            {
                this.volume = volume;
            }
        }

        private void VolumeProvider_VolumeAdded(object? sender, AVolume volume)
        {
            if (this.volume != null) return;
            if (volume.Name.ToLower().Contains(searchName) || volume.Name2.ToLower().Contains(searchName))
            {
                this.volume = volume;
            }
        }

        private AVolume? volume
        {
            get
            {
                return _volume;
            }
            set
            {
                if (_volume != null)
                {
                    _volume.PropertyChanged -= volume_PropertyChanged;
                }
                _volume = value;
                if (_volume != null)
                {
                    _volume.PropertyChanged -= volume_PropertyChanged;
                    _volume.PropertyChanged += volume_PropertyChanged;
                }
            }
        }

        private void volume_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(AVolume.Volume):
                    notifyVolumeChanged();
                    break;

                case nameof(AVolume.IsMuted):
                    notifyIsMutedChanged();
                    break;

                case nameof(AVolume.IsActive):
                    notifyIsActiveChanged();
                    break;
                default: break;
            }

        }


        public string Name
        {
            get
            {
                if (volume == null) return "";
                if (!String.IsNullOrEmpty(customDisplayName)) return customDisplayName;
                if (!String.IsNullOrEmpty(volume.Name)) return volume.Name;
                if (!String.IsNullOrEmpty(volume.Name2)) return volume.Name2;
                return "-";
            }
        }

        public int Volume
        {
            get
            {
                double vol = (volume?.Volume ?? 0.0);
                return (int)Math.Round(vol);
            }
            set
            {
                if (volume != null)
                {
                    double old = volume.Volume;
                    double target = Math.Max(Math.Min((float)value, 100.0), 0.0);
                    if (old != target)
                    {
                        volume.Volume = target;
                    }
                }
            }
        }


        public bool IsMuted
        {
            get
            {
                return volume?.IsMuted ?? false;
            }
            set
            {
                if (volume != null)
                {
                    volume.IsMuted = value;
                }
            }
        }

        public bool IsActive
        {
            get
            {
                return volume?.IsActive ?? false;
            }
        }

        protected void notifyVolumeChanged()
        {
            if (IsActive && PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Volume)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(SpeakerIcon)));
            }
        }

        protected void notifyIsMutedChanged()
        {
            if (IsActive && PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsMuted)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(SpeakerIcon)));
            }
        }

        protected void notifyIsActiveChanged()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsActive)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Visibility)));

            }
        }


        static List<String> icons = [
            "SpeakerMute48", "Speaker048", "Speaker148","Speaker248"
            ];
        public string SpeakerIcon
        {
            get
            {
                if (IsMuted) return icons[0];
                return Volume switch
                {
                    >= 66 => icons[3],
                    >= 33 => icons[3],
                    > 0 => icons[2],
                    _ => icons[0],
                };
            }
        }

        public string Visibility
        {
            get
            {
                if (IsActive)
                {
                    return "Visible";
                }
                else
                {
                    return "Collapsed";
                }
            }
        }

        public void VolumeStepUp(int step)
        {
            if (step <= 0) return;
            Volume = (Volume + step) / step * step;
        }

        public void VolumeStepDown(int step)
        {
            if (step <= 0) return;
            Volume = (Volume - step) / step * step;
        }

    }
}
