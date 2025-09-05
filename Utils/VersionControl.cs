namespace TwitchChatTools.Utils
{
    internal static class VersionControl
    {
        public static string? Version => System.Reflection.Assembly.GetExecutingAssembly().GetName()?.Version?.ToString();
    }
}
