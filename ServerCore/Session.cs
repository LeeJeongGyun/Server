namespace ServerCore;

using System.Net.Sockets;
using System.Text;

/// <summary>
/// Client와 1:1 매핑이 되는 클래스.
/// </summary>
internal class Session
{
    private int _disconnected = 1;

    private Socket _socket = null!;

    /// <summary>
    /// 연결을 끊을 때 호출하는 함수.
    /// </summary>
    /// <remarks>
    ///     멀티스레드에서 동시 호출이 될 수 있기 때문에 Interlocked 처리.
    /// </remarks>
    public void Disconnect()
    {
        if (Interlocked.Exchange(ref _disconnected, 1) == 0)
            _socket.Close();
    }

    // 임시 처리
    public void Send(byte[] sendBuf)
    {
        _socket.Send(sendBuf);
    }

    /// <summary>
    /// 세션 생성 후 통신을 시작하기 위한 함수.
    /// </summary>
    /// <param name="socket">Accept로 부터 획득한 소켓.</param>
    /// <remarks>
    ///     멀티스레드에서 동시 호출이 될 수 있기 때문에 Interlocked 처리.
    /// </remarks>
    public void Start(Socket socket)
    {
        _disconnected = 0;
        _socket = socket;

        SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
        recvArgs.Completed += OnCompletedRecv;
        recvArgs.SetBuffer(new byte[1024], 0, 1024);

        RegisterRecv(recvArgs);
    }

    #region 네트워크 통신

    /// <summary>
    /// 비동기 Recv 완료 시 호출될 Callback 함수.
    /// </summary>
    /// <param name="sender">해당 함수를 호출한 객체 정보.</param>
    /// <param name="args">연결된 소켓 정보를 가지고 있는 인자.</param>
    private void OnCompletedRecv(object? sender, SocketAsyncEventArgs args)
    {
        if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
        {
            try
            {
                string recvData = Encoding.UTF8.GetString(args.Buffer!, args.Offset, args.BytesTransferred);
                Console.WriteLine($"Recv Data: {recvData}");

                RegisterRecv(args);
            }
            catch (Exception e)
            {
                Console.WriteLine($"OnCompletedRecv Fail: {e.Message}");
                throw;
            }
        }
        else
        {
            // TODO DISCONNECT
        }
    }

    /// <summary>
    /// Recv 등록 함수.
    /// </summary>
    /// <param name="args"><see cref="Start"/> 함수에서 세팅된 인자.</param>
    private void RegisterRecv(SocketAsyncEventArgs args)
    {
        bool pending = _socket.ReceiveAsync(args);
        if (!pending)
            OnCompletedRecv(null, args);
    }

    #endregion 네트워크 통신
}
