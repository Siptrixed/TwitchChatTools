using System.Net.WebSockets;

namespace TwitchChatTools.Model.WebServer
{
    internal class CustomWebSocket
    {
        private WebSocket _socket;
        public CustomWebSocket(WebSocket socket)
        {
            _socket = socket;
        }

        public async Task CloseAsync(WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure)
        {
            await _socket.CloseAsync(status, status.ToString(), CancellationToken.None);
        }

    }
}
