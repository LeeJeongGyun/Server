namespace Server;

using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using ServerCore;

internal class ClientSession : PacketSession
{
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

                Console.WriteLine($"[SERVER] testBytes: {playerInfoReq.testByte}");
                Console.WriteLine($"[SERVER] PlayerId: {playerInfoReq.playerId}");
                Console.WriteLine($"[SERVER] Player Name: {playerInfoReq.name}");

                foreach (var skillInfo in playerInfoReq.skills)
                {
                    Console.WriteLine($"Id: {skillInfo.id}, Level: {skillInfo.level}, Duration: {skillInfo.duration}");
                }
            }

            break;
        }
    }

    /// <inheritdoc/>
    public override void OnSend(int byteOfTransferred)
    {
        Console.WriteLine("[SERVER] Send Completed");
    }
}
