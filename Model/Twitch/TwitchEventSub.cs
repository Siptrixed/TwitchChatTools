using Newtonsoft.Json;
using System.Diagnostics;
using TwitchLib.Api.Core.Enums;
using TwitchLib.Api.Helix;
using TwitchLib.EventSub.Core.EventArgs.Channel;
using TwitchLib.EventSub.Websockets;

namespace TwitchChatTools.Model.Twitch
{
    internal class TwitchEventSub
    {
        private static EventSubWebsocketClient _eventsub = new EventSubWebsocketClient();
        private static bool _eventsub_connected = false;

        private TwitchAccount _account;

        public event EventHandler<ChannelPointsCustomRewardRedemptionArgs>? OnRewardRedeemed;
        public event EventHandler<ChannelFollowArgs>? OnNewFollower;
        public event EventHandler<ChannelSubscribeArgs>? OnNewSubscriber;

        public TwitchEventSub(TwitchAccount account)
        {
            _account = account;

            _eventsub.ChannelPointsCustomRewardRedemptionAdd += OnRewardRedemptionAdd;
            _eventsub.ChannelFollow += OnChannelFollow;
            _eventsub.ChannelSubscribe += OnChannelSubscribe;

            _eventsub.WebsocketConnected += OnWebsocketConnected;
            _eventsub.WebsocketDisconnected += OnWebsocketDisconnected;
            _eventsub.ErrorOccurred += OnErrorOccurred;
            if (!_eventsub_connected)
            {
                _eventsub.ConnectAsync().GetAwaiter().GetResult();
                _eventsub_connected = true;
            }
        }

        private Task OnChannelSubscribe(object? sender, ChannelSubscribeArgs e) => RedirectEvent(OnNewSubscriber, sender, e);

        private Task OnRewardRedemptionAdd(object? sender, ChannelPointsCustomRewardRedemptionArgs e) => RedirectEvent(OnRewardRedeemed, sender, e);

        private Task OnChannelFollow(object? sender, ChannelFollowArgs e) => RedirectEvent(OnNewFollower, sender, e);


        private async Task OnWebsocketDisconnected(object? sender, EventArgs e)
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
                var condition = new Dictionary<string, string> { { "broadcaster_user_id", MainApp.Instance?.Settings.ConnectToUserId ?? _account.UserID }, { "moderator_user_id", _account.UserID } };

                List<(string topic, string version)> topics = [
                    ("channel.channel_points_custom_reward_redemption.add", "1"),
                    ("channel.subscribe", "1"),
                    ("channel.follow", "2"),
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
        private Task RedirectEvent<T>(EventHandler<T>? handler, object? sender, T e)
        {
            handler?.Invoke(sender, e);

            return Task.CompletedTask;
        }

    }
}
