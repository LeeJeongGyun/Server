namespace PacketGenerator;

/// <summary>
///     패킷 자동 생성의 Template이 되는 클래스.
/// </summary>
internal class PacketFormat
{
    // {0}: Type
    // {1}: Name
    public static string memberFormat =
"""     public {0} {1};""";

    // {0} : 멤버변수
    // {1} : 읽기 포맷
    // {2} : 쓰기 포맷
    public static string packetFormat =
"""
public class PlayerInfoReq
{{
    {0}

    public void Deserialize(ArraySegment<byte> buffer)
    {{
        ushort count = 0;
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(buffer.Array, buffer.Offset, buffer.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);

        {1}
    }}

    public ArraySegment<byte>? Serialize()
    {{
        ArraySegment<byte> seg = SendBufferHelper.Open(1024);

        ushort count = 0;
        bool success = true;
        Span<byte> sp = new Span<byte>(seg.Array, seg.Offset, seg.Count);
        count = sizeof(ushort);
        success &= BitConverter.TryWriteBytes(sp.Slice(count), (ushort)PacketID.PlayerInfoReq);
        count += sizeof(ushort);

        {2}

        // 패킷 크기
        success &= BitConverter.TryWriteBytes(sp, count);

        if (success == false)
            return null;
        else
            return SendBufferHelper.Close(count);
    }}
}}
""";

    // {0} 멤버 변수
    // {1} Type
    public static string ReadFormat =
"""
        this.{0} = BitConverter.ToInt32(s.Slice(count));
        count += sizeof({1});
""";

    // {0} 멤버 변수
    public static string ReadStringFormat =
"""
        ushort {0}Len = BitConverter.ToUInt16(s.Slice(count));
        count += sizeof(ushort);
        this.{0} = Encoding.Unicode.GetString(s.Slice(count, {0}Len));
        count += {0}Len;
""";

    // {0} 멤버 변수
    // {1} Type
    public static string WriteFormat =
"""
        success &= BitConverter.TryWriteBytes(sp.Slice(count), this.{0});
        count += sizeof({1});
""";

    // {0} 멤버 변수
    public static string WriteStringFormat =
"""
        ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}.AsSpan(), sp.Slice(count + sizeof(ushort)));
        success &= BitConverter.TryWriteBytes(sp.Slice(count), {0}Len);
        count += sizeof(ushort);
        count += {0}Len;
""";
}
