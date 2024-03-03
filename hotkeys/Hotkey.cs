using System.Windows.Input;

namespace VolumeMaster.hotkeys
{
    record Hotkey
    {


        private static int hotkeyId = 7331;
        private static int NextHotkeyId()
        {
            return hotkeyId++;
        }


        public int HotkeyId = NextHotkeyId();
        public required List<ModifierKeys> Modifier { get; init; }
        public required int Key { get; init; }

        public required Action<Hotkey> Handler { get; init; }

    }
}
