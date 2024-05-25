﻿using MorMorAdapter.Enumerates;
using MorMorAdapter.Model.Action;
using MorMorAdapter.Model.Action.Receive;
using MorMorAdapter.Model.Action.Response;
using MorMorAdapter.Model.Internet;
using MorMorAdapter.Net;
using ProtoBuf;
using Terraria;
using Terraria.IO;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;

namespace MorMorAdapter;

public class ActionHandler
{
    public delegate void ActionHandlerArgs(BaseAction action, MemoryStream stream);

    private static readonly Dictionary<ActionType, ActionHandlerArgs> _action = new()
    {
        { ActionType.DeadRank , DeadRankHandler },
        { ActionType.OnlineRank , OnlineRankHandler },
        { ActionType.WorldMap , WorldMapHandler },
        { ActionType.GameProgress , GameProgressHandler },
        { ActionType.UpLoadWorld , UploadWorldHandler },
        { ActionType.Inventory , InventoryHandler },
        { ActionType.ResetServer , RestServerHandler },
        { ActionType.ServerOnline , ServerOnlineHandler },
        { ActionType.RegisterAccount , RegisterAccountHandler },
        { ActionType.PluginMsg , PluginMsgHandler },
        { ActionType.PrivateMsg , PrivateMsgHandler },
        { ActionType.Command , CommandHandler },
        { ActionType.ReStartServer , ReStartServerHandler },
        { ActionType.ServerStatus , ServerStatusHandler },
        { ActionType.ResetPassword , ResetPasswordHandler },
        { ActionType.ConnectStatus, ConnectStatusHandler }
    };

    private static void ConnectStatusHandler(BaseAction action, MemoryStream stream)
    {
        var data = Serializer.Deserialize<SocketConnectStatusArgs>(stream);
        switch(data.Status)
        {
            case SocketConnentType.Success:
                TShock.Log.ConsoleInfo($"[MorMorAdapter] 与({data.ServerName})服务器验证成功，成功连接到MorMor机器人...");
                break;
            case SocketConnentType.VerifyError:
                TShock.Log.ConsoleError($"[MorMorAdapter] 与({data.ServerName})服务器的通信令牌验证失败...");
                break;
            case SocketConnentType.ServerNull:
                TShock.Log.ConsoleError($"[MorMorAdapter] 无法在MorMor机器人上找到({data.ServerName})服务器...");
                break;
            default:
                TShock.Log.ConsoleError("[MorMorAdapter] 因未知错误无验证通信令牌...");
                break;
        }
        ResponseAction(new BaseActionResponse()
        { 
            Status = true
        });
    }

    private static void ResetPasswordHandler(BaseAction action, MemoryStream stream)
    {
        var data = Serializer.Deserialize<PlayerPasswordResetArgs>(stream);
        var msg = "更改成功";
        var status = true;
        var account = new UserAccount()
        {
            Name = data.Name
        };
        try
        {
            TShock.UserAccounts.SetUserAccountPassword(account, data.Password);
        }
        catch (UserAccountNotExistException)
        {
            msg = "所更改的玩家账户不存在!";
            status = false;
        }
        catch (UserAccountManagerException)
        {
            msg = $"尝试更改{data.Name} 的密码失败，原因不明,请查看服务器控制台了解详情。";
            status = false;
        }
        catch (ArgumentOutOfRangeException)
        {
            msg = $"密码必须不少于{TShock.Config.Settings.MinimumPasswordLength}个字符";
            status = false;
        }
        var res = new BaseActionResponse()
        {
            Status = status,
            Message = msg,
            Echo = data.Echo
        };
        ResponseAction(res);
    }

    public static void Adapter(BaseAction action, MemoryStream stream)
    {
        stream.Position = 0;
        if (_action.TryGetValue(action.ActionType, out var Handler))
            Handler(action, stream);
    }

    private static void ResponseAction<T>(T obj) where T : BaseAction
    {
        obj.MessageType = PostMessageType.Action;
        WebSocketReceive.SendMessage(Utils.SerializeObj(obj));
    }

    private static void ServerStatusHandler(BaseAction action, MemoryStream stream)
    {
        var res = new ServerStatus()
        {
            Status = true,
            Message = "获取成功",
            Echo = action.Echo,
            WorldName = Main.worldName,
            WorldID = Main.worldID,
            WorldMode = Main.GameMode,
            WorldSeed = WorldGen.currentWorldSeed,
            Plugins = ServerApi.Plugins.Select(x => new PluginInfo()
            {
                Name = x.Plugin.Name,
                Author = x.Plugin.Author,
                Description = x.Plugin.Description,
            }).ToList(),
            WorldHeight = Main.maxTilesY,
            WorldWidth = Main.maxTilesX,
            TShockPath = Environment.CurrentDirectory,
            RunTime = (DateTime.Now - System.Diagnostics.Process.GetCurrentProcess().StartTime)
        };
        ResponseAction(res);
    }

