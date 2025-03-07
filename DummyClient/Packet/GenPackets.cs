using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

public enum PacketID
{
    PlayerInfoReq = 0,
}

public class PlayerInfoReq
{
    public string name;
    public int playerId;
    public List<Skill> skills = new List<Skill>();
    public byte testByte;

    public void Deserialize(ArraySegment<byte> buffer)
    {
        ushort count = 0;
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(buffer.Array, buffer.Offset, buffer.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);

        this.testByte = (byte)buffer[buffer.Offset + count];
        count += sizeof(byte);
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

        seg[seg.Offset + count] = this.testByte;
        count += sizeof(byte);
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
