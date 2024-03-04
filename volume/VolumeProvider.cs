using CoreAudio;
using CoreAudio.Interfaces;
using System.Windows.Threading;

namespace VolumeMaster.volume
{
    public class VolumeProvider
    {

        private DispatchIfNecessaryDispatcher dispatcher;
        private CancellationTokenSource cancel;


        private MMDeviceCollection devices;

        private List<DeviceVolume> deviceVolumes = [];
        private List<ApplicationVolume> applicationVolumes = [];
        public List<AVolume> Volumes
        {
            get
            {
                List<AVolume> currentVolumes = new List<AVolume>(deviceVolumes.Count + applicationVolumes.Count);
                currentVolumes.AddRange(deviceVolumes);
                currentVolumes.AddRange(applicationVolumes);
                return currentVolumes;

            }
        }



        public delegate void VolumeRemovedHandler(object? sender, AVolume volume);
        public event VolumeRemovedHandler? VolumeRemoved;
        public delegate void VolumeAddedHandler(object? sender, AVolume volume);
        public event VolumeAddedHandler? VolumeAdded;
        public delegate void VolumeUpdatedHandler(object? sender, AVolume volume);
        public event VolumeUpdatedHandler? VolumeUpdated;


        public VolumeProvider(Dispatcher dispatcher)
        {
            this.dispatcher = new DispatchIfNecessaryDispatcher(dispatcher);
            this.cancel = new CancellationTokenSource();
            this.dispatcher.Invoke(scan);
        }


        private void scan()
        {
            try
            {
                _scan();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                Timer timer = null;
                timer = new Timer((obj) =>
                {
                    timer.Dispose();
                    this.dispatcher.Invoke(scan);
                }, null, 5000, Timeout.Infinite);
            }
        }


