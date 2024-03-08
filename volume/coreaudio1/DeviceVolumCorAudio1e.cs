using CoreAudio;

namespace VolumeMaster.volume.coreaudio1
{
    public class DeviceVolumCorAudio1e : AVolume
    {

        private DispatchIfNecessaryDispatcher dispatcher;
        private MMDevice device;

        private string _id;
        private string _name;
        private string _name2;
        private float _volume;
        private bool _muted;
        public override bool IsActive => true;
        public DeviceVolumCorAudio1e(DispatchIfNecessaryDispatcher dispatcher, MMDevice device)
        {
            this.dispatcher = dispatcher;
            updateDevice(device);
        }

        public void updateDevice(MMDevice device)
        {
            dispatcher.Invoke(() =>
            {
                if (this.device != null)
                {
                    this.device.AudioEndpointVolume.OnVolumeNotification -= AudioEndpointVolume_OnVolumeNotification;
                }

                this.device = device;
                _name = device.DeviceInterfaceFriendlyName ?? "";
                _name2 = device.DeviceFriendlyName ?? "";
                _volume = device.AudioEndpointVolume.MasterVolumeLevelScalar;
                _muted = device.AudioEndpointVolume.Mute;
                this.device.AudioEndpointVolume.OnVolumeNotification -= AudioEndpointVolume_OnVolumeNotification;
                this.device.AudioEndpointVolume.OnVolumeNotification += AudioEndpointVolume_OnVolumeNotification;
            });
        }

        private void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            _volume = data.MasterVolume;
            _muted = data.Muted;
            notifyVolumeChanged();
            notifyIsMutedChanged();
        }

        override public string Identifier
        {
            get
            {
                return _id;
            }
        }

        override public string Name
        {
            get
            {
                return _name;
            }
        }

        override public string Name2
        {
            get
            {
                return _name2;
            }
        }

        public override float Volume
        {
            get
            {
                return _volume;
            }

            set
            {
                _volume = value;
                dispatcher.Invoke(() =>
                {
                    device.AudioEndpointVolume.MasterVolumeLevelScalar = value;
                    notifyVolumeChanged();
                });
            }
        }

        public override bool IsMuted
        {
            get
            {
                return _muted;
            }
            set
            {
                _muted = value;
                dispatcher.Invoke(() =>
                {
                    device.AudioEndpointVolume.Mute = value;
                    notifyIsMutedChanged();
                });
            }
        }
    }
}
