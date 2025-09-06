using System.Diagnostics;
using System.Net;
using TwitchChatTools.Model.WebServer;

namespace TwitchChatTools.Model.Twitch
{
    [CustomWebService("Auth")]
    internal static class TwitchOAuth2
    {
        public static readonly List<string> RequaredScopes = new List<string>() { "channel:moderate", "chat:edit", "chat:read", "channel:edit:commercial", "channel:read:redemptions", "channel:manage:redemptions", "channel:read:polls", "channel:manage:polls", "channel:manage:raids", "channel:manage:broadcast", "channel:read:subscriptions", "moderation:read", "moderator:read:chat_settings", "moderator:manage:chat_settings" };
        private static TaskCompletionSource<string>? _currentChallenge = null;

        public static Task<string> PreformAuthChallenge()
        {
            var authUrl = $"https://id.twitch.tv/oauth2/authorize?response_type=token&client_id={TwitchInfo.ClientID}&redirect_uri=http://localhost:8181/Auth/Token&scope={String.Join("+", RequaredScopes)}&state={Guid.NewGuid()}";
            Process.Start(new ProcessStartInfo(authUrl) { UseShellExecute = true });
            _currentChallenge = new TaskCompletionSource<string>();

            return _currentChallenge.Task;
        }

        [CustomWebMethod("Token")]
        public static string ReciveToken(HttpListenerRequest request)
        {
            if (_currentChallenge == null) return PleaseClosePage(request);

            if (string.IsNullOrEmpty(request.Url?.Query))
            {
                return "<script>window.location = window.location.href.replace('#','?');</script>";
            }
            else
            {
                string token = request.QueryString["access_token"] ?? "SCHMOOPIIE";
                _currentChallenge.SetResult(token);
                _currentChallenge = null;
                return "<script>window.location.href = 'http://localhost:8181/Auth/Ready';</script>";
            }
        }

        [CustomWebMethod("Ready")]
        public static string PleaseClosePage(HttpListenerRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return "Закройте пожалуйста окно если оно не закрылось автоматически!<script>window.close();</script>";
        }
    }
}
