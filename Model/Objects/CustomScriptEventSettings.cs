using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchChatTools.Model.Objects
{
    public class CustomScriptEventSettings
    {
        public string OnNewFollowerScript { get; set; } = string.Empty;
        public string OnNewSubscriberScript { get; set; } = string.Empty;
    }
}
