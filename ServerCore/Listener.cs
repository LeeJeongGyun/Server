namespace ServerCore;

using System.Net;
using System.Net.Sockets;

/// <summary>
/// 비동기 Accept를 담당할 클래스.
/// </summary>
public class Listener
{
    private Socket _listenSocket = null!;
    private Func<Session> _sessionFactory = null!;

    /// <summary>
    /// Listener를 초기화합니다.
    /// </summary>
    /// <param name="endPoint">바인딩할 IP 엔드포인트.</param>
    /// <param name="sessionFactory">Session 생성 팩토리.</param>
    /// <remarks>
    ///     엔진 내부에서는 외부에서 어떤 Session을 생성하는 지 알 지 못하기 때문에 SessionFactory를 받아준다.
    /// </remarks>
    public void Init(IPEndPoint endPoint, Func<Session> sessionFactory)
    {
        _sessionFactory = sessionFactory;
        _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _listenSocket.Bind(endPoint);
        _listenSocket.Listen();

        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.Completed += OnAcceptCompleted;
        RegisterAccept(args);
    }

    /// <summary>
    /// Accpet 비동기 완료 시 호출될 Callback 함수.
    /// </summary>
    /// <param name="sender">해당 함수를 호출한 객체 정보.</param>
    /// <param name="args">연결된 소켓 정보를 가지고 있는 인자.</param>
    private void OnAcceptCompleted(object? sender, SocketAsyncEventArgs args)
    {
        if (args.SocketError == SocketError.Success)
        {
            // 엔진에서는 외부에서 어떤 세션을 생성할 지 모르기 때문에 Factory를 이용하여 생성한다.
            Session session = _sessionFactory.Invoke();
            session.Start(args.AcceptSocket!);
            session.OnConnected(args.AcceptSocket!.RemoteEndPoint!);
        }
        else
        {
            Console.WriteLine(args.SocketError.ToString());
        }

        RegisterAccept(args);
    }

    /// <summary>
    /// Accept 비동기 등록 함수.
    /// </summary>
    /// <param name="args"><see cref="Init"/> 함수에서 세팅된 인자.</param>
    /// <remarks>
    ///     인자로 들어온 args는 재사용되기 때문에 AcceptSocket을 null로 밀어야된다.
    /// </remarks>
    private void RegisterAccept(SocketAsyncEventArgs args)
    {
        args.AcceptSocket = null;

        bool pending = _listenSocket.AcceptAsync(args);
        if (!pending)
            OnAcceptCompleted(null, args);
    }
}
