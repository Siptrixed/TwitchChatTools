using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Models;
using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;
using TwitchLib.PubSub.Models.Responses.Messages.Redemption;

namespace TwitchChatTools.Model.UserScripts
{
    public class UserScriptContext
    {
        public string UserName { get; set; }
        public string? TextInput { get; set; }
        public string EventId { get; set; }
        public object? EventData { get; set; }
        public Dictionary<string,object> CustomValues { get; set; } = new Dictionary<string, object>();
        public CancellationToken CancellationToken => _ctSource.Token;
        public bool IsCancellationRequested => _ctSource.IsCancellationRequested;
        private CancellationTokenSource _ctSource { get; set; } = new CancellationTokenSource();

        public UserScriptContext(ChannelPointsCustomRewardRedemption redeemption)
        {
            EventData = redeemption;
            UserName = redeemption.UserName;
            TextInput = redeemption.UserInput;
            EventId = redeemption.Id;
        }

        public UserScriptContext(ChannelFollow follow)
        {
            EventData = follow;
            UserName = follow.UserName;
            TextInput = string.Empty;
            EventId = follow.UserId;
        }

        public UserScriptContext(ChannelSubscribe subscribe)
        {
            EventData = subscribe;
            UserName = subscribe.UserName;
            TextInput = subscribe.Tier;
            EventId = subscribe.UserId;
        }

        public UserScriptContext(ChatMessage message)
        {
            EventData = message;
            UserName = message.Username;
            TextInput = message.Message;
            EventId = message.Id;
        }

        public void Cancel()
        {
            _ctSource.Cancel();
        }
    }
}
