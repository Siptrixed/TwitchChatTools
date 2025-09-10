using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchChatTools.Model.Chat;

namespace TwitchChatTools.Model.Objects
{
    internal class CustomCommand
    {
        public string Command { get; set; } = string.Empty;
        public List<string> Parameters { get; set; } = new List<string>();
        public string Description { get; set; } = string.Empty;
        public string RequiredRight { get; set; } = string.Empty;
        public ChatUserRights RequiredChatUserType { get; set; } = ChatUserRights.Viewer;
        public string ScriptId { get; set; } = string.Empty;
    }
}
