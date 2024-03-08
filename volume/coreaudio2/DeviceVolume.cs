using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.Observables;

namespace VolumeMaster.volume.coreaudio2
{
    public class DeviceVolume : AVolume
    {
        private IDevice device;
        private List<IDisposable> obs = [];
        public DeviceVolume(IDevice device)
        {
            this.device = device;

            obs.Add(device.VolumeChanged.Subscribe((x) => { notifyVolumeChanged(); }));
            obs.Add(device.MuteChanged.Subscribe((x) => { notifyIsMutedChanged(); }));
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

        public override float Volume
        {
            get { return (float)device.GetVolumeAsync().Result; }
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
