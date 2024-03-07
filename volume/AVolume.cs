using System.ComponentModel;
using VolumeMaster.util;

namespace VolumeMaster.volume
{
    public abstract class AVolume : INotifyPropertyChanged
    {

        private Throttle volumeT = new Throttle();
        private Throttle isMutedT = new Throttle();
        public abstract string Identifier { get; }

        public abstract string Name { get; }
        public abstract float Volume { get; set; }
        public abstract bool IsMuted { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void notifyVolumeChanged()
        {
            volumeT.Dispatch(() =>
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Volume)));
                }
            });
        }

        protected void notifyIsMutedChanged()
        {
            isMutedT.Dispatch(() =>
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsMuted)));
                }
            });
        }
    }
}
