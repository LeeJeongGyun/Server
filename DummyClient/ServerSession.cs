namespace DummyClient;

using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using ServerCore;
using static DummyClient.ServerSession.PlayerInfoReq;

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
        pInfo.skills.Add(new Skill() { id = 1, level = 10, duration = 10.0f });
        pInfo.skills.Add(new Skill() { id = 2, level = 20, duration = 20.0f });
        pInfo.skills.Add(new Skill() { id = 3, level = 30, duration = 30.0f });
        pInfo.skills.Add(new Skill() { id = 4, level = 40, duration = 40.0f });

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
