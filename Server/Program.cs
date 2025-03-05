namespace Server;

using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerCore;

internal class Program
{
    private static Listener _listener = new Listener();

    private static void Main(string[] args)
    {
        IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());

        IPAddress ipAdr = ipHostEntry.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAdr, 7777);
        Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        _listener.Init(endPoint, () => { return new ClientSession(); });

        while (true)
        {
        }
    }
}
