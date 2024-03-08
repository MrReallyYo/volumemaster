using AudioSwitcher.AudioApi.Observables;
using AudioSwitcher.AudioApi.Session;
using System.Diagnostics;

namespace VolumeMaster.volume.coreaudio2
{
    public class SessionVolume : AVolume
    {
        private IAudioSession session;
        private List<IDisposable> obs = [];
        public SessionVolume(IAudioSession session)
        {
            this.session = session;


            obs.Add(session.VolumeChanged.Subscribe((x) => { notifyVolumeChanged(); }));
            obs.Add(session.MuteChanged.Subscribe((x) => { notifyIsMutedChanged(); }));
        }

        public override string Identifier
        {
            get { return session.Id.ToString(); }
        }

        public override string Name
        {
            get { return session.DisplayName; }
        }

        public override string Name2
        {
            get { return Process.GetProcessById(session.ProcessId).ProcessName; }
        }

        public override float Volume
        {
            get { return (float)session.GetVolumeAsync().Result; }
            set { session.SetVolumeAsync(value); }
        }
        public override bool IsMuted
        {
            get { return session.IsMuted; }
            set { session.SetMuteAsync(value); }
        }

        public override bool IsActive => session.SessionState == AudioSessionState.Active;

    }
}
