namespace ServerCore;

using System.Net;
using System.Net.Sockets;

/// <summary>
/// 비동기 Accept를 담당할 클래스.
/// </summary>
internal class Listener
{
    private Socket _listenSocket = null!;
    private Action<Socket> _onAcceptHandler = null!;

    /// <summary>
    /// Listener를 초기화합니다.
    /// </summary>
    /// <param name="endPoint">바인딩할 IP 엔드포인트.</param>
    /// <param name="acceptHandler">연결을 처리할 콜백 함수.</param>
    public void Init(IPEndPoint endPoint, Action<Socket> acceptHandler)
    {
        _onAcceptHandler = acceptHandler;
        _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _listenSocket.Bind(endPoint);
        _listenSocket.Listen();

        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.Completed += OnAcceptCompleted;
        RegisterAccept(args);
    }

    /// <summary>
    /// Accept 비동기 등록 함수.
    /// </summary>
    /// <param name="args">AcceptAsync 함수 인자.</param>
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

    /// <summary>
    /// Accpet 비동기 완료 시 호출될 Callback 함수.
    /// </summary>
    /// <param name="sender">해당 함수를 호출한 객체 정보.</param>
    /// <param name="args">연결된 소켓 정보를 가지고 있는 인자.</param>
    private void OnAcceptCompleted(object? sender, SocketAsyncEventArgs args)
    {
        if (args.SocketError == SocketError.Success)
        {
            _onAcceptHandler.Invoke(args.AcceptSocket!);
        }
        else
        {
            Console.WriteLine(args.SocketError.ToString());
        }

        RegisterAccept(args);
    }
}
