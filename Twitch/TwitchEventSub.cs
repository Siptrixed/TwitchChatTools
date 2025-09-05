using Newtonsoft.Json;
using System.Diagnostics;
using TwitchChatTools.Model;
using TwitchLib.Api.Core.Enums;
using TwitchLib.Client.Events;
using TwitchLib.EventSub.Core.EventArgs;
using TwitchLib.EventSub.Core.EventArgs.Channel;
using TwitchLib.EventSub.Websockets;

namespace TwitchChatTools.Twitch
{
    internal class TwitchEventSub
    {
        private static EventSubWebsocketClient _eventsub = new EventSubWebsocketClient();
        private static bool _eventsub_connected = false;

        private TwitchAccount _account;

        public event EventHandler<ChannelPointsCustomRewardRedemptionArgs>? OnRewardRedeemed;

        public TwitchEventSub(TwitchAccount account)
        {
            _account = account;

            _eventsub.WebsocketConnected += OnWebsocketConnected;
            _eventsub.ErrorOccurred += OnErrorOccurred;
            _eventsub.ChannelPointsCustomRewardRedemptionAdd += OnChannelPointsCustomRewardRedemptionAdd;
            if (!_eventsub_connected)
            {
                _eventsub.ConnectAsync().GetAwaiter().GetResult();
                _eventsub_connected = true;
            }
        }

        private async Task OnWebsocketDisconnected(object sender, EventArgs e)
        {
            while (!await _eventsub.ReconnectAsync())
            {
                await Task.Delay(1000);
            }
        }
        private async Task OnWebsocketConnected(object? sender, TwitchLib.EventSub.Websockets.Core.EventArgs.WebsocketConnectedArgs e)
        {
            if (!e.IsRequestedReconnect)
            {
                var condition = new Dictionary<string, string> { { "broadcaster_user_id", MainApp.Instance.Settings.ConnectToUserId ?? _account.UserID }, { "moderator_user_id", _account.UserID } };

                List<(string topic, string version)> topics = [
                    ("channel.channel_points_custom_reward_redemption.add", "1"),
                ];

                foreach (var topic in topics)
                {
                    await _account.api.Helix.EventSub.CreateEventSubSubscriptionAsync(topic.topic, topic.version, condition, EventSubTransportMethod.Websocket, _eventsub.SessionId);
                }
            }
        }

        private Task OnErrorOccurred(object? sender, TwitchLib.EventSub.Websockets.Core.EventArgs.ErrorOccuredArgs e)
        {
            Debug.WriteLine(JsonConvert.SerializeObject(e.Message));

            return Task.CompletedTask;
        }

        private Task OnChannelPointsCustomRewardRedemptionAdd(object? sender, ChannelPointsCustomRewardRedemptionArgs e)
        {
            OnRewardRedeemed?.Invoke(sender, e);

            return Task.CompletedTask;
        }
    }
}
