using System;
using UnityEngine;
using System.Collections;
using System.Reflection;
using System.IO;

public class NetworkSendType
{
    public const byte ClientUnreliable = 1;
    public const byte ServerCommandAll = 2;
    public const byte ServerCommandSingle = 3;
    public const byte ClientReliable = 4;
    public const byte Everyone = 5;
    public const byte None = 6;
}

public class NetworkEvent : OmniEvent
{
    static string[] AllowedTypes = { "Single", "String", "Int32" };

    public NetworkPlayer player;
    public byte type;
    public bool fromServer;
    MemoryStream stream;
    //StreamReader reader;
    BinaryWriter writer;


    public NetworkEvent(int tick)
        : base(tick)
    {



    }

    int getMask(FieldInfo[] info)
    {
        int mask = 0;
        for (int i = 0; i < info.Length; i++)
        {
            object o = info[i].GetValue(this);
            if (o != null)
            {
                mask |= 1 << i + 1;
            }
            else
            {
            }
        }
        return mask;

    }

    bool shouldSend()
    {
        if (!Network.isServer)
            if (!Network.isClient)
                return false;

        if (Network.isServer)
        {
            if (type == NetworkSendType.ServerCommandSingle && player == Network.player)
                return false;
            if (type == NetworkSendType.ClientUnreliable)
                return false;
            if (type == NetworkSendType.ClientReliable)
            {
                type = NetworkSendType.ServerCommandSingle;
                return true;
            }
        }


        if (Network.isClient)
        {
            if (type == NetworkSendType.ServerCommandAll)
                return false;
            if (type == NetworkSendType.ServerCommandSingle)
                return false;
        }

        if (type == NetworkSendType.Everyone)
        {
            if (player != Network.player)
                return false;
        }

        return true;
    }

    public override void init()
    {
        FieldInfo[] info = GetType().GetFields();
        int mask = getMask(info);
        if (shouldSend())
        {
            stream = new MemoryStream();
            writer = new BinaryWriter(stream);
            

            
            writer.Write(mask);

            for (int i = 0; i < info.Length; i++)
            {
                if ((mask & (1 << i+1)) == 0)
                    continue;
                if (info[i].FieldType.Name == "Single")
                {
                    float val = (float)info[i].GetValue(this);
                    writer.Write(val);
                }
                else if (info[i].FieldType.Name == "Char")
                {
                    char val = (char)info[i].GetValue(this);
                    writer.Write(val);
                }
                else if (info[i].FieldType.Name == "Type")
                {
                    Type val = (Type)info[i].GetValue(this);
                    writer.Write(val.Name);
                }
                else if (info[i].FieldType.Name == "Boolean")
                {
                    bool val = (bool)info[i].GetValue(this);
                    writer.Write(val);
                }
                else if (info[i].FieldType.Name == "Byte")
                {
                    byte val = (byte)info[i].GetValue(this);
                    
                    writer.Write(val);
                }
                else if (info[i].FieldType.Name == "String")
                {
                    string val = (string)info[i].GetValue(this);
                    writer.Write(val);
                }
                else if (info[i].FieldType.Name == "Int32")
                {
                    int val = (int)info[i].GetValue(this);
                    writer.Write(val);
                }
                else if (info[i].FieldType.Name == "Vector2")
                {
                    Vector2 val = (Vector2)info[i].GetValue(this);
                    writer.Write(val.x);
                    writer.Write(val.y);
                }
                else if (info[i].FieldType.Name == "Int32[]")
                {
                    int[] val = (int[])info[i].GetValue(this);
                    writer.Write(val.Length);
                    for (int j = 0; j < val.Length; j++)
                        writer.Write(val[j]);
                }
                else if (info[i].FieldType.Name == "tUpdate[]")
                {
                    tUpdate[] val = (tUpdate[])info[i].GetValue(this);
                    writer.Write(val.Length);
                    for (int j = 0; j < val.Length; j++)
                    {
                        if (val[j] is terrainDmg)
                        {
                            writer.Write((byte)1);
                            
                        }
                        else if (val[j] is terrainAddBlock)
                        {
                            writer.Write((byte)2);

                        }
                        FieldInfo[] len = val[j].GetType().GetFields();
                        for (int k = 0; k < len.Length; k++)
                        {
                            if (len[k].FieldType.Name == "Int32")
                            {
                                int v = (int)len[k].GetValue(val[j]);
                                writer.Write(v);
                            }
                        }
                    }

                }
                else if (typeof(OmniObject).IsAssignableFrom(info[i].FieldType))
                {
                    OmniObject val = (OmniObject)info[i].GetValue(this);
                    writer.Write(val.id);
                }
                else if (typeof(OmniItemType).IsAssignableFrom(info[i].FieldType))
                {
                    OmniItemType val = (OmniItemType)info[i].GetValue(this);
                    writer.Write(val.id);
                }
                
                
            }
            writer.Flush();

            RPCMode r = RPCMode.Server;
            switch (type)
            {
                case NetworkSendType.ServerCommandAll:
                    r = RPCMode.Others;
                    break;
                case NetworkSendType.ClientReliable:
                case NetworkSendType.ClientUnreliable:
                    r = RPCMode.Server;
                    break;
                case NetworkSendType.Everyone:
                    r = RPCMode.All;
                    break;
            }

            if(type == NetworkSendType.ServerCommandSingle)
                OmniWorld.netView.RPC("netEvent", player, player, OmniWorld.tick, GetType().Name, stream.ToArray());
            else
                OmniWorld.netView.RPC("netEvent", r,player,OmniWorld.tick, GetType().Name, stream.ToArray());
        }
    }

    public override void handle(int tick)
    {
        if (type == NetworkSendType.ClientReliable)
        {
            forceDelete = true;
        }
        else
        base.handle(tick);
        
    }

    bool isTypeAllowed(string name)
    {
        foreach (string n in AllowedTypes)
            if (n == name)
                return true;

        return false;
    }
}

