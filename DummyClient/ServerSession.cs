namespace DummyClient;

using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using ServerCore;

internal class ServerSession : PacketSession
{
    /// <inheritdoc/>
    /// <remarks>
    ///     size 계산은 데이터를 전부 Write한 후에 알 수 있기 때문에 마지막에 Write한다.
    /// </remarks>
    public override void OnConnected(EndPoint endPoint)
    {
        C2S_PlayerInfoReq pInfo = new C2S_PlayerInfoReq() { testByte = 20, playerId = 1001, name = "jklee" };
        pInfo.skills.Add(new C2S_PlayerInfoReq.Skill() { id = 1, level = 10, duration = 10.0f });
        pInfo.skills.Add(new C2S_PlayerInfoReq.Skill() { id = 2, level = 20, duration = 20.0f });
        pInfo.skills.Add(new C2S_PlayerInfoReq.Skill() { id = 3, level = 30, duration = 30.0f });
        pInfo.skills.Add(new C2S_PlayerInfoReq.Skill() { id = 4, level = 40, duration = 40.0f });

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
    }

    /// <inheritdoc/>
    public override void OnSend(int byteOfTransferred)
    {
        Console.WriteLine($"Transferred Data: {byteOfTransferred}");
    }
}
