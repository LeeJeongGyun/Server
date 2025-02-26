namespace DummyClient
{
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    internal class Program
    {
        private static void Main(string[] args)
        {
            IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());

            IPAddress ipAdr = ipHostEntry.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAdr, 7777);

            while (true)
            {
                try
                {
                    Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(endPoint);
                    Console.WriteLine($"Connected To {socket.RemoteEndPoint.ToString()}");

                    byte[] sendBuf = Encoding.UTF8.GetBytes("You Server?");
                    socket.Send(sendBuf);

                    byte[] recvBuf = new byte[1024];
                    socket.Receive(recvBuf);
                    Console.WriteLine(Encoding.UTF8.GetString(recvBuf));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }

                Thread.Sleep(100);
            }
        }
    }
}
