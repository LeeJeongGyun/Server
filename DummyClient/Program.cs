namespace DummyClient;

using System.Net;
using System.Text;
using ServerCore;

internal class ClientSession : PacketSession
{
    /// <inheritdoc/>
    public override void OnConnected(EndPoint endPoint)
    {
        TestPacket p = new TestPacket();
        p.size = 4;
        p.hp = 100;

        for (int i = 0; i < 5; ++i)
        {
            ArraySegment<byte> buffer = SendBufferHelper.Open(1024);
            byte[] sizeBytes = BitConverter.GetBytes(p.size);
            byte[] hpBytes = BitConverter.GetBytes(p.hp);
            Array.Copy(sizeBytes, 0, buffer.Array, buffer.Offset, sizeBytes.Length);
            Array.Copy(hpBytes, 0, buffer.Array, buffer.Offset + 2, hpBytes.Length);
            ArraySegment<byte> sndBuffer = SendBufferHelper.Close(sizeBytes.Length + hpBytes.Length);
            Send(sndBuffer);
        }
    }

    /// <inheritdoc/>
    public override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine("[Client] OnDisconnected");
    }

    /// <inheritdoc/>
    public override void OnRecvPacket(ArraySegment<byte> packet)
    {
        Console.WriteLine($"[CLIENT] RecvData: {Encoding.UTF8.GetString(packet)}");
    }

    /// <inheritdoc/>
    public override void OnSend(int byteOfTransferred)
    {
        Console.WriteLine($"Transferred Data: {byteOfTransferred}");
    }

    public class TestPacket
    {
        public ushort hp;
        public ushort size;
    }
}

internal class Program
{
    private static void Main(string[] args)
    {
        IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());

        IPAddress ipAdr = ipHostEntry.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAdr, 7777);

        Connector connector = new Connector();
        connector.Connect(endPoint, () => { return new ClientSession(); });

        while (true)
        {
            Thread.Sleep(2000);
        }
    }
}
