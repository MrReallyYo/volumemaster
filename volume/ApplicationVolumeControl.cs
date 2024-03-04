using CoreAudio;
using System.Diagnostics;

namespace VolumeMaster.volume
{
    internal class ApplicationVolumeControl : VolumeControl
    {
        private DeviceProvider deviceProvider;
        private string applicationName;
        private string executableName;
        private string? customDisplayName;


        private AudioSessionControl2 session = null;


        private float? lastNotifiedVolume = null;
        private bool? lastNotifiedMute = null;

        public ApplicationVolumeControl(DeviceProvider deviceProvider, string applicationName = "", string executableName = "", string? customDisplayName = null)
        {
            this.deviceProvider = deviceProvider;
            this.applicationName = applicationName;
            this.executableName = executableName;
            this.customDisplayName = customDisplayName;

            if (String.IsNullOrEmpty(applicationName) && String.IsNullOrEmpty(executableName))
            {
                throw new ArgumentException("We need at least something here.");
            }

            deviceProvider.ClearSessions += DeviceProvider_ClearSessions;
            deviceProvider.RenewSessions += DeviceProvider_RenewSessions;

            DiscoverIfNecessary();
        }

        private void DeviceProvider_RenewSessions(object? sender)
        {
            DiscoverIfNecessary();
        }

        private void DeviceProvider_ClearSessions(object? sender)
        {
            session = null;
        }

        private bool DiscoverIfNecessary()
        {

            if (session != null && session.State == AudioSessionState.AudioSessionStateActive) return true;
            bool hadOldSession = session != null;
            session = null;

            lastNotifiedVolume = null;
            lastNotifiedMute = null;

            foreach (AudioSessionControl2 session in deviceProvider.sessions)
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

                    if (!String.IsNullOrEmpty(applicationName))
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
                    }

                    this.session = session;
                    this.session.OnSimpleVolumeChanged += Session_OnSimpleVolumeChanged;
                    break;
                }

            }

            if (hadOldSession || session != null)
            {
                notifyChanged();
            }
            return session != null;
        }


        private void Session_OnSimpleVolumeChanged(object sender, float newVolume, bool newMute)
        {
            AudioSessionControl2 session = (AudioSessionControl2)sender;
            if (session.SimpleAudioVolume.MasterVolume != lastNotifiedVolume)
            {
                lastNotifiedVolume = session.SimpleAudioVolume.MasterVolume;
                notifyVolumeChanged();
            }
            if (session.SimpleAudioVolume.Mute != lastNotifiedMute)
            {
                lastNotifiedMute = session.SimpleAudioVolume.Mute;
                notifyMuteChanged();
            }

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
                return session != null;
            }
        }
    }
}
