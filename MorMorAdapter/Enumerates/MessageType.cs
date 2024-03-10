﻿using System.ComponentModel;

namespace MorMorAdapter.Enumerates;

public enum MessageType
{
    /// <summary>
    /// 服务器初始化完成
    /// </summary>
    [Description("game_post_init")]
    GamePostInit,

    /// <summary>
    /// 玩家进入服务器
    /// </summary>
    [Description("player_join")]
    PlayerJoin,

    /// <summary>
    /// 玩家离开服务器
    /// </summary>
    [Description("player_leave")]
    PlayerLeave,

    /// <summary>
    /// 玩家消息
    /// </summary>
    [Description("player_message")]
    PlayerMessage,

    /// <summary>
    /// 玩家指令
    /// </summary>
    [Description("player_command")]
    PlayerCommand,

    /// <summary>
    /// 心跳包
    /// </summary>
    [Description("heart_beat")]
    HeartBeat,

    [Description("connect")]
    Connect,
    /// <summary>
    /// 公共消息
    /// </summary>
    [Description("public")]
    PluginMsg,

    /// <summary>
    /// 似有消息
    /// </summary>
    [Description("private")]
    PrivateMsg
}
