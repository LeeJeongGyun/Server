namespace ServerCore;

using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// Client와 1:1 매핑이 되는 클래스.
/// </summary>
internal class Session
{
    private int _disconnected = 1;

    private object _lockObject = new object();
    private SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();
    private SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
    private long _sendFlag = 0;
    private List<ArraySegment<byte>> _sendList = new List<ArraySegment<byte>>();
    private Queue<byte[]> _sendQueue = new Queue<byte[]>();
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

    /// <summary>
    /// 컨텐츠에서 송신을 요청할 때 호출되는 함수.
    /// </summary>
    /// <param name="sendBuf">전송할 데이터를 담은 버퍼.</param>
    public void Send(byte[] sendBuf)
    {
        lock (_lockObject)
            _sendQueue.Enqueue(sendBuf);

        if (Interlocked.Exchange(ref _sendFlag, 1) == 0)
            RegisterSend();
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

        _recvArgs.Completed += OnCompletedRecv;
        _recvArgs.SetBuffer(new byte[1024], 0, 1024);

        _sendArgs.Completed += OnCompletedSend;
        RegisterRecv();
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

                RegisterRecv();
            }
            catch (Exception e)
            {
                Console.WriteLine($"OnCompletedRecv Fail: {e.Message}");
                throw;
            }
        }
        else
        {
            Disconnect();
        }
    }

    /// <summary>
    /// 비동기 Send 완료 시 호출될 Callback 함수.
    /// SendQueue에 데이터가 있다면 내가 다시 보낸다.
    /// </summary>
    /// <param name="sender">해당 함수를 호출한 객체 정보.</param>
    /// <param name="args">연결된 소켓 정보를 가지고 있는 인자.</param>
    private void OnCompletedSend(object? sender, SocketAsyncEventArgs args)
    {
        if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
        {
            try
            {
                _sendList.Clear();

                if (_sendQueue.Count > 0)
                    RegisterSend();
                else
                    _sendFlag = 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
        else
        {
            Disconnect();
        }
    }

    /// <summary>
    /// Recv 등록 함수.
    /// </summary>
    private void RegisterRecv()
    {
        bool pending = _socket.ReceiveAsync(_recvArgs);
        if (!pending)
            OnCompletedRecv(null, _recvArgs);
    }

    /// <summary>
    /// Send 등록 함수.
    /// </summary>
    /// <remarks>
    ///     BufferList 사용 시 Property의 Add 함수를 사용하지 않고, 리스트를 세팅한 후 대입 연산을 이용해야 된다.
    ///     https://stackoverflow.com/questions/11820677/how-use-bufferlist-with-socketasynceventargs-and-not-get-socketerror-invalidargu.
    ///     해당 링크에서 나와있듯, 내부에서 WSABUF[] 배열을 사용할 때 문제가 발생될 수 있다고 한다.
    /// </remarks>
    private void RegisterSend()
    {
        lock (_lockObject)
        {
            // TODO: Queue에 데이터가 너무 많다면 나눠서 보내는 것도 고려해야 된다.
            while (_sendQueue.Count > 0)
            {
                _sendList.Add(new ArraySegment<byte>(_sendQueue.Dequeue()));
            }
        }

        _sendArgs.BufferList = _sendList;

        bool pending = _socket.SendAsync(_sendArgs);
        if (!pending)
            OnCompletedSend(null, _sendArgs);
    }

    #endregion 네트워크 통신
}
