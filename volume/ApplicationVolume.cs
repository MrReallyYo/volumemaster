using CoreAudio;

namespace VolumeMaster.volume
{
    public class ApplicationVolume : AVolume
    {
        private DispatchIfNecessaryDispatcher dispatcher;
        private AudioSessionControl2 session;

        private string _id;
        private string _name;
        private float _volume;
        private bool _muted;


        public ApplicationVolume(DispatchIfNecessaryDispatcher dispatcher, AudioSessionControl2 session)
        {
            this.dispatcher = dispatcher;
            updateSession(session);
        }

        public void updateSession(AudioSessionControl2 session)
        {
            dispatcher.Invoke(() =>
            {
                if (this.session != null)
                {
                    this.session.OnSimpleVolumeChanged -= Session_OnSimpleVolumeChanged;
                }

                this.session = session;
                this._id = session.SessionIdentifier;
                this._name = session.DisplayName;
                this._volume = session.SimpleAudioVolume.MasterVolume;
                this._muted = session.SimpleAudioVolume.Mute;
                this.session.OnSimpleVolumeChanged -= Session_OnSimpleVolumeChanged;
                this.session.OnSimpleVolumeChanged += Session_OnSimpleVolumeChanged;
            });
        }

        private void Session_OnSimpleVolumeChanged(object sender, float newVolume, bool newMute)
        {
            if (_volume != newVolume)
            {
                _volume = newVolume;
                notifyVolumeChanged();
            }
            if (_muted != newMute)
            {
                _muted = newMute;
                notifyMuteChanged();
            }
        }

        public override string Identifier
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
                    session.SimpleAudioVolume.MasterVolume = value;
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
                    session.SimpleAudioVolume.Mute = value;
                });
            }
        }
    }
}
