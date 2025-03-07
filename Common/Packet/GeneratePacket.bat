START ..\..\PacketGenerator\bin\PacketGenerator.exe ..\..\PacketGenerator\PDL.xml

XCOPY GenPackets.cs "..\..\Server\Packet\"	/Y	/D
XCOPY GenPackets.cs "..\..\DummyClient\Packet\"	/Y	/D