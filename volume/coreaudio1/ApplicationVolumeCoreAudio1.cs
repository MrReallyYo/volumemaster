using CoreAudio;
using System.Diagnostics;

namespace VolumeMaster.volume.coreaudio1
{
    public class ApplicationVolumeCoreAudio1 : AVolume
    {
        private DispatchIfNecessaryDispatcher dispatcher;
        private AudioSessionControl2 session;

        private string _id;
        private string _name;
        private string _name2;
        private float _volume;
        private bool _muted;
        public override bool IsActive => true;

        public ApplicationVolumeCoreAudio1(DispatchIfNecessaryDispatcher dispatcher, AudioSessionControl2 session)
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
                _id = session.SessionIdentifier;
                _name = session.DisplayName;
                _name2 = Process.GetProcessById((int)session.ProcessID).ProcessName;
                _volume = session.SimpleAudioVolume.MasterVolume;
                _muted = session.SimpleAudioVolume.Mute;
                this.session.OnSimpleVolumeChanged -= Session_OnSimpleVolumeChanged;
                this.session.OnSimpleVolumeChanged += Session_OnSimpleVolumeChanged;
            });
        }

        private void Session_OnSimpleVolumeChanged(object sender, float newVolume, bool newMute)
        {
            _volume = newVolume;
            _muted = newMute;
            notifyVolumeChanged();
            notifyIsMutedChanged();
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
                    session.SimpleAudioVolume.MasterVolume = value;
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
                    session.SimpleAudioVolume.Mute = value;
                    notifyIsMutedChanged();
                });
            }
        }
    }
}
