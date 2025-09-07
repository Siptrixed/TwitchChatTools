using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api.Helix.Models.ChannelPoints;

namespace TwitchChatTools.Model.Objects
{
    public class RewardInfo
    {
        public string Id { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Prompt { get; set; }
        public int Cost { get; set; }
        public string ScriptId { get; set; } = string.Empty;

        public RewardInfo()
        {
            
        }

        public RewardInfo(CustomReward reward)
        {
            UpdateValues(reward);
        }

        public RewardInfo UpdateValues(CustomReward reward)
        {
            Id = reward.Id;
            Title = reward.Title;
            Prompt = reward.Prompt;
            Cost = reward.Cost;

            return this;
        }
    }
}
