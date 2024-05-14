﻿using ProtoBuf;

namespace MorMorAdapter.Model.Action.Receive;


[ProtoContract]
public class BroadcastArgs : BaseAction
{
    [ProtoMember(5)] public string Text { get; set; }

    [ProtoMember(6)] public byte[] Color { get; set; }

}
