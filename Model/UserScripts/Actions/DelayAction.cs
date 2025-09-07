using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchChatTools.Model.Utils;
using static System.Net.Mime.MediaTypeNames;

namespace TwitchChatTools.Model.UserScripts.Actions
{
    public class DelayAction : UserScriptAction
    {
        public int Delay { get; set; }
        public DelayAction(int delay)
        {
            Delay = delay;
            ActionType = UserScriptActionType.Delay;
        }
        public override async Task Execute(UserScriptContext context)
        {
            await Task.Delay(Delay);
        }
        public override string? ToString()
        {
            switch (ActionType)
            {
                case UserScriptActionType.Delay: return $"Ждать {MyAppExt.GetTimeFromMilliseconds(Delay)}";
            }
            return base.ToString();
        }
    }
}
