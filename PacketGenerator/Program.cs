namespace PacketGenerator;

using System.Xml;

internal class Program
{
    private static string GenPacketString = string.Empty;
    private static string packetEnums;
    private static int packetId = 0;

    public static (string member, string read, string write) ParseMemberList(XmlReader reader)
    {
        string packetName = reader["name"];
        if (string.IsNullOrEmpty(packetName))
        {
            Console.WriteLine("Packet Without Name");
            return default;
        }

        string memberCode = string.Empty;
        string readCode = string.Empty;
        string writeCode = string.Empty;

        var ret = ParseMembers(reader);
        string upperPacketName = ToUpper(packetName);
        string lowerPacketName = ToLower(packetName);

        memberCode = string.Format(PacketFormat.memberListFormat, upperPacketName, lowerPacketName, ret.member, ret.read, ret.write);
        readCode = string.Format(PacketFormat.ReadListFormat, upperPacketName, lowerPacketName);
        writeCode = string.Format(PacketFormat.WriteListFormat, upperPacketName, lowerPacketName);

        return (memberCode, readCode, writeCode);
    }

    /// <summary>
    ///     멤버를 파싱하기 위한 함수.
    /// </summary>
    /// <param name="reader">XML을 가리키는 인자.</param>
    public static (string member, string read, string write) ParseMembers(XmlReader reader)
    {
        string packetName = reader["name"];

        string memberCode = string.Empty;
        string readCode = string.Empty;
        string writeCode = string.Empty;

        int depth = reader.Depth + 1;
        while (reader.Read())
        {
            if (reader.Depth != depth)
                break;

            string memberName = reader["name"];
            if (string.IsNullOrEmpty(memberName))
                return default;

            if (string.IsNullOrEmpty(memberCode) == false)
                memberCode += Environment.NewLine;
            if (string.IsNullOrEmpty(readCode) == false)
                readCode += Environment.NewLine;
            if (string.IsNullOrEmpty(writeCode) == false)
                writeCode += Environment.NewLine;

            string type = reader.Name.ToLower();
            switch (type)
            {
            case "byte":
            case "sbyte":
                memberCode += string.Format(PacketFormat.memberFormat, type, memberName);
                readCode += string.Format(PacketFormat.ReadByteFormat, memberName, type);
                writeCode += string.Format(PacketFormat.WriteByteFormat, memberName, type);
                break;

            case "bool":
            case "short":
            case "ushort":
            case "int":
            case "long":
            case "float":
            case "double":
                memberCode += string.Format(PacketFormat.memberFormat, type, memberName);
                readCode += string.Format(PacketFormat.ReadFormat, memberName, TypeToFormat(type), type);
                writeCode += string.Format(PacketFormat.WriteFormat, memberName, type);
                break;

            case "string":
                memberCode += string.Format(PacketFormat.memberFormat, type, memberName);
                readCode += string.Format(PacketFormat.ReadStringFormat, memberName);
                writeCode += string.Format(PacketFormat.WriteStringFormat, memberName);
                break;

            case "list":
                var tuple = ParseMemberList(reader);
                memberCode += tuple.member;
                readCode += tuple.read;
                writeCode += tuple.write;
                break;

            default:
                break;
            }
        }

        memberCode = memberCode.Replace("\n", "\n\t");
        readCode = readCode.Replace("\n", "\n\t\t");
        writeCode = writeCode.Replace("\n", "\n\t\t");
        return (memberCode, readCode, writeCode);
    }

    /// <summary>
    ///     XML 패킷을 파싱하기 위한 함수.
    /// </summary>
    /// <param name="reader">XML을 가리키는 인자.</param>
    public static void ParsePacket(XmlReader reader)
    {
        if (reader == null)
            return;

        if (reader.NodeType == XmlNodeType.EndElement)
            return;

        if (reader.Name.ToLower() != "packet")
        {
            Console.WriteLine("Pakcet Name is not packet");
            return;
        }

        string packetName = reader["name"]; // reader.GetAttribute("name")
        if (string.IsNullOrEmpty(packetName))
        {
            Console.WriteLine("Packet Without Name");
            return;
        }

        (string member, string readFormat, string writeFormat) = ParseMembers(reader);
        GenPacketString += string.Format(PacketFormat.packetFormat, packetName, member, readFormat, writeFormat);
        GenPacketString += Environment.NewLine;
        GenPacketString += Environment.NewLine;

        packetEnums += string.Format(PacketFormat.packetEnumFormat, $"{packetName}Req", packetId++) + Environment.NewLine + "\t";
        packetEnums += string.Format(PacketFormat.packetEnumFormat, $"{packetName}Res", packetId++) + Environment.NewLine + "\t";
    }

    private static void Main(string[] args)
    {
        XmlReaderSettings setting = new XmlReaderSettings()
        {
            IgnoreComments = true,
            IgnoreWhitespace = true,
        };

        using (XmlReader reader = XmlReader.Create("PDL.xml", setting))
        {
            reader.MoveToContent();

            while (reader.Read())
            {
                if (reader.Depth == 1 && reader.NodeType == XmlNodeType.Element)
                    ParsePacket(reader);
            }

            string fileText = string.Format(PacketFormat.fileFormat, packetEnums, GenPacketString);
            File.WriteAllText("GenPackets.cs", fileText);
        }
    }

    private static string ToLower(string packetName) => packetName[0].ToString().ToLower() + packetName.Substring(1);

    private static string ToUpper(string packetName) => packetName[0].ToString().ToUpper() + packetName.Substring(1);

    private static string TypeToFormat(string type) => type switch
    {
        "bool" => "ToBoolean",
        "short" => "ToInt16",
        "ushort" => "ToUInt16",
        "int" => "ToInt32",
        "uint" => "ToUInt32",
        "long" => "ToInt64",
        "ulong" => "ToUInt64",
        "float" => "ToSingle",
        "double" => "ToDouble",
        _ => string.Empty
    };
}
