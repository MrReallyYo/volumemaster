using System.Runtime.InteropServices;

namespace Test.src
{
    internal class Win11OSD
    {

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, ShowWindwValues nCmdShow);

        private enum ShowWindwValues
        {
            Hide = 0, Show = 5
        };


        private static IntPtr TryFindOsd()
        {
            IntPtr outerWindow = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "XamlExplorerHostIslandWindow", "");
            if (outerWindow == IntPtr.Zero) return IntPtr.Zero;
            return FindWindowEx(outerWindow, IntPtr.Zero, "Windows.UI.Composition.DesktopWindowContentBridge", "DesktopWindowXamlSource");
        }


        public static void hide()
        {
            IntPtr osd = TryFindOsd();
            if (osd != IntPtr.Zero)
            {
                ShowWindow(osd, ShowWindwValues.Hide);
            }
        }

        public static void restore()
        {
            IntPtr osd = TryFindOsd();
            if (osd != IntPtr.Zero)
            {
                ShowWindow(osd, ShowWindwValues.Show);
            }
        }

    }
}
