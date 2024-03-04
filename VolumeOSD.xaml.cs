using System.Collections.ObjectModel;
using System.ComponentModel;
using Test.src;
using VolumeMaster.volume;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace VolumeMaster
{
    /// <summary>
    /// Interaktionslogik für VolumeOSD.xaml
    /// </summary>
    public partial class VolumeOSD : FluentWindow
    {

        private List<VolumeControl> items;
        ObservableCollection<VolumeControl> itemSource = new ObservableCollection<VolumeControl>();
        public VolumeOSD(List<VolumeControl> items)
        {
            SystemThemeWatcher.Watch(this);
            InitializeComponent();
            this.items = new List<VolumeControl>(items);
            VolumeList.ItemsSource = itemSource;
            foreach (var item in items)
            {
                item.PropertyChanged += VolumeChanged;
            }

        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            popuplate();
            InvalidateArrange();
            InvalidateMeasure();
            UpdateLayout();
        }

        private void VolumeChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(VolumeControl.Volume) && e.PropertyName != nameof(VolumeControl.IsMuted)) return;
            Dispatcher.Invoke(popUp);
        }

        private bool popuplate()
        {
            IEnumerable<VolumeControl> activeItems = items.Where(item => item.IsActive);
            itemSource.Clear();
            foreach (VolumeControl activeItem in activeItems)
            {
                itemSource.Add(activeItem);
            }
            return activeItems.Count() != 0;
        }

        Timer timer = null;
        private void popUp()
        {
            if (!popuplate())
            {
                Win11OSD.restore();
                return;
            }
            Win11OSD.hide();

            timer?.Dispose();

            Left = 10;
            Top = 10;
            Show();



            timer = new Timer((obj) =>
             {
                 timer.Dispose();
                 Dispatcher.Invoke(new Action(() =>
                 {
                     Hide();
                 }));
             }, null, 1000, Timeout.Infinite);




        }

    }
}
