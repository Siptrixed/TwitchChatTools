using System.Windows;
using System.Windows.Data;
using TwitchChatTools.Model.Chat;
using TwitchChatTools.Model.Objects;
using TwitchChatTools.Model.Twitch;
using TwitchChatTools.Model.UserScripts;
using TwitchChatTools.Model.Utils;
using TwitchLib.Client.Models;

namespace TwitchChatTools.Model
{
    internal class MainApp
    {
        public static MainApp? Instance { get; private set; }

        internal AppSettings Settings { get; }
        internal TwitchAccount Account { get; set; }
        internal TwitchConnection? Connection { get; set; }
        internal UserScriptRunEventHandler EventHandler { get; set; } = new UserScriptRunEventHandler();
        internal ChatMessageHandler MessageHandler { get; set; } = new ChatMessageHandler();
        internal Dictionary<string, RewardInfo> Rewards { get; set; } = [];
        internal Dictionary<string, UserScript> Scripts { get; set; } = [];
        internal Dictionary<string, CustomCommand> Commands { get; set; } = [];
        internal Dictionary<string, TwitchChatUserInfo> Viewers { get; set; } = [];

        private MainApp()
        {
            if (Instance != null) throw new InvalidOperationException("Instance of class TwitchChatToolsApp alreay exists");

            Settings = ObjectFileSystem.LoadObject<AppSettings>(false, nameof(Settings));
            Account = ObjectFileSystem.LoadObject<TwitchAccount>(true);
            Scripts = ObjectFileSystem.LoadObject<Dictionary<string, UserScript>>(true, nameof(Scripts));
            Commands = ObjectFileSystem.LoadObject<Dictionary<string, CustomCommand>>(true, nameof(Commands));
            Viewers = ObjectFileSystem.LoadObject<Dictionary<string, TwitchChatUserInfo>>(true, nameof(Viewers));

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

            await GetRewards();

            Connection.OnMessageReceived += MessageHandler.OnMessage;
            Connection.OnRewardRedeemed += EventHandler.OnRewardRedeemed;
            Connection.OnNewFollower += EventHandler.OnNewFollower;
            Connection.OnNewSubscriber += EventHandler.OnNewSubscriber;
            MessageHandler.OnCustomCommand += EventHandler.OnCustomCommand;
        }

        public async Task GetRewards()
        {
            if (Connection == null) return;

            var rewards = await Connection.GetCustomRewards();
            var rewardsFromFile = ObjectFileSystem.LoadObject<Dictionary<string, RewardInfo>>(false, nameof(Rewards));
            Rewards = rewards.Select(x =>
            {
                if (rewardsFromFile.ContainsKey(x.Id))
                {
                    return rewardsFromFile[x.Id].UpdateValues(x);
                }
                else
                {
                    return new RewardInfo(x);
                }
            }).ToDictionary(x => x.Id);
        }

        public TwitchChatUserInfo GetUserInfo(string nickname, ChatMessage chatData)
        {
            if (!Viewers.ContainsKey(nickname))
            {
                Viewers.Add(nickname, new TwitchChatUserInfo()
                {
                    Nickname = chatData.DisplayName,
                    Right = GetChatUserRights(chatData)
                });
            }

            var found = Viewers[nickname];
            found.Right = GetChatUserRights(chatData);
            return found;
        }

        public void SaveSettings()
        {
            ObjectFileSystem.SaveObject(Account, true);
            ObjectFileSystem.SaveObject(Settings, false, nameof(Settings));
            ObjectFileSystem.SaveObject(Rewards, false, nameof(Rewards));
            ObjectFileSystem.SaveObject(Scripts, false, nameof(Scripts));
            ObjectFileSystem.SaveObject(Commands, false, nameof(Commands));
            ObjectFileSystem.SaveObject(Viewers, false, nameof(Viewers));
        }

        private void OnAppExit(object sender, ExitEventArgs e)
        {
            SaveSettings();
        }

        private ChatUserRights GetChatUserRights(ChatMessage chatData)
        {
            var rights = ChatUserRights.Viewer;

            if (chatData.IsVip) rights |= ChatUserRights.VIP;
            if (chatData.IsModerator) rights |= ChatUserRights.Moderator;
            if (chatData.IsSubscriber) rights |= ChatUserRights.Subscriber;
            if (chatData.IsBroadcaster) rights |= ChatUserRights.Broadcaster;

            return rights;
        }
    }
}
