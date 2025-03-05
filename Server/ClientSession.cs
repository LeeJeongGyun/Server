namespace Server;

using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using ServerCore;

internal class ClientSession : PacketSession
{
    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoRes = 2,
    }

    /// <inheritdoc/>
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine("[SERVER] Client Connected");
        Thread.Sleep(3000);
        Disconnect();
    }

    /// <inheritdoc/>
    public override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine($"[SERVER] Client Disconnected: {endPoint}");
    }

    /// <inheritdoc/>
    public override void OnRecvPacket(ArraySegment<byte> packet)
    {
        ushort count = 0;
        ushort dataSize = BitConverter.ToUInt16(packet.Array, count);
        count += 2;
        ushort packetId = BitConverter.ToUInt16(packet.Array, count);
        count += 2;
        Console.WriteLine($"[SERVER] PacketSize: {dataSize}, PacketId: {packetId}");

        switch (packetId)
        {
        case (ushort)PacketID.PlayerInfoReq:
            {
                PlayerInfoReq playerInfoReq = new PlayerInfoReq();
                playerInfoReq.Deserialize(packet);

                Console.WriteLine($"[SERVER] PlayerId: {playerInfoReq.playerId}");
                Console.WriteLine($"[SERVER] Player Name: {playerInfoReq.name}");
            }

            break;
        }
    }

    /// <inheritdoc/>
    public override void OnSend(int byteOfTransferred)
    {
        Console.WriteLine("[SERVER] Send Completed");
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
