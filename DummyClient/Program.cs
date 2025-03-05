namespace DummyClient;

using System.Net;
using System.Text;
using ServerCore;

internal class Program
{
    private static void Main(string[] args)
    {
        IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());

        IPAddress ipAdr = ipHostEntry.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAdr, 7777);

        Connector connector = new Connector();
        connector.Connect(endPoint, () => { return new ServerSession(); });

        while (true)
        {
            Thread.Sleep(2000);
        }
    }
}
