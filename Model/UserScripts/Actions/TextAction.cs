using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchChatTools.Model.Utils;
using static System.Net.Mime.MediaTypeNames;

namespace TwitchChatTools.Model.UserScripts.Actions
{
    public class TextAction : UserScriptAction
    {
        public string Text { get; set; } = string.Empty;
        public TextAction(string text, UserScriptActionType type = UserScriptActionType.SendMessage)
        {
            Text = text;
            ActionType = type;
            if(type != UserScriptActionType.SendMessage && type != UserScriptActionType.ShellComand)
            {
                throw new ArgumentException("Invalid ActionType for this class");
            }
        }
        public override async Task Execute(UserScriptContext context)
        {
            if (!string.IsNullOrEmpty(Text))
            {
                switch (ActionType)
                {
                    case UserScriptActionType.ShellComand:
                        await MyAppExt.RunCMD(ComposeText(context, Text));
                        break;
                    case UserScriptActionType.SendMessage:
                        MainApp.Instance?.Connection?.SendMessage(ComposeText(context, Text));
                        break;
                }

            }
        }
        public override string? ToString()
        {
            switch (ActionType)
            {
                case UserScriptActionType.SendMessage: return $"Написать '{Text}'";
                case UserScriptActionType.ShellComand: return $"Выполнить '{Text}'";
            }
            return base.ToString();
        }
    }
}
