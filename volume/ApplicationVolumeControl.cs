using CoreAudio;
using System.Diagnostics;

namespace VolumeMaster.volume
{
    internal class ApplicationVolumeControl : VolumeControl
    {

        private string applicationName;
        private string executableName;
        private string? customDisplayName;

        private AudioSessionControl2 session = null;

        public ApplicationVolumeControl(string applicationName = "", string executableName = "", string? customDisplayName = null)
        {
            this.applicationName = applicationName;
            this.executableName = executableName;
            this.customDisplayName = customDisplayName;

            if (String.IsNullOrEmpty(applicationName) && String.IsNullOrEmpty(executableName))
            {
                throw new ArgumentException("We need at least something here.");
            }

            DiscoverIfNecessary();
        }


        private bool DiscoverIfNecessary()
        {

            if (session != null && session.State == AudioSessionState.AudioSessionStateActive) return true;
            bool hadOldSession = session != null;
            session?.Dispose();
            session = null;

            MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator(Guid.NewGuid());
            foreach (MMDevice device in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                foreach (var session in device.AudioSessionManager2.Sessions)
                {
                    if (session.State == AudioSessionState.AudioSessionStateActive)
                    {
                        Process p = Process.GetProcessById((int)session.ProcessID);

                        //check for executable name
                        if (!String.IsNullOrEmpty(executableName) && !String.IsNullOrEmpty(p.ProcessName))
                        {
                            string executable = executableName.ToLower();
                            string process = p.ProcessName.ToLower();
                            if (!process.Contains(executable))
                            {
                                // skip if no match
                                continue;
                            }
                        }

                        if (!String.IsNullOrEmpty(applicationName) &&
                            (!String.IsNullOrEmpty(session.DisplayName) || !String.IsNullOrEmpty(p.MainWindowTitle)))
                        {
                            string application = applicationName.ToLower();
                            string? sessionName = session.DisplayName?.ToLower();
                            string? windowName = p.MainWindowTitle?.ToLower();

                            if (sessionName == null && windowName == null)
                            {
                                continue;
                            }

                            if ((sessionName != null && !sessionName.Contains(application)) &&
                                (windowName != null && !windowName.Contains(application)))
                            {
                                continue;
                            }

                            this.session = session;
                            this.session.OnSimpleVolumeChanged += (s, v, m) =>
                            {
                                notifyVolumeChanged();
                                notifyMuteChanged();
                            };
                        }
                    }
                }
            }
            if (hadOldSession || session != null)
            {
                notifyChanged();
            }

            return session != null;
        }



        public override string Name
        {
            get
            {
                DiscoverIfNecessary();
                return customDisplayName ?? session?.DisplayName ?? "N/A";
            }
        }

        public override int Volume
        {
            get
            {

                float volume = (session?.SimpleAudioVolume?.MasterVolume ?? 0.0f);
                return (int)Math.Round(volume * 100);
            }
            set
            {

                if (DiscoverIfNecessary())
                {
                    float old = session.SimpleAudioVolume.MasterVolume * 100.0f;
                    float target = Math.Max(Math.Min((float)value, 100.0f), 0.0f);
                    if (old != target)
                    {
                        session.SimpleAudioVolume.MasterVolume = target / 100.0f;
                    }
                    if (session.SimpleAudioVolume.MasterVolume != old)
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
                return session?.SimpleAudioVolume?.Mute ?? true;
            }
            set
            {
                if (DiscoverIfNecessary())
                {
                    bool old = session.SimpleAudioVolume.Mute;
                    if (old != value)
                    {
                        session.SimpleAudioVolume.Mute = value;
                    }
                    if (session.SimpleAudioVolume.Mute != old)
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
