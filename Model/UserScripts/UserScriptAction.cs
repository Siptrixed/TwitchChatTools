using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TwitchChatTools.Model.UserScripts
{
    public abstract class UserScriptAction
    {
        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public UserScriptActionType ActionType { get; protected set; }
        public string NextStepId { get; set; } = string.Empty;

        internal void SetEntryId()
        {
            Id = Guid.Empty.ToString();
        }

        protected string ComposeText(UserScriptContext context, string text)
        {
            foreach (Match match in Regex.Matches(text, @"\{([\.\w]*)\}"))
            {
                if (match.Success)
                {
                    var key = match.Groups[1].Value;

                    switch (key)
                    {
                        case "UserName":
                            text = text.Replace($"{{{key}}}", context.UserName);
                            break;
                        case "InputText":
                            text = text.Replace($"{{{key}}}", context.TextInput);
                            break;
                        default:
                            if (context.CustomValues.TryGetValue(key, out object? value))
                            {
                                text = text.Replace($"{{{key}}}", value.ToString());
                            }
                            break;
                    }
                    
                }
            }
            return text;
        }

        public abstract Task Execute(UserScriptContext context);
        public string GetNextStep(UserScriptContext context)
        {
            return NextStepId;
        }
    }
}
