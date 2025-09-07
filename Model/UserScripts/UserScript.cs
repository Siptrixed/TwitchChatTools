using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchChatTools.Model.UserScripts
{
    internal class UserScript
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;

        public void Launch(UserScriptContext context)
        {

        }
    }
}
