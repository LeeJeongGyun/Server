namespace Server.Packet;

using System.Net.Sockets;
using ServerCore;

internal class PacketManager
{
    #region Singleton

    public static PacketManager _instance;

    public static PacketManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new PacketManager();

            return _instance;
        }
    }

    #endregion Singleton

    private Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    private Dictionary<ushort, Action<PacketSession, IPacket>> _packetHandler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void MakePacket<T>(PacketSession session, ArraySegment<byte> packet) where T : IPacket, new()
    {
        T pkt = new T();
        pkt.Deserialize(packet);

        if (_packetHandler.TryGetValue(pkt.Protocol, out var packetAction))
            packetAction.Invoke(session, pkt);
    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> packet)
    {
        ushort count = 0;
        ushort dataSize = BitConverter.ToUInt16(packet.Array, count);
        count += 2;
        ushort packetId = BitConverter.ToUInt16(packet.Array, count);
        count += 2;

        if (_onRecv.TryGetValue(packetId, out var recvAction))
            recvAction.Invoke(session, packet);

        //switch (packetId)
        //{
        //case (ushort)PacketID.PlayerInfoReq:
        //    {
        //        PlayerInfoReq playerInfoReq = new PlayerInfoReq();
        //        playerInfoReq.Deserialize(packet);

        //        PacketHandler.PlayerInfoReqHandler(this, playerInfoReq);
        //    }

        //    break;
        //}
    }

    public void Register()
    {
        _onRecv.Add((ushort)PacketID.PlayerInfoReq, MakePacket<PlayerInfoReq>);
        _packetHandler.Add((ushort)PacketID.PlayerInfoReq, PacketHandler.PlayerInfoReqHandler);
    }
}
