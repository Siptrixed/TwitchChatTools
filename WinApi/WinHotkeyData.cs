using Newtonsoft.Json;
using System.Windows.Input;

namespace TwitchChatTools.WinApi
{
    public class WinHotkeyData
    {
        public Key Key { get; set; }
        public KeyModifier Mod { get; set; }

        [JsonIgnore]
        public bool IsEmpty => Key == Key.None;

        public WinHotkeyData()
        {

        }
        public WinHotkeyData(Key key, KeyModifier mod)
        {
            Key = key;
            Mod = mod;
        }
        public void Update(Key key, KeyModifier mod)
        {
            Key = key;
            Mod = mod;
        }
        internal void Clear()
        {
            Key = Key.None;
            Mod = KeyModifier.None;
        }
        public override string ToString()
        {
            return IsEmpty ? string.Empty : $"{(Mod != KeyModifier.None ? $"{Mod}+" : "")}{Key}";
        }

    }
}
