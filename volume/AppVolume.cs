using CoreAudio;
using System.ComponentModel;
using System.Diagnostics;

namespace Test.src
{
    internal class AppVolume : INotifyPropertyChanged
    {
        private string appName;
        private AudioSessionControl2 session = null;


        public event PropertyChangedEventHandler? PropertyChanged;

        public AppVolume(string appName)
        {
            this.appName = appName;
            DiscoverSessionIfNecessary();
        }

        private bool DiscoverSessionIfNecessary()
        {
            // we got valid session, exit
            if (session != null && session.State == AudioSessionState.AudioSessionStateActive) return true;
            session = null;
            // try find
            MMDeviceEnumerator DevEnum = new MMDeviceEnumerator(Guid.NewGuid());
            MMDevice device = DevEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            foreach (var session in device.AudioSessionManager2.Sessions)
            {
                if (session.State == AudioSessionState.AudioSessionStateActive)
                {
                    Process p = Process.GetProcessById((int)session.ProcessID);
                    if (string.Equals(session.DisplayName, appName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(p.ProcessName, appName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(p.MainWindowTitle, appName, StringComparison.OrdinalIgnoreCase))
                    {
                        this.session = session;
                        this.session.OnSimpleVolumeChanged += (s, v, m) =>
                        {
                            if (PropertyChanged != null)
                            {
                                PropertyChanged(this, new PropertyChangedEventArgs("Volume"));
                            }
                        };

                        break;
                    }
                }
            }

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Name"));
                PropertyChanged(this, new PropertyChangedEventArgs("Volume"));
            }

            return session != null;
        }

        public string Name
        {
            get
            {
                return session?.DisplayName ?? appName;
            }
        }

        public int Volume
        {
            get
            {
                if (DiscoverSessionIfNecessary())
                {
                    float volume = (session?.SimpleAudioVolume?.MasterVolume ?? 0.0f);
                    return (int)(volume * 100);
                }
                return 0;
            }
            set
            {
                if (DiscoverSessionIfNecessary())
                {
                    session.SimpleAudioVolume.MasterVolume = Math.Max(Math.Min(value / 100.0f, 1.0f), 0.0f);
                }
            }
        }


    }
}