        private void _scan()
        {
            List<DeviceVolume> oldDeviceVolumes = new List<DeviceVolume>(deviceVolumes);
            List<DeviceVolume> updatedDeviceVolumes = new List<DeviceVolume>();
            List<DeviceVolume> newDeviceVolumes = new List<DeviceVolume>();

            List<ApplicationVolume> oldApplicationVolumes = new List<ApplicationVolume>(applicationVolumes);
            List<ApplicationVolume> updatedApplicationVolumes = new List<ApplicationVolume>();
            List<ApplicationVolume> newApplicationVolumes = new List<ApplicationVolume>();

            MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator(Guid.NewGuid());
            MMDeviceCollection _devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            foreach (MMDevice device in _devices)
            {
                device.AudioSessionManager2?.RefreshSessions();
                if (device.AudioSessionManager2 == null) continue;

                device.AudioSessionManager2.OnSessionCreated -= AudioSessionManager2_OnSessionCreated;
                device.AudioSessionManager2.OnSessionCreated += AudioSessionManager2_OnSessionCreated;

                // todo device volume


                DeviceVolume? oldDeviceVolume = oldDeviceVolumes.Find((s) => s.Identifier == device.ID);
                DeviceVolume updatedOrNewDeviceVolume;

                if (oldDeviceVolume != null)
                {
                    oldDeviceVolumes.Remove(oldDeviceVolume);
                    updatedOrNewDeviceVolume = oldDeviceVolume;
                    updatedOrNewDeviceVolume.updateDevice(device);
                }
                else
                {
                    updatedOrNewDeviceVolume = new DeviceVolume(dispatcher, device);
                }
                if (oldDeviceVolume != null)
                {
                    updatedDeviceVolumes.Add(updatedOrNewDeviceVolume);
                }
                else
                {
                    newDeviceVolumes.Add(updatedOrNewDeviceVolume);
                }

                foreach (AudioSessionControl2 session in device.AudioSessionManager2.Sessions)
                {
                    session.OnSessionDisconnected -= Session_OnSessionDisconnected;
                    session.OnStateChanged -= Session_OnStateChanged;
                    session.OnSessionDisconnected += Session_OnSessionDisconnected;
                    session.OnStateChanged += Session_OnStateChanged;
                    // only use if session is active
                    if (session.State != AudioSessionState.AudioSessionStateActive) continue;

                    ApplicationVolume? oldVolume = oldApplicationVolumes.Find((s) => s.Identifier == session.SessionIdentifier);
                    ApplicationVolume updatedOrNewVolume;
                    if (oldVolume != null)
                    {
                        oldApplicationVolumes.Remove(oldVolume);
                        updatedOrNewVolume = oldVolume;
                        updatedOrNewVolume.updateSession(session);
                    }
                    else
                    {
                        updatedOrNewVolume = new ApplicationVolume(dispatcher, session);
                    }
                    if (oldVolume != null)
                    {
                        updatedApplicationVolumes.Add(updatedOrNewVolume);
                    }
                    else
                    {
                        newApplicationVolumes.Add(updatedOrNewVolume);
                    }
                }


                List<DeviceVolume> deviceVolumes = new List<DeviceVolume>();
                deviceVolumes.AddRange(newDeviceVolumes);
                deviceVolumes.AddRange(updatedDeviceVolumes);

                List<ApplicationVolume> applicationVolumes = new List<ApplicationVolume>();
                applicationVolumes.AddRange(newApplicationVolumes);
                applicationVolumes.AddRange(updatedApplicationVolumes);


                this.devices = _devices;
                this.deviceVolumes = deviceVolumes;
                this.applicationVolumes = applicationVolumes;
            }

            if (VolumeRemoved != null)
            {
                foreach (AVolume volume in oldDeviceVolumes)
                {
                    VolumeRemoved(this, volume);
                }
                foreach (AVolume volume in oldApplicationVolumes)
                {
                    VolumeRemoved(this, volume);
                }
            }

            if (VolumeAdded != null)
            {
                foreach (AVolume volume in newDeviceVolumes)
                {
                    VolumeAdded(this, volume);
                }
                foreach (AVolume volume in newApplicationVolumes)
                {
                    VolumeAdded(this, volume);
                }
            }

            if (VolumeUpdated != null)
            {
                foreach (AVolume volume in updatedDeviceVolumes)
                {
                    VolumeUpdated(this, volume);
                }
                foreach (AVolume volume in updatedApplicationVolumes)
                {
                    VolumeUpdated(this, volume);
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void AudioSessionManager2_OnSessionCreated(object sender, IAudioSessionControl2 newSession)
        {
            dispatcher.Invoke(() =>
            {
                if (!(newSession is AudioSessionControl2)) return;
                AudioSessionControl2 session = (AudioSessionControl2)newSession;

                session.OnSessionDisconnected -= Session_OnSessionDisconnected;
                session.OnStateChanged -= Session_OnStateChanged;
                session.OnSessionDisconnected += Session_OnSessionDisconnected;
                session.OnStateChanged += Session_OnStateChanged;


                ApplicationVolume? oldVolume = applicationVolumes.Find((s) => s.Identifier == session.SessionIdentifier);

                if (session.State == AudioSessionState.AudioSessionStateActive)
                {
                    if (oldVolume != null)
                    {
                        oldVolume.updateSession(session);
                        if (VolumeUpdated != null)
                        {
                            VolumeUpdated(this, oldVolume);
                        }
                    }
                    else
                    {
                        ApplicationVolume volume = new ApplicationVolume(dispatcher, session);
                        applicationVolumes.Add(volume);
                        if (VolumeAdded != null)
                        {
                            VolumeAdded(this, volume);
                        }
                    }
                }
                else if (oldVolume != null)
                {
                    applicationVolumes.Remove(oldVolume);
                    if (VolumeRemoved != null)
                    {
                        VolumeRemoved(this, oldVolume);
                    }
                }
            });
        }

        private void Session_OnStateChanged(object sender, AudioSessionState newState)
        {
            dispatcher.Invoke(() =>
            {
                if (!(sender is AudioSessionControl2)) return;
                AudioSessionControl2 session = (AudioSessionControl2)sender;
                ApplicationVolume? oldVolume = applicationVolumes.Find((s) => s.Identifier == session.SessionIdentifier);

                if (session.State == AudioSessionState.AudioSessionStateActive)
                {
                    if (oldVolume != null)
                    {
                        oldVolume.updateSession(session);
                        if (VolumeUpdated != null)
                        {
                            VolumeUpdated(this, oldVolume);
                        }
                    }
                    else
                    {
                        ApplicationVolume volume = new ApplicationVolume(dispatcher, session);
                        applicationVolumes.Add(volume);
                        if (VolumeAdded != null)
                        {
                            VolumeAdded(this, volume);
                        }
                    }
                }
                else if (oldVolume != null)
                {
                    applicationVolumes.Remove(oldVolume);
                    if (VolumeRemoved != null)
                    {
                        VolumeRemoved(this, oldVolume);
                    }
                }
            });
        }

        private void Session_OnSessionDisconnected(object sender, AudioSessionDisconnectReason disconnectReason)
        {
            dispatcher.Invoke(() =>
            {
                if (!(sender is AudioSessionControl2)) return;
                AudioSessionControl2 session = (AudioSessionControl2)sender;
                ApplicationVolume? oldVolume = applicationVolumes.Find((s) => s.Identifier == session.SessionIdentifier);
                if (oldVolume != null)
                {
                    applicationVolumes.Remove(oldVolume);
                    if (VolumeRemoved != null)
                    {
                        VolumeRemoved(this, oldVolume);
                    }
                }
            });
        }
    }
}
