namespace ServerCore;

using System.Net;
using System.Net.Sockets;
using System.Text;

internal class Program
{
    private static Listener _listener = new Listener();
    private static int number = 0;

    private static async Task Main(string[] args)
    {
        IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());

        IPAddress ipAdr = ipHostEntry.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAdr, 7777);
        Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        _listener.Init(endPoint, OnAcceptHandler);

        while (true)
        {
        }
    }

    private static void OnAcceptHandler(Socket clientSocket)
    {
        try
        {
            Session session = new Session();
            session.Start(clientSocket);

            byte[] buf = Encoding.UTF8.GetBytes("Hello Server");
            session.Send(buf);

            Thread.Sleep(1000);
            session.Disconnect();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw;
        }
    }
}
