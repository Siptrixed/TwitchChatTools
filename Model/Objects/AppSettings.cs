using System.Globalization;
using TwitchChatTools.Model.Utils;
using TwitchChatTools.Model.WinApi;

namespace TwitchChatTools.Model.Objects
{
    public class AppSettings
    {
        public bool MinimizeToTray { get; set; }

        public WinHotkeyData StopSoundHotkey { get; set; } = new WinHotkeyData();

        public string? ConnectToUsername { get; set; } = null;
        public string? ConnectToUserId { get; set; } = null;
        public int SelectedLanguage { get; set; } = 0;

        public CustomScriptEventSettings Events { get; set; } = new CustomScriptEventSettings();

        public void ApplyLanguage()
        {
            if (SelectedLanguage != 0)
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(SelectedLanguage);
                Lang.Instance.SetCulture(Thread.CurrentThread.CurrentUICulture);
            }
        }
    }
}
