using CoreAudio;

namespace VolumeMaster.volume
{
    public class DeviceVolume : AVolume
    {

        private DispatchIfNecessaryDispatcher dispatcher;
        private MMDevice device;

        private string _id;
        private string _name;
        private float _volume;
        private bool _muted;

        public DeviceVolume(DispatchIfNecessaryDispatcher dispatcher, MMDevice device)
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
                this._name = device.DeviceInterfaceFriendlyName ?? device.DeviceFriendlyName;
                this._volume = device.AudioEndpointVolume.MasterVolumeLevelScalar;
                this._muted = device.AudioEndpointVolume.Mute;
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
