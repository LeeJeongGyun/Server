namespace PacketGenerator;

/// <summary>
///     패킷 자동 생성의 Template이 되는 클래스.
/// </summary>
internal class PacketFormat
{
    // {0}: Type
    // {1}: Name
    public static string memberFormat =
"""
public {0} {1};
""";

    // {0} : 패킷 이름 대문자
    // {1} : 패킷 이름 소문자
    // {2}: 멤버 변수
    // {3}: 읽기 포맷
    // {4}: 쓰기 포맷
    public static string memberListFormat =
"""
public List<{0}> {1}s = new List<{0}>();

public struct {0}
{{
    {2}

    internal void Deserialize(ReadOnlySpan<byte> s, ref ushort count)
    {{
        {3}
    }}

    internal bool Serialize(Span<byte> sp, ref ushort count)
    {{
        bool success = true;
        {4}
        return success;
    }}
}}
""";

    // {0} : 패킷 이름
    // {1} : 멤버 변수
    // {2} : 읽기 포맷
    // {3} : 쓰기 포맷
    public static string packetFormat =
"""
public class {0}
{{
    {1}
    public void Deserialize(ArraySegment<byte> buffer)
    {{
        ushort count = 0;
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(buffer.Array, buffer.Offset, buffer.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);

        {2}
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

        {3}
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
    // {1} To~
    // {2} Type
    public static string ReadFormat =
"""
this.{0} = BitConverter.{1}(s.Slice(count));
count += sizeof({2});
""";

    // {0} 대문자 패킷 이름
    // {1} 소문자 패킷 이름
    public static string ReadListFormat =
"""
ushort {1}Len = BitConverter.ToUInt16(s.Slice(count));
count += sizeof(ushort);

{1}s.Clear();
for (int i = 0; i < {1}Len; ++i)
{{
    {0} {1} = new {0}();
    {1}.Deserialize(s, ref count);
    {1}s.Add({1});
}}
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

    // {0} 대문자 패킷 이름
    // {1} 소문자 패킷 이름
    public static string WriteListFormat =
"""
success &= BitConverter.TryWriteBytes(sp.Slice(count), (ushort){1}s.Count);
count += sizeof(ushort);

foreach ({0} {1} in {1}s)
{{
    success &= {1}.Serialize(sp, ref count);
}}
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
