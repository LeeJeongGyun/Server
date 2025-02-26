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

        try
        {
            _listener.Init(endPoint);

            while (true)
            {
                Console.WriteLine("Litening...");

                Socket clientSocket = await _listener.AcceptAsync();

                byte[] readBuf = new byte[1024];
                int recvBytes = clientSocket.Receive(readBuf);
                string recvData = Encoding.UTF8.GetString(readBuf, 0, recvBytes);
                Console.WriteLine($"Recv Data: {recvData}");

                byte[] buf = Encoding.UTF8.GetBytes("Hello Server");
                clientSocket.Send(buf);

                clientSocket.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw;
        }
    }
}
