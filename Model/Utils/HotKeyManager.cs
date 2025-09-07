using System.Windows.Input;
using TwitchChatTools.Model.Objects;
using TwitchChatTools.Model.WinApi;

namespace TwitchChatTools.Model.Utils
{
    internal static class HotKeyManager
    {
        private static AppSettings Setts => MainApp.Instance?.Settings ?? new AppSettings();

        private static WinHotkey? StopSound;
        public static void ClearStopSound() => UnsetHotkey(Setts.StopSoundHotkey, StopSound);
        public static void SetStopSound(Key key, KeyModifier mod) => StopSound = SetHotkey(key, mod, StopSoundPressed, Setts.StopSoundHotkey, StopSound);
        public static void StopSoundPressed(WinHotkey key)
        {

        }

        private static WinHotkey SetHotkey(Key key, KeyModifier mod, Action<WinHotkey> act, WinHotkeyData data, WinHotkey? prev = null)
        {
            UnsetHotkey(data, prev);
            data.Update(key, mod);
            return new WinHotkey(key, mod, act);
        }
        private static void UnsetHotkey(WinHotkeyData data, WinHotkey? prev)
        {
            if (prev != null)
            {
                prev.Unregister();
                prev.Dispose();
            }
            data.Clear();
        }
    }
}
