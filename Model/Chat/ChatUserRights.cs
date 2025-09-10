using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchChatTools.Model.Chat
{
    [Flags]
    public enum ChatUserRights
    {
        Undefined = 0,

        Viewer = 1 << 0,

        VIP = 1 << 1,
        Moderator = 1 << 2,

        Subscriber = 1 << 3,
        Broadcaster = 1 << 4,
    }
}
