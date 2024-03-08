
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using AudioSwitcher.AudioApi.Observables;
using AudioSwitcher.AudioApi.Session;
using System.Collections.Concurrent;

namespace VolumeMaster.volume.coreaudio2
{
    public class VolumeProviderCoreAudio2 : VolumeProvider

    {
        public IEnumerable<AVolume> Volumes
        {
            get
            {
                return vols.Values;
            }
        }

        public event VolumeProvider.VolumeRemovedHandler? VolumeRemoved;
        public event VolumeProvider.VolumeAddedHandler? VolumeAdded;
        public event VolumeProvider.VolumeUpdatedHandler? VolumeUpdated;


        private CoreAudioController controller;
        private IDisposable deviceChanged;
        private ConcurrentDictionary<string, List<IDisposable>> sessionChanged = new ConcurrentDictionary<string, List<IDisposable>>();
        private ConcurrentDictionary<string, AVolume> vols = new ConcurrentDictionary<string, AVolume>();
        public VolumeProviderCoreAudio2()
        {
            controller = new CoreAudioController();

            IEnumerable<CoreAudioDevice> devices = controller.GetDevices(AudioSwitcher.AudioApi.DeviceType.Playback, AudioSwitcher.AudioApi.DeviceState.Active);
            foreach (CoreAudioDevice device in devices)
            {
                addDevice(device);

                IAudioSessionController sessionCtrl = device.GetCapability<IAudioSessionController>();
                foreach (IAudioSession session in sessionCtrl.All())
                {
                    addSession(session);
                }
            }


            deviceChanged = controller.AudioDeviceChanged.Subscribe((args) =>
            {
                switch (args.ChangedType)
                {
                    case DeviceChangedType.DeviceAdded:
                        addDevice(args.Device);
                        break;
                    case DeviceChangedType.DeviceRemoved:
                        removeDevice(args.Device);
                        break;
                }
            });
        }

        private void addDevice(IDevice device)
        {
            removeDevice(device);
            DeviceVolume addedDevice = new DeviceVolume(device);
            vols[device.Id.ToString()] = addedDevice;
            added(addedDevice);


            IAudioSessionController sessionCtrl = device.GetCapability<IAudioSessionController>();
            foreach (IAudioSession session in sessionCtrl.ActiveSessions())
            {
                addSession(session);
            }
            sessionChanged[device.Id.ToString()] = [sessionCtrl.SessionCreated.Subscribe((x) => {

                addSession((IAudioSession)x);

            }), sessionCtrl.SessionDisconnected.Subscribe(removeSession)];
        }

        private void removeDevice(IDevice device)
        {
            List<IDisposable> disposable;
            if (sessionChanged.Remove(device.Id.ToString(), out disposable))
            {
                foreach (IDisposable obs in disposable) { obs.Dispose(); }
            }

            AVolume existing;
            if (vols.Remove(device.Id.ToString(), out existing))
            {
                removed(existing);
            }
        }

        private void addSession(IAudioSession session)
        {
            removeSession(session.Id);
            SessionVolume addedSession = new SessionVolume(session);
            vols[session.Id] = addedSession;
            added(addedSession);
        }

        private void removeSession(string sessionId)
        {
            AVolume existing;
            if (vols.Remove(sessionId, out existing))
            {
                removed(existing);
            }
        }



        private void added(AVolume volume)
        {
            if (VolumeAdded != null)
            {
                VolumeAdded(this, volume);
            }
        }
        private void removed(AVolume volume)
        {
            if (VolumeRemoved != null)
            {
                VolumeRemoved(this, volume);
            }
        }
        private void updated(AVolume volume)
        {
            if (VolumeUpdated != null)
            {
                VolumeUpdated(this, volume);
            }
        }

    }
}
