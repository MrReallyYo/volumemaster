using CoreAudio;
using CoreAudio.Interfaces;
using System.Windows.Threading;

namespace VolumeMaster.volume.coreaudio1
{
    public class VolumeProviderCoreAudio1 : VolumeProvider
    {

        private DispatchIfNecessaryDispatcher dispatcher;
        private CancellationTokenSource cancel;


        private MMDeviceCollection devices;

        private List<DeviceVolumCorAudio1e> deviceVolumes = [];
        private List<ApplicationVolumeCoreAudio1> applicationVolumes = [];

        public event VolumeProvider.VolumeRemovedHandler? VolumeRemoved;
        public event VolumeProvider.VolumeAddedHandler? VolumeAdded;
        public event VolumeProvider.VolumeUpdatedHandler? VolumeUpdated;

        public IEnumerable<AVolume> Volumes
        {
            get
            {
                List<AVolume> currentVolumes = new List<AVolume>(deviceVolumes.Count + applicationVolumes.Count);
                currentVolumes.AddRange(deviceVolumes);
                currentVolumes.AddRange(applicationVolumes);
                return currentVolumes;

            }
        }

        public VolumeProviderCoreAudio1(Dispatcher dispatcher)
        {
            this.dispatcher = new DispatchIfNecessaryDispatcher(dispatcher);
            cancel = new CancellationTokenSource();
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
                    dispatcher.Invoke(scan);
                }, null, 5000, Timeout.Infinite);
            }
        }


        private void _scan()
        {
            List<DeviceVolumCorAudio1e> oldDeviceVolumes = new List<DeviceVolumCorAudio1e>(deviceVolumes);
            List<DeviceVolumCorAudio1e> updatedDeviceVolumes = new List<DeviceVolumCorAudio1e>();
            List<DeviceVolumCorAudio1e> newDeviceVolumes = new List<DeviceVolumCorAudio1e>();

            List<ApplicationVolumeCoreAudio1> oldApplicationVolumes = new List<ApplicationVolumeCoreAudio1>(applicationVolumes);
            List<ApplicationVolumeCoreAudio1> updatedApplicationVolumes = new List<ApplicationVolumeCoreAudio1>();
            List<ApplicationVolumeCoreAudio1> newApplicationVolumes = new List<ApplicationVolumeCoreAudio1>();

            MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator(Guid.NewGuid());
            MMDeviceCollection _devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            foreach (MMDevice device in _devices)
            {
                device.AudioSessionManager2?.RefreshSessions();
                if (device.AudioSessionManager2 == null) continue;

                device.AudioSessionManager2.OnSessionCreated -= AudioSessionManager2_OnSessionCreated;
                device.AudioSessionManager2.OnSessionCreated += AudioSessionManager2_OnSessionCreated;

                // todo device volume


                DeviceVolumCorAudio1e? oldDeviceVolume = oldDeviceVolumes.Find((s) => s.Identifier == device.ID);
                DeviceVolumCorAudio1e updatedOrNewDeviceVolume;

                if (oldDeviceVolume != null)
                {
                    oldDeviceVolumes.Remove(oldDeviceVolume);
                    updatedOrNewDeviceVolume = oldDeviceVolume;
                    updatedOrNewDeviceVolume.updateDevice(device);
                }
                else
                {
                    updatedOrNewDeviceVolume = new DeviceVolumCorAudio1e(dispatcher, device);
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

                    ApplicationVolumeCoreAudio1? oldVolume = oldApplicationVolumes.Find((s) => s.Identifier == session.SessionIdentifier);
                    ApplicationVolumeCoreAudio1 updatedOrNewVolume;
                    if (oldVolume != null)
                    {
                        oldApplicationVolumes.Remove(oldVolume);
                        updatedOrNewVolume = oldVolume;
                        updatedOrNewVolume.updateSession(session);
                    }
                    else
                    {
                        updatedOrNewVolume = new ApplicationVolumeCoreAudio1(dispatcher, session);
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


                List<DeviceVolumCorAudio1e> deviceVolumes = new List<DeviceVolumCorAudio1e>();
                deviceVolumes.AddRange(newDeviceVolumes);
                deviceVolumes.AddRange(updatedDeviceVolumes);

                List<ApplicationVolumeCoreAudio1> applicationVolumes = new List<ApplicationVolumeCoreAudio1>();
                applicationVolumes.AddRange(newApplicationVolumes);
                applicationVolumes.AddRange(updatedApplicationVolumes);


                devices = _devices;
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


                ApplicationVolumeCoreAudio1? oldVolume = applicationVolumes.Find((s) => s.Identifier == session.SessionIdentifier);

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
                        ApplicationVolumeCoreAudio1 volume = new ApplicationVolumeCoreAudio1(dispatcher, session);
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
                ApplicationVolumeCoreAudio1? oldVolume = applicationVolumes.Find((s) => s.Identifier == session.SessionIdentifier);

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
                        ApplicationVolumeCoreAudio1 volume = new ApplicationVolumeCoreAudio1(dispatcher, session);
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
                ApplicationVolumeCoreAudio1? oldVolume = applicationVolumes.Find((s) => s.Identifier == session.SessionIdentifier);
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
