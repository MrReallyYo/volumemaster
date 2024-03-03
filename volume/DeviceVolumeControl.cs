using CoreAudio;

namespace VolumeMaster.volume
{
    internal class DeviceVolumeControl : VolumeControl
    {

        public enum Device
        {
            Communication,
            Multimeda
        }


        private Device deviceType;
        private string? customDisplayName;

        private MMDevice? device = null;

        private Role role
        {
            get
            {
                return deviceType switch
                {
                    Device.Communication => Role.Communications,
                    Device.Multimeda => Role.Multimedia,
                    _ => throw new NotImplementedException()
                };
            }
        }

        public DeviceVolumeControl(Device deviceType = Device.Multimeda, string? customDisplayName = null)
        {
            this.deviceType = deviceType;
            this.customDisplayName = customDisplayName;
            DiscoverIfNecessary();
        }

        private bool DiscoverIfNecessary()
        {
            if (device != null && device.State == DeviceState.Active) return true;
            device = null;

            MMDeviceEnumerator DevEnum = new MMDeviceEnumerator(Guid.NewGuid());
            device = DevEnum.GetDefaultAudioEndpoint(DataFlow.Render, role);
            notifyChanged();

            if (device != null && device.AudioEndpointVolume != null)
            {
                device.AudioEndpointVolume.OnVolumeNotification += (d) =>
                {
                    notifyVolumeChanged();
                };
            }

            return device != null;
        }

        private void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get
            {
                DiscoverIfNecessary();
                return customDisplayName ?? device?.DeviceInterfaceFriendlyName ?? device?.DeviceFriendlyName ?? "N/A";
            }
        }

        public override int Volume
        {
            get
            {

                float volume = (device?.AudioEndpointVolume?.MasterVolumeLevelScalar ?? 0.0f);
                return (int)(volume * 100);
            }
            set
            {

                if (DiscoverIfNecessary())
                {
                    float volume = ((float)value) / 100.0f;
                    float old = device.AudioEndpointVolume.MasterVolumeLevelScalar;
                    device.AudioEndpointVolume.MasterVolumeLevelScalar = Math.Max(Math.Min(volume, 1.0f), 0.0f);
                    if (device.AudioEndpointVolume.MasterVolumeLevelScalar != old)
                    {
                        notifyVolumeChanged();
                    }
                }
            }
        }

        override public bool IsMuted
        {
            get
            {
                DiscoverIfNecessary();
                return device?.AudioEndpointVolume?.Mute ?? true;
            }
        }

        override public bool IsActive
        {
            get
            {
                return DiscoverIfNecessary();
            }
        }
    }
}
