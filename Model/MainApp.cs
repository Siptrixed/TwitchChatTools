using System.Windows;
using System.Windows.Data;
using TwitchChatTools.Model.Processing;
using TwitchChatTools.PageTabs;
using TwitchChatTools.Twitch;
using TwitchChatTools.Utils;
using TwitchLib.Api.Helix.Models.ChannelPoints;

namespace TwitchChatTools.Model
{
    internal class MainApp
    {
        public static MainApp Instance { get; private set; }

        internal AppSettings Settings { get; }
        internal TwitchAccount Account { get; set; }
        internal TwitchConnection? Connection { get; set; }
        internal ScriptsEventHandler EventHandler { get; set; } = new ScriptsEventHandler();
        internal ChatHandler MessageHandler { get; set; } = new ChatHandler();
        private MainApp()
        {
            if (Instance != null) throw new InvalidOperationException("Instance of class TwitchChatToolsApp alreay exists");

            Settings = ObjectFileSystem.LoadObject<AppSettings>(false, nameof(Settings));
            Account = ObjectFileSystem.LoadObject<TwitchAccount>(true);

            Application.Current.Exit += OnAppExit;
        }
        public static void InitializeInstance()
        {
            if (Instance != null) throw new InvalidOperationException("Instance of class TwitchChatToolsApp alreay exists");

            Instance = new MainApp();
        }

        public async Task<bool> TryToAuth(bool doOAuthChallenge = false)
        {
            if (doOAuthChallenge)
            {
                return await Account.Auth();
            }
            else
            {
                return await Account.Validate();
            }
        }
        public async Task<bool> GetConnectToUserId()
        {
            var result = await Account.api.Helix.Users.GetUsersAsync(logins: [Settings.ConnectToUsername]);

            foreach (var user in result.Users)
            {
                if (user.Login.Equals(Settings.ConnectToUsername))
                {
                    Settings.ConnectToUserId = user.Id;
                    return true;
                }
            }

            return false;
        }
        public async Task Connect(Action<Binding> progress)
        {
            Connection = new TwitchConnection(Account);

            progress.Invoke(Lang.Bind("FetchingChannelData"));

            var rewards = await Connection.GetCustomRewards();
            EventHandler.Rewards = rewards;

            //updelegate(Lang.Bind("Finishing"));

            Connection.OnMessageReceived += MessageHandler.OnMessage;
            Connection.OnRewardRedeemed += EventHandler.OnRewardRedeemed;
        }

        public void SaveSettings()
        {
            ObjectFileSystem.SaveObject(Settings, false, nameof(Settings));
            ObjectFileSystem.SaveObject(Account, true);
        }

        private void OnAppExit(object sender, ExitEventArgs e)
        {
            SaveSettings();
        }

    }
}
