using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Models;
using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;

namespace TwitchChatTools.Model.UserScripts
{
    internal struct UserScriptContext
    {
        public string UserName {  get; set; }
        public string? TextInput { get; set; }

        public object? EventData { get; set; }

        public UserScriptContext(ChannelPointsCustomRewardRedemption redeemption)
        {
            EventData = redeemption;
            UserName = redeemption.UserName;
            TextInput = redeemption.UserInput;
        }

        public UserScriptContext(ChannelFollow follow)
        {
            EventData = follow;
            UserName = follow.UserName;
            TextInput = string.Empty;
        }

        public UserScriptContext(ChannelSubscribe follow)
        {
            EventData = follow;
            UserName = follow.UserName;
            TextInput = follow.Tier;
        }

        public UserScriptContext(ChatMessage message)
        {
            EventData = message;
            UserName = message.Username;
            TextInput = message.Message;
        }
    }
}
