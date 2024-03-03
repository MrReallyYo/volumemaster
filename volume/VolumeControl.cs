using System.ComponentModel;

namespace VolumeMaster.volume
{
    public abstract class VolumeControl : INotifyPropertyChanged
    {
        public abstract string Name { get; }
        public abstract int Volume { get; set; }

        public abstract bool IsMuted { get; }

        public abstract bool IsActive { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void notifyChanged()
        {
            notifyNameChanged();
            notifyVolumeChanged();
        }

        protected void notifyNameChanged()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Name"));
            }
        }

        protected void notifyVolumeChanged()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Volume"));
                PropertyChanged(this, new PropertyChangedEventArgs("IsMuted"));
                PropertyChanged(this, new PropertyChangedEventArgs("SpeakerIcon"));
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
