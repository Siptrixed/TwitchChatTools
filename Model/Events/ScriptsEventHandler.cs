using TwitchLib.Api.Helix.Models.ChannelPoints;
using TwitchLib.EventSub.Core.EventArgs.Channel;

namespace TwitchChatTools.Model.Events
{
    internal class ScriptsEventHandler
    {
        internal Dictionary<string, CustomReward> Rewards { get; set; } = new Dictionary<string, CustomReward>();

        public void OnRewardRedeemed(object? sender, ChannelPointsCustomRewardRedemptionArgs e)
        {

        }
    }
}
