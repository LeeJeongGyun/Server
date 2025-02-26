namespace ServerCore;

using System.Net;
using System.Net.Sockets;

internal class Listener
{
    private Socket _listenSocket;

    public void Init(IPEndPoint endPoint)
    {
        this._listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        this._listenSocket.Bind(endPoint);
        this._listenSocket.Listen();
    }

    public async Task<Socket> AcceptAsync()
    {
        return await this._listenSocket.AcceptAsync();
    }
}
