using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace ArcticFox.Net.Sockets
{
    // todo: named this way due to conflict...
    public class WebSocketInterface : CancellingSocket
    {
        private readonly WebSocket m_webSocket;
        private readonly bool m_binaryMessages;
        
        public WebSocketInterface(WebSocket webSocket, bool binaryMessages)
        {
            m_webSocket = webSocket;
            m_binaryMessages = binaryMessages;
        }

        public override ValueTask SendBuffer(ReadOnlyMemory<byte> data)
        {
            // todo: somehow support batching with no end of message flag?
            return m_webSocket.SendAsync(data, m_binaryMessages ? WebSocketMessageType.Binary : WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, m_cancellationTokenSource.Token);
        }

        public override async ValueTask<int> ReceiveBuffer(Memory<byte> buffer)
        {
            var result = await m_webSocket.ReceiveAsync(buffer, m_cancellationTokenSource.Token);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                Close();
                return 0;
            }
            return result.Count;
        }

        protected override async ValueTask CloseSocket()
        {
            await base.CloseSocket();
            await m_webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, default);
        }

        public override void Dispose()
        {
            base.Dispose();
            m_webSocket.Dispose();
        }
    }
}