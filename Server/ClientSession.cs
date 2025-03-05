namespace Server;

using System.Net;
using System.Net.NetworkInformation;
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
        public int playerId;

        public PlayerInfoReq()
        {
            packetId = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Deserialize(ArraySegment<byte> buffer)
        {
            ushort count = 0;
            //ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;
            //ushort packetId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            this.playerId = BitConverter.ToInt32(new ReadOnlySpan<byte>(buffer.Array, buffer.Offset + count, buffer.Count - count));
            count += 4;
        }

        public override ArraySegment<byte>? Serialize()
        {
            ArraySegment<byte> seg = SendBufferHelper.Open(1024);

            ushort count = 0;
            bool success = true;
            Span<byte> sp = new Span<byte>(seg.Array, seg.Offset, seg.Count);
            count = 2;
            success &= BitConverter.TryWriteBytes(sp.Slice(count), this.packetId);
            count += 2;
            success &= BitConverter.TryWriteBytes(sp.Slice(count), this.playerId);
            count += 4;
            success &= BitConverter.TryWriteBytes(sp.Slice(0), count);

            if (success == false)
                return null;
            else
                return SendBufferHelper.Close(count);
        }
    }
}
