using TwitchChatTools.Model.Twitch;
using TwitchLib.Client.Events;

namespace TwitchChatTools.Model.Events
{
    internal class ChatHandler
    {
        private static TwitchConnection? _connection => MainApp.Instance?.Connection;
        internal void OnMessage(object? Sender, OnMessageReceivedArgs e)
        {
            _connection?.SendMessage($"Hello {e.ChatMessage.Username}");
        }
    }
}
