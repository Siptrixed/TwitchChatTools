using System.Globalization;
using System.Windows.Threading;
using TwitchChatTools.Twitch;
using TwitchChatTools.Utils;
using TwitchChatTools.WinApi;

namespace TwitchChatTools.Model
{
    public class AppSettings
    {
        public bool MinimizeToTray { get; set; }

        public WinHotkeyData StopSoundHotkey { get; set; } = new WinHotkeyData();

        public string? ConnectToUsername { get; set; } = null;
        public string? ConnectToUserId { get; set; } = null;
        public int SelectedLanguage { get; set; } = 0;

        //public Dictionary<string, MyRewardInfo> CustomRewards { get; set; } = new Dictionary<string, MyRewardInfo>();

        // public OBSConnectionInfo? OBSWebSockCI { get; set; }
        // public TrueTTSVoices DefaultVoice { get; set; } = TrueTTSVoices.alena;
        //  public byte DefaultVolume { get; set; } = 50;
        //public string YandexToken { get; set; }
        // public int DefaultRate { get; set; } = 0; 

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
