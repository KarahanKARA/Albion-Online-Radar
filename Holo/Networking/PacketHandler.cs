using AlbionRadar.AOEnums;
using AlbionRadar.Harvestable;
using AlbionRadar.Mobs;
using AlbionRadar.Player;
using PacketDotNet;
using PhotonPackageParser;
using SharpDX.Direct2D1;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AlbionRadar.Networking;

public sealed class PacketHandler : PhotonParser
{
    public void HandlePacket(object _, PacketCapture e)
    {
        UdpPacket packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data).Extract<UdpPacket>();

        try
        {
            ReceivePacket(packet.PayloadData);
        }
        catch (Exception)
        {
            //MainForm.Log(ex.ToString());
        }
    }

    protected override void OnEvent(byte code, Dictionary<byte, object> parameters)
    {
        if (code == 3)
        {
            parameters.Add(252, (short)EventCodes.Move);
            byte[] bytes = (byte[])parameters[1];

            parameters.Add(4, BitConverter.ToSingle(bytes, 9));
            parameters.Add(5, BitConverter.ToSingle(bytes, 13));
        }

        short codeID = parameters.TryGetValue(252, out var codeObj) ? (short)codeObj : (short)0;

        if (codeID == 0)
            return;

        EventCodes eventCode = (EventCodes)codeID;

        switch (eventCode)
        {
            case EventCodes.Leave:
                HandleLeave(parameters);
                break;
            case EventCodes.Move:
                HandlePlayerMovement(parameters);
                break;
            case EventCodes.NewCharacter:
                HandleNewPlayerEvent(parameters);
                break;
            case EventCodes.NewSimpleHarvestableObjectList:
                HandleNewSimpleHarvestableObjectList(parameters);
                break;
            case EventCodes.NewHarvestableObject:
                HandleNewHarvestableObject(parameters);
                break;
            case EventCodes.HarvestableChangeState:
                HandleHarvestableChangeState(parameters);
                break;
            case EventCodes.HarvestFinished:
                //HandleHarvestFinished(parameters);
                break;
            case EventCodes.MobChangeState:
                HandleMobChangeState(parameters);
                break;
            case EventCodes.NewMob:
                HandleNewMob(parameters);
                break;
            case EventCodes.JoinFinished:
                HandleJoinFinished();
                break;
        }
    }

    protected override void OnRequest(byte operationCode, Dictionary<byte, object> parameters)
    {
        OperationCodes code = parameters.TryGetValue(253, out var codeObj) ? (OperationCodes)Convert.ToInt16(codeObj) : 0;

        switch (code)
        {
            case OperationCodes.Move:
                HandleLocalPlayerMovement(parameters);
                break;
        }
    }

    protected override void OnResponse(byte OperationCode, short ReturnCode, string DebugMessage, Dictionary<byte, object> parameters)
    {
        OperationCodes code = parameters.TryGetValue(253, out var codeObj) ? (OperationCodes)Convert.ToInt16(codeObj) : 0;

        switch (code)
        {
            case OperationCodes.Join:
                HandleJoinOperation(parameters);
                break;
        }
    }

    private static void HandleMobChangeState(Dictionary<byte, object> parameters)
    {
        try
        {
            int mobId = Convert.ToInt32(parameters[0]);
            byte enchantmentLevel = Convert.ToByte(parameters[1]);

            MobsHandler.UpdateMobEnchantmentLevel(mobId, enchantmentLevel);
        }
        catch (Exception e)
        {
            Console.WriteLine($@"HandleMobChangeState: {e}");
        }
    }

    private static void HandleJoinFinished()
    {
        try
        {
            HarvestableHandler.Reset();
            MobsHandler.Reset();
            PlayerHandler.Reset();
        }
        catch (Exception e)
        {
            MainForm.Log($"HandleJoinFinished: {e}");
        }
    }

    private static void HandleNewMob(Dictionary<byte, object> parameters)
    {
        try
        {
            int id = Convert.ToInt32(parameters[0]);
            int typeId = Convert.ToInt32(parameters[1]);

            float[] loc = (float[])parameters[7];
            float posX = loc[0];
            float posY = loc[1];

            int health = parameters.TryGetValue(13, out object healthObj) ? Convert.ToInt32(healthObj) : 0;

            // Debug için tüm parametreleri logla
            string debugInfo = $"New Mob:\n";
            debugInfo += $"ID: {id}, TypeID: {typeId}, Health: {health}\n";
            debugInfo += $"Position: X={posX}, Y={posY}\n";
            
            var mobInfo = AlbionRadar.Mobs.MobInfo.GetMobInfo(typeId);
            if (mobInfo != null)
            {
                debugInfo += $"MobInfo: Tier={mobInfo.Tier}, Type={mobInfo.MobType}, HarvestableType={mobInfo.HarvestableMobType}\n";
            }
            else
            {
                debugInfo += "MobInfo: null\n";
            }
            
            debugInfo += "PARAMETERS:\n";
            
            foreach (var param in parameters)
            {
                if (param.Value != null)
                {
                    if (param.Value.GetType().IsArray)
                    {
                        var arr = param.Value as Array;
                        string arrayValues = "[ ";
                        if (arr != null)
                        {
                            foreach (var item in arr)
                            {
                                arrayValues += item + " ";
                            }
                        }
                        arrayValues += "]";
                        debugInfo += $"Key = {param.Key}, Value = {arrayValues}, Type = {param.Value.GetType()}\n";
                    }
                    else
                    {
                        debugInfo += $"Key = {param.Key}, Value = {param.Value}, Type = {param.Value.GetType()}\n";
                    }
                }
            }

            MainForm.Log(debugInfo);

            MobsHandler.AddMob(id, typeId, posX, posY, health);
        }
        catch (Exception e)
        {
            MainForm.Log($"HandleNewMob Error: {e}");
        }
    }

    private static void HandleNewSimpleHarvestableObjectList(Dictionary<byte, object> parameters)
    {
        List<int> a0 = [];

        if (parameters[0].GetType() == typeof(byte[]))
        {
            byte[] typeListByte = (byte[])parameters[0]; 
            foreach (byte b in typeListByte)
                a0.Add(b);
        }
        else if (parameters[0].GetType() == typeof(short[]))
        {
            short[] typeListByte = (short[])parameters[0];
            foreach (short b in typeListByte)
                a0.Add(b);
        }
        else
        {
            MainForm.Log("onNewSimpleHarvestableObjectList type error: " + parameters[0].GetType());
            return;
        }

        try
        {
            /*
            Key = 0, Value = System.Int16[] //id
            Key = 1, Value = System.Byte[] // type WOOD etc
            Key = 2, Value = System.Byte[] // tier
            Key = 3, Value = System.Single[] //location
            Key = 4, Value = System.Byte[] // size
            Key = 252, Value = 29
             */
            byte[] a1 = (byte[])parameters[1]; //list of types
            byte[] a2 = (byte[])parameters[2]; //list of tiers
            float[] a3 = (float[])parameters[3]; //list of positions X1, Y1, X2, Y2 ...
            byte[] a4 = (byte[])parameters[4]; //size

            for (int i = 0; i < a0.Count; ++i)
            {
                int id = a0[i];
                byte type = a1[i];
                byte tier = a2[i];
                float posX = a3[i * 2];
                float posY = a3[i * 2 + 1];
                byte count = a4[i];

                HarvestableHandler.AddHarvestable(id, type, tier, posX, posY, 0, count);
            }

        }
        catch (Exception e)
        {
            MainForm.Log($"HandleNewSimpleHarvestableObjectList: {e}");
        }
    }

    private static void HandleNewHarvestableObject(Dictionary<byte, object> parameters)
    {
        try
        {
            int id = Convert.ToInt32(parameters[0]);
            byte type = Convert.ToByte(parameters[5]);
            byte tier = Convert.ToByte(parameters[7]);
            float[] loc = (float[])parameters[8];

            float posX = loc[0];
            float posY = loc[1];

            byte size = parameters.TryGetValue(10, out var sizeObj) ? Convert.ToByte(sizeObj) : (byte)0;
            byte charges = parameters.TryGetValue(11, out var chargesObj) ? Convert.ToByte(chargesObj) : (byte)0;

            HarvestableHandler.AddHarvestable(id, type, tier, posX, posY, charges, size);
        }
        catch (Exception e)
        {
            MainForm.Log($"HandleNewHarvestableObject: {e}");
        }
    }

    private static void HandleHarvestableChangeState(Dictionary<byte, object> parameters)
    {
        try
        {
            int id = Convert.ToInt32(parameters[0]);
            byte size = parameters.TryGetValue(1, out object sizeObj) ? Convert.ToByte(sizeObj) : (byte)0;

            HarvestableHandler.UpdateHarvestable(id, size);
        }
        catch (Exception e)
        {
            MainForm.Log($"HandleHarvestableChangeState: {e}");
        }
    }

    private static void HandleLeave(Dictionary<byte, object> parameters)
    {
        try
        {
            int id = Convert.ToInt32(parameters[0]);
            PlayerHandler.RemovePlayer(id);
        }
        catch (Exception e)
        {
            MainForm.Log($"HandleLeave {e}");
        }
    }

    private static void HandleLocalPlayerMovement(Dictionary<byte, object> parameters)
    {
        try
        {
            float[] location = (float[])parameters[1];
            float posX = Convert.ToSingle(location[0]);
            float posY = Convert.ToSingle(location[1]);

            PlayerHandler.UpdateLocalPlayerPosition(posX, posY);
        }
        catch (Exception e)
        {
            MainForm.Log($"HandleLocalPlayerMovement: {e}");
        }
    }


    private static void HandlePlayerMovement(Dictionary<byte, object> parameters)
    {
        try
        {
            int id = Convert.ToInt32(parameters[0]);
            
            if (parameters.TryGetValue(1, out var payloadObj) && payloadObj is byte[] payload)
            {
                var localPos = PlayerHandler.GetLocalPlayerPosition();
                if (localPos.X == 0 && localPos.Y == 0)
                    return;

                var playerPos = PlayerHandler.GetPlayerPosition(id);
                if (!playerPos.HasValue)
                {
                    Random rnd = new Random(id);

                    float angle = (float)(id % 360) * ((float)Math.PI / 180);
                    float distance = 10 + (id % 20);

                    float relX = (float)Math.Cos(angle) * distance;
                    float relY = (float)Math.Sin(angle) * distance;

                    float posX = localPos.X + relX;
                    float posY = localPos.Y + relY;

                    PlayerHandler.UpdatePlayerPosition(id, posX, posY);
                    return;
                }

                var localMovement = PlayerHandler.GetLocalPlayerMovement();
                if (localMovement.HasValue)
                {
                    float newX = playerPos.Value.X + localMovement.Value.deltaX;
                    float newY = playerPos.Value.Y + localMovement.Value.deltaY;

                    PlayerHandler.UpdatePlayerPosition(id, newX, newY);
                }
            }
        }
        catch (Exception e)
        {
            MainForm.Log($"HandlePlayerMovement: {e}");
        }
    }


    private static void HandleNewPlayerEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            int id = Convert.ToInt32(parameters[0]);
            string nick = Convert.ToString(parameters[1]);
            string guild = parameters.TryGetValue(8, out var guildObj) ? Convert.ToString(guildObj) : string.Empty;
            string alliance = parameters.TryGetValue(49, out var allianceObj) ? Convert.ToString(allianceObj) : string.Empty;

            // PK durumunu kontrol et
            bool isPKEnabled = false;
            if (parameters.TryGetValue(46, out var pkObj))
            {
                isPKEnabled = Convert.ToBoolean(pkObj);
            }

            float angle = ((id % 360) * (float)Math.PI) / 180.0f; 
            float distance = 30.0f;

            float posX = distance * (float)Math.Cos(angle);
            float posY = distance * (float)Math.Sin(angle);

            PlayerHandler.AddPlayer(posX, posY, nick, guild, alliance, id, isPKEnabled);
        }
        catch (Exception e)
        {
            MainForm.Log($"HandleNewPlayerEvent: {e}");
        }
    }


    public static int LocalPlayerId { get; set; } = -1;
    private static void HandleJoinOperation(Dictionary<byte, object> parameters)
    {
        string mapStr = parameters.TryGetValue(8, out var mapObj) ? Convert.ToString(mapObj) : string.Empty;

        if (string.IsNullOrEmpty(mapStr))
        {
            MainForm.Log($"Can't parse mapID: {mapStr}");
            return;
        }

        string zoneType = "BLUE"; 
        if (mapStr.Contains("BLACK"))
            zoneType = "BLACK";
        else if (mapStr.Contains("RED"))
            zoneType = "RED";
        else if (mapStr.Contains("YELLOW"))
            zoneType = "YELLOW";

        if (parameters.TryGetValue(249, out var playerIdObj))
        {
            LocalPlayerId = Convert.ToInt32(playerIdObj);
            MainForm.Log($"Yerel oyuncu ID: {LocalPlayerId}");
        }

        PlayerHandler.MapID = mapStr;
        PlayerHandler.ZoneType = zoneType;
        MainForm.Log($"Zone tipi: {zoneType}");
    }
}
