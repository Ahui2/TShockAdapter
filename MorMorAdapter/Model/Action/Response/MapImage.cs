﻿using ProtoBuf;


namespace MorMorAdapter.Model.Action.Response;

[ProtoContract]
public class MapImage : BaseActionResponse
{
    [ProtoMember(8)] public byte[] Buffer { get; set; }

    public MapImage(byte[] buffer) : base()
    {
        Buffer = buffer;
    }
}
