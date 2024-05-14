﻿using ProtoBuf;

namespace MorMorAdapter.Model.Action.Response;

[ProtoContract]
public class GameProgress : BaseActionResponse
{
    [ProtoMember(8)] public Dictionary<string, bool> Progress { get; set; }

    public GameProgress(Dictionary<string, bool> prog) : base()
    {
        Progress = prog;
    }
}
