using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchChatTools.Twitch;
using TwitchLib.Client.Events;
using TwitchLib.Communication.Events;

namespace TwitchChatTools.Model.Processing
{
    internal class ChatHandler
    {
        private static TwitchConnection? _connection => MainApp.Instance.Connection;
        internal void OnMessage(object? Sender, OnMessageReceivedArgs e)
        {
            _connection?.SendMessage($"Hello {e.ChatMessage.Username}");
        }
    }
}
