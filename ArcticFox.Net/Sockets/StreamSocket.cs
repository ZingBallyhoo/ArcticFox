using System;
using System.IO;
using System.Threading.Tasks;

namespace ArcticFox.Net.Sockets;

public class StreamSocket : SocketInterface
{
    private readonly Stream m_stream;

    public StreamSocket(Stream stream)
    {
        m_stream = stream;
    }

    public override ValueTask SendBuffer(ReadOnlyMemory<byte> data)
    {
        return m_stream.WriteAsync(data);
    }

    public override ValueTask<int> ReceiveBuffer(Memory<byte> buffer)
    {
        return m_stream.ReadAsync(buffer, m_cancellationTokenSource.Token);
    }

    protected override ValueTask CloseSocket()
    {
        m_stream.Close();
        return ValueTask.CompletedTask;
    }
}