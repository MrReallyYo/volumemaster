using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace VolumeMaster.hotkeys
{
    internal class HotkeyWindow : Window
    {
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(nint hWnd, int id);

        private const int WM_HOTKEY = 0x0312;

        private nint handle;
        private HwndSource source;
        private Dictionary<int, Hotkey> hotkeys = new Dictionary<int, Hotkey>(0);



        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            handle = new WindowInteropHelper(this).Handle;
            source = HwndSource.FromHwnd(handle);
            source.AddHook(HwndHook);
        }

        private nint HwndHook(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_HOTKEY:

                    Hotkey hotkey = hotkeys[wParam.ToInt32()];
                    if (hotkey != null)
                    {
                        handled = true;
                        hotkey.Handler(hotkey);
                    }
                    break;
            }
            return nint.Zero;
        }


        public bool register(Hotkey hotkey)
        {
            new WindowInteropHelper(this).EnsureHandle();
            //RegisterHotKey(handle, 1337, 0x0, 0x14);
            uint mod = (uint)hotkey.Modifier.Sum(mod => ((uint)mod));
            if (RegisterHotKey(handle, hotkey.HotkeyId, mod, hotkey.Key))
            {
                hotkeys[hotkey.HotkeyId] = hotkey;
                return true;
            }
            return false;

        }

        public bool unregister(Hotkey hotkey)
        {
            if (UnregisterHotKey(handle, hotkey.HotkeyId))
            {
                hotkeys.Remove(hotkey.HotkeyId);
                return true;
            }
            return false;
        }

    }
}
