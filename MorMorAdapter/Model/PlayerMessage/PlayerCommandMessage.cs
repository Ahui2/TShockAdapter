﻿using MorMorAdapter.Enumerates;
using Newtonsoft.Json;
using ProtoBuf;
using TShockAPI;

namespace MorMorAdapter.Model.PlayerMessage;

[ProtoContract]
internal class PlayerCommandMessage : BasePlayerMessage
{
    [ProtoMember(8)]  public string Command { get; set; }

    [ProtoMember(9)] public string CommandPrefix { get; set; }

}
