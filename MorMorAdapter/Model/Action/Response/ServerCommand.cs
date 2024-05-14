﻿using ProtoBuf;

namespace MorMorAdapter.Model.Action.Response;

[ProtoContract]
public class ServerCommand : BaseActionResponse
{
    [ProtoMember(8)] public List<string> Params { get; set; }

    public ServerCommand(List<string> param) : base()
    {
        Params = param;
    }
}
