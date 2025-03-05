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
                int playerId = BitConverter.ToInt32(packet.Array, count);
                count += 4;
                Console.WriteLine($"[SERVER] PlayerId: {playerId}");

                PlayerInfoRes playerInfoRes = new PlayerInfoRes();
                playerInfoRes._size = 12;
                playerInfoRes.packetId = (ushort)PacketID.PlayerInfoRes;
                playerInfoRes.attack = 10;
                playerInfoRes.hp = 100;

                ArraySegment<byte> buffer = SendBufferHelper.Open(1024);
                byte[] size = BitConverter.GetBytes(playerInfoRes._size);
                byte[] pId = BitConverter.GetBytes(playerInfoRes.packetId);
                byte[] attack = BitConverter.GetBytes(playerInfoRes.attack);
                byte[] hp = BitConverter.GetBytes(playerInfoRes.hp);
                Array.Copy(size, 0, buffer.Array, buffer.Offset, size.Length);
                Array.Copy(pId, 0, buffer.Array, buffer.Offset + size.Length, pId.Length);
                Array.Copy(attack, 0, buffer.Array, buffer.Offset + size.Length + pId.Length, attack.Length);
                Array.Copy(hp, 0, buffer.Array, buffer.Offset + size.Length + pId.Length + attack.Length, hp.Length);
                ArraySegment<byte> sendBuf = SendBufferHelper.Close(size.Length + pId.Length + attack.Length + hp.Length);
                Send(sendBuf);
            }
            break;
        }
    }

    /// <inheritdoc/>
    public override void OnSend(int byteOfTransferred)
    {
        Console.WriteLine("[SERVER] Send Completed");
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
