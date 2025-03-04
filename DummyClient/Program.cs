namespace DummyClient
{
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using ServerCore;

    internal class ClientSession : Session
    {
        /// <inheritdoc/>
        public override void OnConnected(EndPoint endPoint)
        {
            for (int i = 0; i < 5; ++i)
            {
                byte[] sendBuf = Encoding.UTF8.GetBytes($"Hello World {i + 1}");
                Send(sendBuf);
            }
        }

        /// <inheritdoc/>
        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine("[Client] OnDisconnected");
        }

        /// <inheritdoc/>
        public override int OnRecv(ArraySegment<byte> recvData)
        {
            Console.WriteLine($"[CLIENT] RecvData: {Encoding.UTF8.GetString(recvData)}");
            return recvData.Count;
        }

        /// <inheritdoc/>
        public override void OnSend(int byteOfTransferred)
        {
            Console.WriteLine($"Transferred Data: {byteOfTransferred}");
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());

            IPAddress ipAdr = ipHostEntry.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAdr, 7777);

            while (true)
            {
                Connector connector = new Connector();
                connector.Connect(endPoint, () => { return new ClientSession(); });
                Thread.Sleep(2000);
            }
        }
    }
}
