using Newtonsoft.Json;
using System.Diagnostics;
using System.Windows;
using TwitchChatTools.Model;
using TwitchChatTools.Utils;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.ChannelPoints;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Interfaces;
using TwitchLib.Communication.Models;
using TwitchLib.EventSub.Core.EventArgs.Channel;

namespace TwitchChatTools.Twitch
{
    internal class TwitchConnection
    {
        private TwitchEventSub _eventsub;
        private TwitchClient _client;
        private TwitchAPI _api;
        private TwitchAccount _account;

        private string _selectedChannel => MainApp.Instance.Settings.ConnectToUsername ?? _account.Login;
        private string _selectedChannelId => MainApp.Instance.Settings.ConnectToUserId ?? _account.UserID;

        public event EventHandler<OnMessageReceivedArgs> OnMessageReceived
        {
            add { _client.OnMessageReceived += value; }
            remove { _client.OnMessageReceived -= value; }
        }
        public event EventHandler<ChannelPointsCustomRewardRedemptionArgs> OnRewardRedeemed
        {
            add { _eventsub.OnRewardRedeemed += value; }
            remove { _eventsub.OnRewardRedeemed -= value; }
        }

        public TwitchConnection(TwitchAccount account)
        {
            _account = account;
            _api = account.api;

            ConnectionCredentials credentials = new ConnectionCredentials(_account.Login, _account.Token);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            _client = new TwitchClient(customClient);
            _client.Initialize(credentials, _selectedChannel);

            _client.Connect();

            _client.OnConnectionError += OnConnectionError;

            _eventsub = new TwitchEventSub(_account);
        }

        private void OnConnectionError(object? sender, OnConnectionErrorArgs e)
        {
            MainApp.Instance.Settings.ConnectToUsername = null;
            MainApp.Instance.Settings.ConnectToUserId = null;
            MainApp.Instance.Account = new TwitchAccount();

            Process.Start(ObjectFileSystem.AppFile);
            Application.Current.Shutdown();
        }

        internal void SendMessage(string text)
        {
            _client.SendMessage(_selectedChannel, text);
        }

        internal async Task<Dictionary<string, CustomReward>> GetCustomRewards()
        {
            var response = await _api.Helix.ChannelPoints.GetCustomRewardAsync(_selectedChannelId);
            return response.Data.ToDictionary(x => x.Id);
        }
    }
}
