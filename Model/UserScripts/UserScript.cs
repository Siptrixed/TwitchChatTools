using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Communication.Interfaces;

namespace TwitchChatTools.Model.UserScripts
{
    internal class UserScript
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public bool IsMultilaunch { get; set; } = false;
        public Dictionary<string, UserScriptAction> Actions { get; set; } = new Dictionary<string, UserScriptAction>();

        [MessagePack.IgnoreMember]
        [Newtonsoft.Json.JsonIgnore]
        public static LinkedList<UserScriptContext> Queue = new LinkedList<UserScriptContext>();

        [MessagePack.IgnoreMember]
        [Newtonsoft.Json.JsonIgnore]
        public static HashSet<string> LaunchedScripts = new HashSet<string>();

        public bool IsEmpty => Actions.Count == 0;

        public void Launch(UserScriptContext context)
        {
            Task.Run(async () =>
            {
                Queue.AddLast(context);
                if (IsMultilaunch)
                {
                    await Execute(context);
                }
                else
                {
                    lock (this)
                    {
                        Execute(context).GetAwaiter().GetResult();
                    }
                }
                Queue.Remove(context);
            });
        }
        private async Task Execute(UserScriptContext context)
        {
            if (context.IsCancellationRequested) return;

            var nextStep = Guid.Empty.ToString();
            while (true)
            {
                if(Actions.TryGetValue(nextStep, out UserScriptAction? action))
                {
                    await action.Execute(context);
                    nextStep = action.GetNextStep(context);
                    
                    if (string.IsNullOrEmpty(nextStep))
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }
}

