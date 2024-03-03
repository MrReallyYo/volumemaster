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
            device?.Dispose();
            device = null;

            MMDeviceEnumerator DevEnum = new MMDeviceEnumerator(Guid.NewGuid());
            device = DevEnum.GetDefaultAudioEndpoint(DataFlow.Render, role);
            notifyChanged();

            if (device != null && device.AudioEndpointVolume != null)
            {
                device.AudioEndpointVolume.OnVolumeNotification += (d) =>
                {
                    notifyVolumeChanged();
                    notifyMuteChanged();
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
                return (int)Math.Round(volume * 100);
            }
            set
            {
                if (DiscoverIfNecessary())
                {
                    float old = device.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
                    float target = Math.Max(Math.Min((float)value, 100.0f), 0.0f);
                    if (old != target)
                    {
                        device.AudioEndpointVolume.MasterVolumeLevelScalar = target / 100.0f;
                    }
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
            set
            {
                if (DiscoverIfNecessary())
                {
                    bool old = device.AudioEndpointVolume.Mute;
                    if (old != value)
                    {
                        device.AudioEndpointVolume.Mute = value;
                    }
                    if (device.AudioEndpointVolume.Mute != old)
                    {
                        notifyMuteChanged();
                    }

                }
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
