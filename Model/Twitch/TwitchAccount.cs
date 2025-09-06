using TwitchLib.Api;

namespace TwitchChatTools.Model.Twitch
{
    [MessagePack.MessagePackObject(keyAsPropertyName: true)]
    public class TwitchAccount
    {
        public string UserID { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;

        [MessagePack.IgnoreMember()]
        internal TwitchAPI api = new TwitchAPI();

        public TwitchAccount()
        {
            api.Settings.ClientId = TwitchInfo.ClientID;
        }
        public TwitchAccount(string token) : this()
        {
            Token = token;
            api.Settings.AccessToken = Token;
        }

        public async Task<bool> Auth()
        {
            if (!await Validate())
            {
                Token = await TwitchOAuth2.PreformAuthChallenge();
                api.Settings.AccessToken = Token;
                return await Validate();
            }
            return true;
        }

        public async Task<bool> Validate()
        {
            if (string.IsNullOrEmpty(Token)) return false;
            api.Settings.AccessToken = Token;

            var validated = await api.Auth.ValidateAccessTokenAsync(Token);

            if (validated == null || validated.ClientId != TwitchInfo.ClientID || validated.ExpiresIn < 86400) return false;

            var scopes = TwitchOAuth2.RequaredScopes.ToHashSet();
            foreach (var scope in validated.Scopes)
            {
                scopes.Remove(scope);
            }
            if (scopes.Count > 0) return false;

            Login = validated.Login;
            UserID = validated.UserId;

            return true;
        }
    }
}
