﻿namespace MorMorAdapter.Attributes;

[AttributeUsage(AttributeTargets.Method)]
internal class RestMatch : Attribute
{
    public string ApiPath { get; set; }

    public RestMatch(string api)
    {
        ApiPath = api;
    }
}
