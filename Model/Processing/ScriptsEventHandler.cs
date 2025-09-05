using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api.Helix.Models.ChannelPoints;
using TwitchLib.EventSub.Core.EventArgs.Channel;

namespace TwitchChatTools.Model.Processing
{
    internal class ScriptsEventHandler
    {
        internal Dictionary<string, CustomReward> Rewards { get; set; } = new Dictionary<string, CustomReward>();

        public void OnRewardRedeemed(object? sender, ChannelPointsCustomRewardRedemptionArgs e)
        {

        }
    }
}
