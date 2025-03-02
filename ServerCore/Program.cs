namespace ServerCore;

using System.Net;
using System.Net.Sockets;
using System.Text;

internal class GameSession : Session
{
    /// <inheritdoc/>
    public override void OnConnected(EndPoint endPoint)
    {
        byte[] buf = Encoding.UTF8.GetBytes("Hello Server");
        Send(buf);

        Thread.Sleep(1000);
        Disconnect();
    }

    /// <inheritdoc/>
    public override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine($"[SERVER] Client Disconnected: {endPoint}");
    }

    /// <inheritdoc/>
    public override void OnRecv(ArraySegment<byte> recvData)
    {
        string data = Encoding.UTF8.GetString(recvData.Array!, recvData.Offset, recvData.Count);
        Console.WriteLine($"Recv Data: {data}");
    }

    /// <inheritdoc/>
    public override void OnSend(int byteOfTransferred)
    {
        Console.WriteLine("[SERVER] Send Completed");
    }
}

internal class Program
{
    private static Listener _listener = new Listener();

    private static void Main(string[] args)
    {
        IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());

        IPAddress ipAdr = ipHostEntry.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAdr, 7777);
        Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        _listener.Init(endPoint, () => { return new GameSession(); });

        while (true)
        {
        }
    }
}
