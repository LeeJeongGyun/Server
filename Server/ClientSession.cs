namespace Server;

using System;
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

    public class PlayerInfoReq
    {
        public string name;
        public int playerId;
        public List<Skill> skills = new List<Skill>();

        public void Deserialize(ArraySegment<byte> buffer)
        {
            ushort count = 0;
            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(buffer.Array, buffer.Offset, buffer.Count);
            count += sizeof(ushort);
            count += sizeof(ushort);

            this.playerId = BitConverter.ToInt32(s.Slice(count));
            count += sizeof(int);
            ushort nameLen = BitConverter.ToUInt16(s.Slice(count));
            count += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
            count += nameLen;
            ushort skillLen = BitConverter.ToUInt16(s.Slice(count));
            count += sizeof(ushort);

            skills.Clear();
            for (int i = 0; i < skillLen; ++i)
            {
                Skill skill = new Skill();
                skill.Deserialize(s, ref count);
                skills.Add(skill);
            }
        }

        public ArraySegment<byte>? Serialize()
        {
            ArraySegment<byte> seg = SendBufferHelper.Open(1024);

            ushort count = 0;
            bool success = true;
            Span<byte> sp = new Span<byte>(seg.Array, seg.Offset, seg.Count);
            count = sizeof(ushort);
            success &= BitConverter.TryWriteBytes(sp.Slice(count), (ushort)PacketID.PlayerInfoReq);
            count += sizeof(ushort);

            success &= BitConverter.TryWriteBytes(sp.Slice(count), this.playerId);
            count += sizeof(int);
            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name.AsSpan(), sp.Slice(count + sizeof(ushort)));
            success &= BitConverter.TryWriteBytes(sp.Slice(count), nameLen);
            count += sizeof(ushort);
            count += nameLen;
            success &= BitConverter.TryWriteBytes(sp.Slice(count), (ushort)skills.Count);
            count += sizeof(ushort);

            foreach (Skill skill in skills)
            {
                success &= skill.Serialize(sp, ref count);
            }
            // 패킷 크기
            success &= BitConverter.TryWriteBytes(sp, count);

            if (success == false)
                return null;
            else
                return SendBufferHelper.Close(count);
        }

        public struct Skill
        {
            public float duration;
            public int id;
            public short level;

            internal void Deserialize(ReadOnlySpan<byte> s, ref ushort count)
            {
                this.id = BitConverter.ToInt32(s.Slice(count));
                count += sizeof(int);
                this.level = BitConverter.ToInt16(s.Slice(count));
                count += sizeof(short);
                this.duration = BitConverter.ToSingle(s.Slice(count));
                count += sizeof(float);
            }

            internal bool Serialize(Span<byte> sp, ref ushort count)
            {
                bool success = true;
                success &= BitConverter.TryWriteBytes(sp.Slice(count), this.id);
                count += sizeof(int);
                success &= BitConverter.TryWriteBytes(sp.Slice(count), this.level);
                count += sizeof(short);
                success &= BitConverter.TryWriteBytes(sp.Slice(count), this.duration);
                count += sizeof(float);
                return success;
            }
        }
    }
}
