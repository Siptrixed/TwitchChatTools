using Newtonsoft.Json.Linq;
using TwitchChatTools.Model.Objects;
using TwitchChatTools.Model.Twitch;
using TwitchChatTools.Model.UserScripts;
using TwitchLib.Api.Helix.Models.ChannelPoints;
using TwitchLib.Client.Models;
using TwitchLib.EventSub.Core.EventArgs.Channel;
using TwitchLib.PubSub.Models.Responses.Messages.Redemption;

namespace TwitchChatTools.Model.Events
{
    internal class ScriptsEventsHandler
    {
        private static TwitchConnection? _connection => MainApp.Instance?.Connection;

        public void OnRewardRedeemed(object? sender, ChannelPointsCustomRewardRedemptionArgs e)
        {
            var rewardId = e.Payload.Event.Reward.Id;
            if (MainApp.Instance?.Rewards.TryGetValue(rewardId, out RewardInfo? value) ?? false)
            {
                if (string.IsNullOrEmpty(value.ScriptId)) return;

                var context = new UserScriptContext(e.Payload.Event);
                if(!LaunchScript(value.ScriptId, context))
                {
                    value.ScriptId = string.Empty;
                }
            }
        }

        public void OnCustomCommand(object? sender, ParsedCommand command)
        {
            if (command.Message == null) return;

            if (MainApp.Instance?.Commands.TryGetValue(command.Command, out CustomCommand? value) ?? false)
            {
                if (string.IsNullOrEmpty(value.ScriptId)) return;

                var context = new UserScriptContext(command.Message);
                if (!LaunchScript(value.ScriptId, context))
                {
                    value.ScriptId = string.Empty;
                }
            }
        }

        public void OnNewFollower(object? sender, ChannelFollowArgs e)
        {
            var scriptId = MainApp.Instance?.Settings.Events.OnNewFollowerScript ?? string.Empty;
            if (!string.IsNullOrEmpty(scriptId))
            {
                var context = new UserScriptContext(e.Payload.Event);
                if (!LaunchScript(scriptId, context))
                {
                    MainApp.Instance!.Settings.Events.OnNewFollowerScript = string.Empty;
                }
            }
        }

        public void OnNewSubscriber(object? sender, ChannelSubscribeArgs e)
        {
            var scriptId = MainApp.Instance?.Settings.Events.OnNewSubscriberScript ?? string.Empty;
            if (!string.IsNullOrEmpty(scriptId))
            {
                var context = new UserScriptContext(e.Payload.Event);
                if (!LaunchScript(scriptId, context))
                {
                    MainApp.Instance!.Settings.Events.OnNewSubscriberScript = string.Empty;
                }
            }
        }

        private bool LaunchScript(string scriptId, UserScriptContext context)
        {
            if (string.IsNullOrEmpty(scriptId)) return false;

            if(MainApp.Instance?.Scripts.TryGetValue(scriptId, out UserScript? value) ?? false)
            {
                value.Launch(context);
                return true;
            }
            return false;
        }
    }
}
