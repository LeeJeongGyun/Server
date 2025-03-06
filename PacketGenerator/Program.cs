namespace PacketGenerator;

using System.Xml;

internal class Program
{
    /// <summary>
    ///     멤버를 파싱하기 위한 함수.
    /// </summary>
    /// <param name="reader">XML을 가리키는 인자.</param>
    public static void ParseMembers(XmlReader reader)
    {
        string packetName = reader["name"];
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

        ParseMembers(reader);
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
        }
    }
}
