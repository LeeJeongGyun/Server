namespace DummyClient;

using System.Net;
using System.Net.NetworkInformation;
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
        PlayerInfoReq pInfo = new PlayerInfoReq() { playerId = 1001, name = "jklee" };

        ArraySegment<byte>? sendBuf = pInfo.Serialize();

        if (sendBuf.HasValue)
            Send(sendBuf.Value);
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

    public abstract class Header
    {
        public ushort _size;
        public ushort packetId;

        public abstract void Deserialize(ArraySegment<byte> buffer);

        public abstract ArraySegment<byte>? Serialize();
    }

    public class PlayerInfoReq : Header
    {
        public string name;
        public int playerId;

        public PlayerInfoReq()
        {
            packetId = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Deserialize(ArraySegment<byte> buffer)
        {
            ushort count = 0;
            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(buffer.Array, buffer.Offset, buffer.Count);
            //ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += sizeof(ushort);
            //ushort packetId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += sizeof(ushort);

            this.playerId = BitConverter.ToInt32(s.Slice(count));
            count += sizeof(int);

            ushort nameLen = BitConverter.ToUInt16(s.Slice(count));
            count += sizeof(ushort);

            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
            count += nameLen;
        }

        public override ArraySegment<byte>? Serialize()
        {
            ArraySegment<byte> seg = SendBufferHelper.Open(1024);

            ushort count = 0;
            bool success = true;
            Span<byte> sp = new Span<byte>(seg.Array, seg.Offset, seg.Count);
            count = sizeof(ushort);
            success &= BitConverter.TryWriteBytes(sp.Slice(count), this.packetId);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(sp.Slice(count), this.playerId);
            count += sizeof(int);

            // string 내용
            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name.AsSpan(), sp.Slice(count + sizeof(ushort)));
            // string 길이
            success &= BitConverter.TryWriteBytes(sp.Slice(count), nameLen);
            count += sizeof(ushort);
            count += nameLen;

            // 패킷 크기
            success &= BitConverter.TryWriteBytes(sp, count);

            if (success == false)
                return null;
            else
                return SendBufferHelper.Close(count);
        }
    }
}
