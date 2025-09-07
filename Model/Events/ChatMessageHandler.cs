using System.Text;
using TwitchChatTools.Model.Objects;
using TwitchChatTools.Model.Twitch;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.EventSub.Core.EventArgs.Channel;

namespace TwitchChatTools.Model.Events
{
    internal class ChatMessageHandler
    {
        private static TwitchConnection? _connection => MainApp.Instance?.Connection;

        public event EventHandler<ParsedCommand>? OnCustomCommand;
        internal void OnMessage(object? Sender, OnMessageReceivedArgs e)
        {
            var parsed = ParsedCommand.TryParse(e.ChatMessage);
            if (parsed != null)
            {
                ProcessCommand(parsed);
            }
            else
            {

            }
        }

        private void ProcessCommand(ParsedCommand command)
        {
            switch (command.Command)
            {
                case "help":
                    var help = new StringBuilder();

                    help.AppendLine(">help - Вывести список доступных команд;\r\n");

                    if (MainApp.Instance != null)
                    {
                        foreach (var helpItem in MainApp.Instance.Commands.Values)
                        {
                            help.AppendLine($">{helpItem.Command} {string.Join(' ',helpItem.Parameters.Select(x=>$"[{x}]"))} - {helpItem.Description};\r\n");
                        }
                    }
                    _connection?.SendMessage(help.ToString());
                    break;
                default:
                    OnCustomCommand?.Invoke(this, command);
                    break;
            }
        }
    }
}
