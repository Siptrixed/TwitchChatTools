using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchChatTools.Model.Objects
{
    internal class CustomCommand
    {
        public string Command { get; set; } = string.Empty;
        public List<string> Parameters { get; set; } = new List<string>();
        public string Description { get; set; } = string.Empty;
        public string ScriptId { get; set; } = string.Empty;
    }
}
