namespace ServerCore;

public class SendBufferHelper
{
    /// <summary>
    ///     Lock을 걸지 않기 위하여 ThreadLocal을 사용.
    ///     현재 스레드가 가지고 있는 SendBuffer.
    /// </summary>
    private static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null!; });

    private static int ChunkSize => 4069 * 10;

    /// <summary>
    ///     Open을 통해 예약한 버퍼에 데이터를 복사 후 호출된다.
    /// </summary>
    /// <param name="usedSize">실제 사용한 크기.</param>
    /// <returns>데이터가 저장된 영역.</returns>
    public static ArraySegment<byte> Close(int usedSize)
    {
        return CurrentBuffer.Value!.Close(usedSize);
    }

    /// <summary>
    ///     예약하고 싶은 크기의 버퍼를 리턴.
    /// </summary>
    /// <param name="reserveSize">예약하고 싶은 크기.</param>
    /// <returns> 인자로 들어온 크기의 버퍼를 리턴.</returns>
    public static ArraySegment<byte> Open(int reserveSize)
    {
        if (reserveSize > ChunkSize)
            throw new ArgumentOutOfRangeException("예약하려는 버퍼의 크기가 ChunkSize보다 큽니다.");

        if (CurrentBuffer.Value == null)
            CurrentBuffer.Value = new SendBuffer(ChunkSize);

        if (CurrentBuffer.Value.FreeSize < reserveSize)
            CurrentBuffer.Value = new SendBuffer(ChunkSize);

        return CurrentBuffer.Value.Open(reserveSize);
    }

    private class SendBuffer
    {
        private byte[] _buffer;
        private int _usedSize;

        public SendBuffer(int chunkSize)
        {
            _buffer = new byte[chunkSize];
            _usedSize = 0;
        }

        /// <summary>
        ///     사용 가능한 크기.
        /// </summary>
        public int FreeSize => _buffer.Length - _usedSize;

        /// <summary>
        ///     Open을 통해 예약한 버퍼에 데이터를 복사 후 호출된다.
        /// </summary>
        /// <param name="usedSize">실제 사용한 크기.</param>
        /// <returns>데이터가 저장된 영역.</returns>
        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            _usedSize += usedSize;

            return segment;
        }

        /// <summary>
        ///     SendBufferHelper를 통해서 호출.
        ///     예약하고 싶은 크기의 버퍼를 리턴.
        /// </summary>
        /// <param name="reserveSize">예약하고 싶은 크기.</param>
        /// <returns> 인자로 들어온 크기의 버퍼를 리턴.</returns>
        /// <remarks>
        ///     SendBufferHelper를 통해서 호출되며, 호출 전 FreeSize 프로퍼티와 reserveSize 비교를 확인한 후 호출된다.
        /// </remarks>
        public ArraySegment<byte> Open(int reserveSize)
        {
            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }
    }
}
