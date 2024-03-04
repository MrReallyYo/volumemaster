using System.ComponentModel;

namespace VolumeMaster.volume
{
    public abstract class AVolume : INotifyPropertyChanged
    {
        public abstract string Identifier { get; }

        public abstract string Name { get; }
        public abstract float Volume { get; set; }
        public abstract bool IsMuted { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void notifyVolumeChanged()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Volume)));
            }
        }

        protected void notifyMuteChanged()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsMuted)));
            }
        }
    }
}
