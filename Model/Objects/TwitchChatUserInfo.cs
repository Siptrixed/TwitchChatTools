using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchChatTools.Model.Chat;

namespace TwitchChatTools.Model.Objects
{
    internal class TwitchChatUserInfo
    {
        public string Nickname { get; set; } = string.Empty;
        public ChatUserRights Right { get; set; } = ChatUserRights.Viewer;
        public HashSet<string> CustomRights { get; set; } = [];

        DateTime LastMessageDate { get; set; } = DateTime.Now;
    }
}
