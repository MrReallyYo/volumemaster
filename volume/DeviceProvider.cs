﻿using CoreAudio;

namespace VolumeMaster.volume
{
    class DeviceProvider
    {

        public delegate void ClearSessionsHandler(object? sender);
        public event ClearSessionsHandler? ClearSessions;
        public delegate void RenewSessionsHandler(object? sender);
        public event RenewSessionsHandler? RenewSessions;


        private List<MMDevice> devices;
        private List<AudioSessionManager2> manager;
        public List<AudioSessionControl2> sessions;

        public void refresh()
        {
            if (ClearSessions != null) ClearSessions(this);

            List<MMDevice> devices = new List<MMDevice>();
            List<AudioSessionManager2> manager = new List<AudioSessionManager2>();
            List<AudioSessionControl2> sessions = new List<AudioSessionControl2>();

            MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator(Guid.NewGuid());
            foreach (MMDevice device in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
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

            this.devices = devices;
            this.manager = manager;
            this.sessions = sessions;

            if (RenewSessions != null) RenewSessions(this);
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
