using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.Observables;

namespace VolumeMaster.volume.coreaudio2
{
    public class ASDeviceVolume : AVolume
    {
        private IDevice device;
        private List<IDisposable> obs = [];
        public ASDeviceVolume(IDevice device)
        {
            this.device = device;

            obs.Add(device.VolumeChanged.Subscribe((_) => { notifyVolumeChanged(); }));
            obs.Add(device.MuteChanged.Subscribe((_) => { notifyIsMutedChanged(); }));
        }

        ~ASDeviceVolume()
        {
            foreach (var obs in this.obs) { obs.Dispose(); }
        }

        public override string Identifier
        {
            get { return device.Id.ToString(); }
        }

        public override string Name
        {
            get { return device.Name; }
        }

        public override string Name2
        {
            get { return device.FullName; }
        }

        public override double Volume
        {
            get { return device.GetVolumeAsync().Result; }
            set { device.SetVolumeAsync(value); }
        }
        public override bool IsMuted
        {
            get { return device.IsMuted; }
            set { device.SetMuteAsync(value); }
        }

        public override bool IsActive => device.State == DeviceState.Active;

    }
}
