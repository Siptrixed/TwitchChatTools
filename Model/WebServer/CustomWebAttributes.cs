namespace TwitchChatTools.Model.WebServer
{
    internal abstract class CustomWebAttrubute(string? url = null) : Attribute
    {
        public string? URL { get; set; } = url;
    }
    internal class CustomWebServiceAttribute : CustomWebAttrubute
    {
        public CustomWebServiceAttribute(string? url = null) : base(url)
        {
            
        }
    }
    internal class CustomWebMethodAttribute : CustomWebAttrubute
    {
        public CustomWebMethodAttribute(string? url = null) : base(url)
        {

        }
    }
    internal class CustomWebSocketHandlerAttrubute : CustomWebAttrubute
    {
        public CustomWebSocketHandlerAttrubute(string? url = null) : base(url)
        {

        }
    }
}
