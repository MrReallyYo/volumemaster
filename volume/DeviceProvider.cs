using CoreAudio;
using System.Windows;

namespace VolumeMaster.volume
{
    class DeviceProvider
    {

        public delegate void ClearSessionsHandler(object? sender);
        public event ClearSessionsHandler? ClearSessions;
        public delegate void RenewSessionsHandler(object? sender);
        public event RenewSessionsHandler? RenewSessions;


        private MMDeviceCollection? _devices = null;
        public List<MMDevice> devices = [];
        private List<AudioSessionManager2> manager = [];
        public List<AudioSessionControl2> sessions = [];


        public DeviceProvider()
        {
            refresh();
        }

        public MMDevice? Device(Role role)
        {
            if (_devices == null) return null;
            string defaultDevice = new MMDeviceEnumerator(Guid.NewGuid()).GetDefaultAudioEndpoint(DataFlow.Render, role).ID;
            foreach (MMDevice device in _devices)
            {
                try
                {
                    if (device.ID == defaultDevice) return device;
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }
            return null;
        }


        private void _refresh()
        {
            if (ClearSessions != null) ClearSessions(this);

            List<MMDevice> devices = new List<MMDevice>();
            List<AudioSessionManager2> manager = new List<AudioSessionManager2>();
            List<AudioSessionControl2> sessions = new List<AudioSessionControl2>();

            MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator(Guid.NewGuid());
            MMDeviceCollection _devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            foreach (MMDevice device in _devices)
            {
                device.AudioSessionManager2?.RefreshSessions();
                if (device.AudioSessionManager2 == null) continue;
                devices.Add(device);
                manager.Add(device.AudioSessionManager2);
                device.AudioSessionManager2.OnSessionCreated += AudioSessionManager2_OnSessionCreated;

                foreach (AudioSessionControl2 session in device.AudioSessionManager2.Sessions)
                {
                    sessions.Add(session);
                    session.OnSessionDisconnected += Session_OnSessionDisconnected;
                    session.OnStateChanged += Session_OnStateChanged;
                }
            }

            this._devices = _devices;
            this.devices = devices;
            this.manager = manager;
            this.sessions = sessions;

            if (RenewSessions != null) RenewSessions(this);
        }


        private CancellationTokenSource cancel;
        void refresh()
        {
            cancel?.Cancel();
            cancel?.Dispose();
            cancel = new CancellationTokenSource();
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    _refresh();
                }));
            }, cancel.Token);

        }

        private void Session_OnStateChanged(object sender, AudioSessionState newState)
        {
            refresh();
        }

        private void Session_OnSessionDisconnected(object sender, AudioSessionDisconnectReason disconnectReason)
        {
            refresh();
        }

        private void AudioSessionManager2_OnSessionCreated(object sender, CoreAudio.Interfaces.IAudioSessionControl2 newSession)
        {
            refresh();
        }
    }
}
