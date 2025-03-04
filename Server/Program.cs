namespace Server;

using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerCore;

internal class GameSession : PacketSession
{
    /// <inheritdoc/>
    public override void OnConnected(EndPoint endPoint)
    {
        Thread.Sleep(3000);
        Disconnect();
    }

    /// <inheritdoc/>
    public override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine($"[SERVER] Client Disconnected: {endPoint}");
    }

    /// <inheritdoc/>
    public override void OnRecvPacket(ArraySegment<byte> packet)
    {
        int dataSize = BitConverter.ToUInt16(packet.Array, 0);
        int hp = BitConverter.ToUInt16(packet.Array, 2);

        Console.WriteLine($"DataSize: {dataSize}, Hp: {hp}");
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
