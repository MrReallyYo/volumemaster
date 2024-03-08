namespace VolumeMaster.volume
{
    public interface VolumeProvider
    {

        public IEnumerable<AVolume> Volumes { get; }


        public delegate void VolumeRemovedHandler(object? sender, AVolume volume);
        public event VolumeRemovedHandler? VolumeRemoved;
        public delegate void VolumeAddedHandler(object? sender, AVolume volume);
        public event VolumeAddedHandler? VolumeAdded;
        public delegate void VolumeUpdatedHandler(object? sender, AVolume volume);
        public event VolumeUpdatedHandler? VolumeUpdated;


    }
}
