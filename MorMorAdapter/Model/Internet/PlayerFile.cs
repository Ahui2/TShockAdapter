﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace MorMorAdapter.Model.Internet;

[ProtoContract]
public class PlayerFile
{
    [ProtoMember(1)] public byte[] Buffer { get; set; } = Array.Empty<byte>();

    [ProtoMember(2)] public string Name { get; set; } = string.Empty;

    [ProtoMember(3)] public bool Active { get; set; } = true;
}