    private static void ReStartServerHandler(BaseAction action, MemoryStream stream)
    {
        var data = Serializer.Deserialize<ReStartServerArgs>(stream);
        var res = new BaseActionResponse()
        {
            Status = true,
            Message = "正在进行重启",
            Echo = action.Echo
        };
        ResponseAction(res);
        Utils.ReStarServer(data.StartArgs, true);
    }

    private static void CommandHandler(BaseAction action, MemoryStream stream)
    {
        var data = Serializer.Deserialize<ServerCommandArgs>(stream);
        var player = new OneBotPlayer("MorMorBot");
        Commands.HandleCommand(player, data.Text);
        var res = new ServerCommand(player.CommandOutput)
        {
            Status = true,
            Message = "执行成功",
            Echo = data.Echo
        };
        ResponseAction(res);
    }

    private static void PrivateMsgHandler(BaseAction action, MemoryStream stream)
    {
        var data = Serializer.Deserialize<PrivatMsgArgs>(stream);
        TShock.Players.FirstOrDefault(x => x != null && x.Name == data.Name && x.Active)
            ?.SendMessage(data.Text, data.Color[0], data.Color[1], data.Color[2]);
        var res = new BaseActionResponse()
        {
            Status = true,
            Message = "发送成功",
            Echo = data.Echo
        };
        ResponseAction(res);
    }

    private static void PluginMsgHandler(BaseAction action, MemoryStream stream)
    {
        var data = Serializer.Deserialize<BroadcastArgs>(stream);
        TShock.Utils.Broadcast(data.Text, data.Color[0], data.Color[1], data.Color[2]);
        var res = new BaseActionResponse()
        {
            Status = true,
            Message = "发送成功",
            Echo = data.Echo
        };
        ResponseAction(res);
    }

    private static void RegisterAccountHandler(BaseAction action, MemoryStream stream)
    {
        var res = new BaseActionResponse()
        {
            Echo = action.Echo
        };
        var data = Serializer.Deserialize<RegisterAccountArgs>(stream);
        try
        {
            var account = new UserAccount()
            {
                Name = data.Name,
                Group = data.Group
            };
            account.CreateBCryptHash(data.Password);
            TShock.UserAccounts.AddUserAccount(account);
            res.Status = true;
            res.Message = "注册成功";
        }
        catch (Exception ex)
        {
            res.Status = false;
            res.Message = ex.Message;
        }
        ResponseAction(res);
    }

    private static void ServerOnlineHandler(BaseAction action, MemoryStream stream)
    {
        var players = TShock.Players.Where(x => x != null && x.Active).Select(x => new PlayerInfo(x)).ToList();
        var res = new ServerOnline(players)
        {
            Status = true,
            Message = "查询成功",
            Echo = action.Echo
        };
        ResponseAction(res);
    }

    private static void RestServerHandler(BaseAction action, MemoryStream stream)
    {
        var data = Serializer.Deserialize<ResetServerArgs>(stream);
        var res = new BaseActionResponse()
        {
            Status = true,
            Message = "正在重置",
            Echo = data.Echo
        };
        ResponseAction(res);
        Utils.RestServer(data);
    }

    private static void InventoryHandler(BaseAction action, MemoryStream stream)
    {
        var data = Serializer.Deserialize<QueryPlayerInventoryArgs>(stream);
        var inventory = Utils.BInvSee(data.Name);
        var res = new PlayerInventory(inventory)
        {
            Status = inventory != null,
            Message = "",
            Echo = data.Echo
        };
        ResponseAction(res);
    }

    private static void UploadWorldHandler(BaseAction action, MemoryStream stream)
    {
        WorldFile.SaveWorld();
        var buffer = File.ReadAllBytes(Main.worldPathName);
        var res = new UpLoadWorldFile()
        {
            Status = true,
            Message = "成功",
            Echo = action.Echo,
            WorldBuffer = buffer,
            WorldName = Main.worldName
        };
        ResponseAction(res);
    }

    private static void GameProgressHandler(BaseAction action, MemoryStream stream)
    {
        var res = new GameProgress(Utils.GetGameProgress())
        {
            Status = true,
            Message = "进度查询成功",
            Echo = action.Echo
        };
        ResponseAction(res);
    }

    private static void WorldMapHandler(BaseAction action, MemoryStream stream)
    {
        var data = Serializer.Deserialize<MapImageArgs>(stream);
        var buffer = Utils.CreateMapBytes(data.ImageType);
        var res = new MapImage(buffer)
        {
            Status = true,
            Message = "地图生成成功",
            Echo = data.Echo
        };
        ResponseAction(res);
    }

    private static void OnlineRankHandler(BaseAction action, MemoryStream stream)
    {
        var res = new PlayerOnlineRank(Plugin.Onlines)
        {
            Status = true,
            Message = "在线排行查询成功",
            Echo = action.Echo
        };
        ResponseAction(res);
    }

    private static void DeadRankHandler(BaseAction action, MemoryStream stream)
    {
        var res = new DeadRank(Plugin.Deaths)
        {
            Status = true,
            Message = "死亡排行查询成功",
            Echo = action.Echo
        };
        ResponseAction(res);
    }
}
