using Hardcodet.Wpf.TaskbarNotification;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using Test.src;
using VolumeMaster.hotkeys;
using VolumeMaster.volume;

namespace VolumeMaster
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        HotkeyWindow hotkeyWindow;
        VolumeOSD osd;
        TaskbarIcon tbi = new TaskbarIcon();
        // private 

        protected override void OnStartup(StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            hotkeyWindow = new HotkeyWindow();



            VolumeControl system = new DeviceVolumeControl(DeviceVolumeControl.Device.Multimeda);
            VolumeControl spotify = new ApplicationVolumeControl("spotify");

            osd = new VolumeOSD([system, spotify]);


            tbi.Icon = new Icon(Application.GetResourceStream(new Uri("pack://application:,,,/res/ico/tray.ico")).Stream);
            tbi.ToolTipText = "VolumeMaster v0.001";
            tbi.DoubleClickCommand = new LeftCommand();





            hotkeyWindow.register(new Hotkey
            {
                Modifier = [ModifierKeys.Shift],
                Key = 0x7D,
                Handler = (hotkey) =>
                {
                    spotify.VolumeStepDown(2);
                }
            });


            hotkeyWindow.register(new Hotkey
            {
                Modifier = [ModifierKeys.Shift],
                Key = 0x7E,
                Handler = (hotkey) =>
                {
                    spotify.VolumeStepUp(2);
                }
            });

            hotkeyWindow.register(new Hotkey
            {
                Modifier = [ModifierKeys.Shift, ModifierKeys.Control],
                Key = 0x7D,
                Handler = (hotkey) =>
                {
                    spotify.VolumeStepDown(5);
                }
            });


            hotkeyWindow.register(new Hotkey
            {
                Modifier = [ModifierKeys.Shift, ModifierKeys.Control],
                Key = 0x7E,
                Handler = (hotkey) =>
                {
                    spotify.VolumeStepUp(5);
                }
            });


            this.RegisterCleanup();
        }


        private void RegisterCleanup()
        {
            Thread.GetDomain().UnhandledException += (sender, eventArgs) => Cleanup();
            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => Cleanup();
        }

        private void Cleanup()
        {
            Win11OSD.restore();

        }


        public class LeftCommand : ICommand
        {


            public void Execute(object parameter)
            {
                Application.Current.Shutdown();
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }
        }



    }

}
