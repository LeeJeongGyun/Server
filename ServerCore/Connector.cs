namespace ServerCore;

using System.Net;
using System.Net.Sockets;

public class Connector
{
    private Func<Session> _sessionFactory = null!;

    /// <summary>
    /// 인자로 들어온 endPoint와 Connect하는 함수
    /// </summary>
    /// <param name="endPoint">연결을 원하는 Peer.</param>
    /// <param name="sessionFactory">Session 생성 팩토리.</param>
    /// <remarks>
    ///     여러 EndPoint와 연결을 할 수 있기 때문에 인자로 소켓을 가지고 있지 않고, Connect 함수에서 소켓을 생성한 후 UserToken으로 전달.
    ///     RemoteEndPoint 세팅이 필요하다.
    /// </remarks>
    public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory)
    {
        _sessionFactory = sessionFactory;
        Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        SocketAsyncEventArgs connectArgs = new SocketAsyncEventArgs();
        connectArgs.Completed += OnCompletedConnect;
        connectArgs.RemoteEndPoint = endPoint;
        connectArgs.UserToken = socket;
        RegisterConnect(connectArgs);
    }

    private void OnCompletedConnect(object? sender, SocketAsyncEventArgs args)
    {
        if (args.SocketError == SocketError.Success)
        {
            Session session = _sessionFactory.Invoke();
            session.Start(args.ConnectSocket);
            session.OnConnected(args.RemoteEndPoint);
        }
        else
        {
            Console.WriteLine("OnCompletedConnect Error");
        }
    }

    private void RegisterConnect(SocketAsyncEventArgs args)
    {
        Socket socket = args.UserToken as Socket;
        if (socket == null)
            return;

        bool pending = socket.ConnectAsync(args);
        if (!pending)
            OnCompletedConnect(null, args);
    }
}
