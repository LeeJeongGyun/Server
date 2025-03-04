namespace ServerCore;

public class RecvBuffer
{
    private ArraySegment<byte> _buffer;

    private int _readPos;
    private int _writePos;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RecvBuffer"/> class.
    /// </summary>
    /// <param name="bufferSize">버퍼 크기.</param>
    public RecvBuffer(int bufferSize)
    {
        _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        _readPos = _writePos = 0;
    }

    /// <summary>
    ///     buffer의 데이터 크기.
    /// </summary>
    public int DataSize => _writePos - _readPos;

    /// <summary>
    ///     buffer의 여유 공간 크기.
    /// </summary>
    public int FreeSize => _buffer.Count - _writePos;

    /// <summary>
    ///     Read할 수 있는 Segment.
    /// </summary>
    public ArraySegment<byte> ReadSegment => new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize);

    /// <summary>
    ///     Write할 수 있는 Segment.
    /// </summary>
    public ArraySegment<byte> WriteSegment => new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize);

    /// <summary>
    ///     버퍼 데이터.
    /// </summary>
    /// <param name="numOfBytes">Read한 데이터 크기.</param>
    /// <returns>읽기 실패.</returns>
    public bool OnRead(int numOfBytes)
    {
        if (numOfBytes > DataSize)
            return false;

        _readPos += numOfBytes;
        Clean();
        return true;
    }

    /// <summary>
    ///     버퍼 데이터.
    /// </summary>
    /// <param name="numOfBytes">Write한 데이터 크기.</param>
    /// <returns>쓰기 실패.</returns>
    public bool OnWrite(int numOfBytes)
    {
        if (numOfBytes > FreeSize)
            return false;

        _writePos += numOfBytes;
        return true;
    }

    /// <summary>
    ///     데이터가 없다면 읽기, 쓰기 커서만 변경.
    ///     데이터가 존재한다면 복사 후 커서 변경.
    /// </summary>
    private void Clean()
    {
        if (_readPos == _writePos)
            _readPos = _writePos = 0;
        else
        {
            Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, DataSize);
            _readPos = 0;
            _writePos = DataSize;
        }
    }
}
