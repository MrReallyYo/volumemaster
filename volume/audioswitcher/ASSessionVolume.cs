using AudioSwitcher.AudioApi.Observables;
using AudioSwitcher.AudioApi.Session;
using System.Diagnostics;

namespace VolumeMaster.volume.coreaudio2
{
    public class ASSessionVolume : AVolume
    {
        private IAudioSession session;
        private List<IDisposable> obs = [];
        public ASSessionVolume(IAudioSession session)
        {
            this.session = session;
            obs.Add(session.VolumeChanged.Subscribe((_) => { notifyVolumeChanged(); }));
            obs.Add(session.MuteChanged.Subscribe((_) => { notifyIsMutedChanged(); }));
        }

        ~ASSessionVolume()
        {
            foreach (var obs in this.obs) { obs.Dispose(); }
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

        public override double Volume
        {
            get { return session.GetVolumeAsync().Result; }
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
