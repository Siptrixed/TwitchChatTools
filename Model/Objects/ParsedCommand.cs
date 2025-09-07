using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Models;

namespace TwitchChatTools.Model.Objects
{
    internal class ParsedCommand
    {
        public ChatMessage? Message { get; set; } = null;
        public string Command { get; set; } = string.Empty;
        public string[] Args { get; set; } = new string[0];

        public string GetArg(int index)
        {
            if(Args.Length <= index) return string.Empty;
            return Args[index];
        }

        public string GetTextAfter(int startFrom)
        {
            return string.Join(" ", Args.Skip(startFrom + 1));
        }

        public static ParsedCommand? TryParse(ChatMessage message)
        {
            var input = message.Message;
            if (input.StartsWith(">"))
            {
                var parsed = new ParsedCommand();
                parsed.Message = message;

                var splited = input.Substring(1).Split(" ");
                parsed.Command = splited.FirstOrDefault(string.Empty).ToLower();
                parsed.Args = splited.Skip(1).ToArray();

                return parsed;
            }
            return null;
        }
    }
}
