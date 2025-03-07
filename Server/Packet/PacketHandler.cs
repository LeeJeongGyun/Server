using ServerCore;

/// <summary>
///     자동화 코드에 의해 호출되는 부분.
///     컨텐츠 코드는 이쪽에서 부터 작성하면 된다.
/// </summary>
internal class PacketHandler
{
    public static void C2S_PlayerInfoReqHandler(PacketSession session, IPacket packet)
    {
        C2S_PlayerInfoReq? p = packet as C2S_PlayerInfoReq;

        Console.WriteLine($"[SERVER] testBytes: {p.testByte}");
        Console.WriteLine($"[SERVER] PlayerId: {p.playerId}");
        Console.WriteLine($"[SERVER] Player Name: {p.name}");

        foreach (var skillInfo in p.skills)
        {
            Console.WriteLine($"Id: {skillInfo.id}, Level: {skillInfo.level}, Duration: {skillInfo.duration}");
        }
    }
}
