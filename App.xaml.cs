using Hardcodet.Wpf.TaskbarNotification;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Test.src;
using VolumeMaster.hotkeys;
using VolumeMaster.volume;
using VolumeMaster.volume.coreaudio2;

namespace VolumeMaster
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        VolumeProvider volumeProvider;
        HotkeyWindow hotkeyWindow;
        VolumeOSD osd;
        TaskbarIcon tbi = new TaskbarIcon();
        // private 

        protected override void OnStartup(StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            volumeProvider = new ASVolumeProvider();
            hotkeyWindow = new HotkeyWindow();

            VolumeControl system = new VolumeControl(volumeProvider, "topping");
            VolumeControl spotify = new VolumeControl(volumeProvider, "spotify");
            VolumeControl tidal = new VolumeControl(volumeProvider, "tidalplayer");
            VolumeControl firefox = new VolumeControl(volumeProvider, "firefox");

            osd = new VolumeOSD([system, spotify, tidal, firefox]);


            tbi.Icon = new Icon(GetResourceStream(new Uri("pack://application:,,,/res/ico/tray.ico")).Stream);
            tbi.ToolTipText = "VolumeMaster v0.001";
            tbi.DoubleClickCommand = new LeftCommand();

            hotkeyWindow.register(new Hotkey
            {
                Modifier = [ModifierKeys.None],
                Key = Keys.VolumeMute,
                Handler = (hotkey) =>
                {
                    system.IsMuted = !system.IsMuted;
                }
            });

            hotkeyWindow.register(new Hotkey
            {
                Modifier = [ModifierKeys.None],
                Key = Keys.VolumeDown,
                Handler = (hotkey) =>
                {
                    system.VolumeStepDown(2);
                }
            });


            hotkeyWindow.register(new Hotkey
            {
                Modifier = [ModifierKeys.None],
                Key = Keys.VolumeUp,
                Handler = (hotkey) =>
                {
                    system.VolumeStepUp(2);
                }
            });

            hotkeyWindow.register(new Hotkey
            {
                Modifier = [ModifierKeys.Shift],
                Key = Keys.VolumeDown,
                Handler = (hotkey) =>
                {
                    system.VolumeStepDown(5);
                }
            });


            hotkeyWindow.register(new Hotkey
            {
                Modifier = [ModifierKeys.Shift],
                Key = Keys.VolumeUp,
                Handler = (hotkey) =>
                {
                    system.VolumeStepUp(5);
                }
            });


            hotkeyWindow.register(new Hotkey
            {
                Key = Keys.F16,
                Handler = (hotkey) =>
                {
                    spotify.IsMuted = !spotify.IsMuted;
                    tidal.IsMuted = !tidal.IsMuted;
                }
            });


            hotkeyWindow.register(new Hotkey
            {
                Key = Keys.F14,
                Handler = (hotkey) =>
                {
                    spotify.VolumeStepDown(2);
                    tidal.VolumeStepDown(2);
                }
            });


            hotkeyWindow.register(new Hotkey
            {
                Key = Keys.F15,
                Handler = (hotkey) =>
                {
                    spotify.VolumeStepUp(2);
                    tidal.VolumeStepUp(2);
                }
            });

            hotkeyWindow.register(new Hotkey
            {
                Modifier = [ModifierKeys.Alt],
                Key = Keys.F14,
                Handler = (hotkey) =>
                {
                    spotify.VolumeStepDown(5);
                    tidal.VolumeStepDown(5);
                }
            });


            hotkeyWindow.register(new Hotkey
            {
                Modifier = [ModifierKeys.Alt],
                Key = Keys.F15,
                Handler = (hotkey) =>
                {
                    spotify.VolumeStepUp(5);
                    tidal.VolumeStepUp(5);
                }
            });



            hotkeyWindow.register(new Hotkey
            {
                Modifier = [ModifierKeys.Control],
                Key = Keys.F16,
                Handler = (hotkey) =>
                {
                    firefox.IsMuted = !firefox.IsMuted;
                }
            });

            hotkeyWindow.register(new Hotkey
            {
                Modifier = [ModifierKeys.Control],
                Key = Keys.F14,
                Handler = (hotkey) =>
                {
                    firefox.VolumeStepDown(2);
                }
            });


            hotkeyWindow.register(new Hotkey
            {
                Modifier = [ModifierKeys.Control],
                Key = Keys.F15,
                Handler = (hotkey) =>
                {
                    firefox.VolumeStepUp(2);
                }
            });


            hotkeyWindow.register(new Hotkey
            {
                Modifier = [ModifierKeys.Control, ModifierKeys.Alt],
                Key = Keys.F14,
                Handler = (hotkey) =>
                {
                    firefox.VolumeStepDown(5);
                }
            });


            hotkeyWindow.register(new Hotkey
            {
                Modifier = [ModifierKeys.Control, ModifierKeys.Alt],
                Key = Keys.F15,
                Handler = (hotkey) =>
                {
                    firefox.VolumeStepUp(5);
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
                System.Windows.Application.Current.Shutdown();
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
