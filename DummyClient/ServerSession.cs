namespace DummyClient;

using System.Net;
using System.Text;
using ServerCore;

internal class ServerSession : PacketSession
{
    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoRes = 2,
    }

    /// <inheritdoc/>
    /// <remarks>
    ///     size 계산은 데이터를 전부 Write한 후에 알 수 있기 때문에 마지막에 Write한다.
    /// </remarks>
    public override void OnConnected(EndPoint endPoint)
    {
        PlayerInfoReq pInfo = new PlayerInfoReq() { packetId = (ushort)PacketID.PlayerInfoReq, playerId = 1001 };

        ArraySegment<byte> seg = SendBufferHelper.Open(1024);

        ushort count = 0;
        bool success = true;
        Span<byte> sp = new Span<byte>(seg.Array, seg.Offset, seg.Count);
        //success &= BitConverter.TryWriteBytes(sp.Slice(count), pInfo._size);
        count += 2;
        success &= BitConverter.TryWriteBytes(sp.Slice(count), pInfo.packetId);
        count += 2;
        success &= BitConverter.TryWriteBytes(sp.Slice(count), pInfo.playerId);
        count += 4;
        success &= BitConverter.TryWriteBytes(sp.Slice(0), count);

        ArraySegment<byte> sendBuf = SendBufferHelper.Close(count);

        if (success)
            Send(sendBuf);
        else
            throw new InvalidOperationException("TryWritesBytes 실패");
    }

    /// <inheritdoc/>
    public override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine("[Client] OnDisconnected");
    }

    /// <inheritdoc/>
    public override void OnRecvPacket(ArraySegment<byte> packet)
    {
        ushort dataSize = BitConverter.ToUInt16(packet.Array, 0);
        ushort packetId = BitConverter.ToUInt16(packet.Array, 2);
        Console.WriteLine($"[CLIENT] PacketSize: {dataSize}, PacketId: {packetId}");

        switch (packetId)
        {
        case (ushort)PacketID.PlayerInfoRes:
            {
                int attack = BitConverter.ToInt32(packet.Array, 4);
                int hp = BitConverter.ToInt32(packet.Array, 8);
                Console.WriteLine($"[CLIENT] ATTACK: {attack}, HP: {hp}");
            }

            break;
        }
    }

    /// <inheritdoc/>
    public override void OnSend(int byteOfTransferred)
    {
        Console.WriteLine($"Transferred Data: {byteOfTransferred}");
    }

    public class Header
    {
        public ushort _size;
        public ushort packetId;
    }

    public class PlayerInfoReq : Header
    {
        public int playerId;
    }

    public class PlayerInfoRes : Header
    {
        public int attack;
        public int hp;
    }
}
