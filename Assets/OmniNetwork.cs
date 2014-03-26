using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

[RequireComponent(typeof(NetworkView))]
public class OmniNetwork : MonoBehaviour {

    public const string url_add = "http://www.jessewatson.org/OmniDig/sqladd.php?";
    public const string url_view = "http://www.jessewatson.org/OmniDig/sqlview.php?";
    public const string url_del = "http://www.jessewatson.org/OmniDig/sqldel.php?";
    public const string url_load = "http://www.jessewatson.org/OmniDig/sqlload.php?";
    public const string url_save = "http://www.jessewatson.org/OmniDig/sqlsave.php?";
    public Component[] nonDedicatedObjects;
    static OmniNetwork instance;
    public static int MaxConnectTries = 5;
    public static int RetryTime = 5; // Every 5 frames
    public static bool Dedicated = false;
    Camera cam;
    OmniTerrain terrain;
    GameObject localPlayer;
    OmniObject playerObject;
    OmniLocal localInput;
    OmniWorld world;
    MemoryStream snapshot;
    BinaryReader snapshot_reader;
    BinaryWriter snapshot_writer;
    NetworkView clientView;
    int count=0;

    public Dictionary<NetworkPlayer, ClientControllable> connectedClients;
    List<OmniObject> updateList;

    public static List<Type> registeredClientEvents = new List<Type>();

    public static MemoryStream getSnapShot() { return instance.snapshot; }
    public static BinaryReader getSnapShotReader() { return instance.snapshot_reader; }
    public static BinaryWriter getSnapShotWriter() { return instance.snapshot_writer; }

    void OnEnable()
    {
        connectedClients = new Dictionary<NetworkPlayer, ClientControllable>();
        updateList = new List<OmniObject>();
        instance = this;
    }
    // Use this for initialization
    void Start()
    {
        terrain = GetComponent<OmniTerrain>();
        world = GetComponent<OmniWorld>();
        snapshot = new MemoryStream();
        snapshot_reader = new BinaryReader(snapshot);
        snapshot_writer = new BinaryWriter(snapshot);
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }


    void OnServerInitialized()
    {
        Debug.Log("Server Initialized");
        OmniWorld.RegMaster = true;
        if (Dedicated)
        {
            for (int i = 0; i < nonDedicatedObjects.Length; i++)
            {
                if (nonDedicatedObjects[i] is Transform)
                {
                    GameObject.Destroy(nonDedicatedObjects[i].gameObject);
                }
                else
                Component.Destroy(nonDedicatedObjects[i]);
            }
        }
    }

    void OnPlayerConnected(NetworkPlayer player)
    {

        networkView.RPC("t", player, OmniWorld.tick, OmniWorld.GetSpawnID());
        
        SendFullUpdate(player);
     
    }
    void OnPlayerDisconnected(NetworkPlayer player)
    {
        OmniSaveManager.Save(connectedClients[player]);
        Debug.Log("Player disconnected");
        DestroyEvent e = new DestroyEvent(OmniWorld.tick);
        e.objid = connectedClients[player].id;
        OmniEvents.AddEvent(e);
        connectedClients.Remove(player);
        NetworkView[] views = GetComponents<NetworkView>();
        for (int i = 0; i < views.Length; i++)
        {
            if (views[i].owner == player)
                GameObject.Destroy(views[i]);
        }
    }
    void OnConnectedToServer()
    {
        world.ClearWorld();        
    }

    public static NetworkView getView()
    {
        return instance.networkView;
    }

    public static void AddClient(ClientControllable obj, NetworkPlayer player)
    {
        instance.connectedClients.Add(player, obj);
    }


    void SendFullMapUpdate(NetworkPlayer player)
    {
        MemoryStream buf = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(buf);
        
        for (int i = 0; i < OmniTerrain.loadedChunks.Count; i++)
        {
            TerrainChunk ch = OmniTerrain.loadedChunks[i];
            writer.Write((int)ch.pos.x/OmniTerrain.chunkSize);
            writer.Write((int)ch.pos.y/OmniTerrain.chunkSize);
            for (int j = 0; j < ch.tileMap.Length; j++)
            {
                writer.Write(ch.tileMap[j]);
                writer.Write(ch.damageMap[j]);
            }
        }
        writer.Flush();
        networkView.RPC("FullMap", player,OmniTerrain.loadedChunks.Count,buf.ToArray());
    }
    [RPC]
    void onJoin(NetworkPlayer player)
    {
        if(player != Network.player)
        {
            networkView.RPC("v", player, clientView.viewID,SystemInfo.deviceUniqueIdentifier);
        }
    }
    [RPC]
    void v(NetworkViewID v, string guid)
    {

        if (Network.isServer)
        {
            NetworkView view = gameObject.AddComponent<NetworkView>();
            view.group = 2;
            view.observed = GetComponent<OmniClientNetwork>();
            view.viewID = v;
            view.stateSynchronization = NetworkStateSynchronization.Unreliable;


            LoadPlayerEvent e = new LoadPlayerEvent(OmniWorld.tick, guid, v.owner);
            OmniEvents.AddEvent(e);
        }
        
    }

    void SendFullUpdate(NetworkPlayer player)
    {
        MemoryStream buf = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(buf);
        byte[] send;
        List<OmniObject> list = OmniWorld.getSpawnedObjs();
        int c = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null)
                continue;


            c++;
            writer.Write(list[i].item.name);
            writer.Write(list[i].Name);
            writer.Write(list[i].GetType().Name);
            writer.Write(list[i].id);
            writer.Write(list[i].bounds.x);
            writer.Write(list[i].bounds.y);

        }
        writer.Flush();
        send = buf.ToArray();
        if (send == null)
        {
            Debug.Log("DIS IS BAD");
            send = new byte[] { 0 };
        }

        networkView.RPC("FullUpdate", player, OmniWorld.tick, c, send);
    }


    [RPC]
    void t(int tick, int id)
    {
        OmniWorld.tick = tick;
        OmniLocal.LocalID = id;

        
        clientView = gameObject.AddComponent<NetworkView>();
        clientView.group = 2;
        clientView.observed = GetComponent<OmniClientNetwork>();
        clientView.viewID = Network.AllocateViewID();
        clientView.stateSynchronization = NetworkStateSynchronization.Unreliable;

        networkView.RPC("v", RPCMode.Server, clientView.viewID,SystemInfo.deviceUniqueIdentifier);
        
        
    }


    [RPC]
    void giveItem(NetworkPlayer player, string name, int amt)
    {
        connectedClients[player].AddItemToBag(name, amt);
        UpdateInventory(player);
    }

    [RPC]
    void TryMoveHeld(NetworkPlayer player, int from, int to, int amt)
    {
        connectedClients[player].TryMoveHeld(from, to, amt);
        UpdateInventory(player);
    }

    [RPC]
    void eq(NetworkPlayer player, int id)
    {
        connectedClients[player].TryEquiptItem(id);
    }
    [RPC]
    void sel(NetworkPlayer player, int sel)
    {
        connectedClients[player].selected = sel;
        UpdateInventory(player);
    }

    [RPC]
    void FullUpdate(int tick,int count, byte[] data)
    {

        if (data.Length == 1)
        {
            Debug.Log("Bad update");
            return;
        }
        MemoryStream buf = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(buf);
        for (int i = 0; i < count; i++)
        {
            string name = reader.ReadString();
            string plname = reader.ReadString();
            string typename = reader.ReadString();
            int id = reader.ReadInt32();
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            if (id == OmniLocal.LocalID)
                continue;
            SpawnEvent e = new SpawnEvent(tick, name,plname, new Vector2(x, y), Vector2.zero,id, Type.GetType(typename));
            OmniEvents.AddEvent(e);

        }
        reader.Close();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ClientControllable[] objs = new ClientControllable[connectedClients.Values.Count];
            connectedClients.Values.CopyTo(objs, 0);
            for (int i = 0; i < connectedClients.Values.Count; i++)
            {
                Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition);
                objs[i].bounds.x = pos.x;
                objs[i].bounds.y = pos.y;
            }
        }
    }

    void UpdateInventory(NetworkPlayer player)
    {
        MemoryStream buf = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(buf);

        ClientControllable cl = connectedClients[player];

        for (int i = 0; i < OmniInventory.Bag_MaxItems; i++)
        {
            int id = -1;
            int amt = 0;
            if (cl.bagItems[i] != null)
            {
                id = cl.bagItems[i].id;
                amt = cl.bagItems[i].stack;
            }
            writer.Write(id);
            writer.Write(amt);
        }
        networkView.RPC("Inv", player, buf.ToArray());
    }

    [RPC]
    void Inv(byte[] data)
    {
        MemoryStream buf = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(buf);
        for (int i = 0; i < OmniInventory.Bag_MaxItems; i++)
        {
            int id = reader.ReadInt32();
            int amt = reader.ReadInt32();

            //if (id < 0 || amt == 0)
                //OmniLocal.getLocalPlayer().bag.bagItems[i] = null;
            //else
               // OmniLocal.getLocalPlayer().bag.bagItems[i] = new OmniItemObject(OmniItems.itemTypes[id], amt);
        }

    }
    public void OmniUpdate(float delta)
    {
        if (Network.isServer)
        {
            if (snapshot.Position > 0)
            {
                networkView.RPC("bl", RPCMode.Others, snapshot.ToArray());
                snapshot.Position = 0;
                snapshot.SetLength(0);
            }
        }
    }

    [RPC]
    void bl(byte[] data)
    {
       
        snapshot.SetLength(0);
        snapshot.Write(data,0,data.Length);
        snapshot.Position = 0;
        int size = (sizeof(int)*4);
        if (snapshot.Length % size == 0)
        {
            long len = snapshot.Length / size;
            for (int i = 0; i < len; i++)
            {
                int x = snapshot_reader.ReadInt32();
                int y = snapshot_reader.ReadInt32();
                int type = snapshot_reader.ReadInt32();
                int hp = snapshot_reader.ReadInt32();
                //BlockEvent e = new BlockEvent(OmniWorld.tick,x,y, type, hp);
                //OmniEvents.AddEvent(e);
            }
        }

    }
    [RPC]
    void delay(int delay)
    {

        OmniWorld.localDelay = delay;
        Debug.Log("Delay set to " + delay + " frames (" + OmniWorld.timeStep * OmniWorld.localDelay * 1000 + ")ms");
    }
    [RPC]
    void sync(NetworkPlayer player,int tick, float x, float y, float velx, float vely,bool grounded)
    {
        if (Network.isServer)
        {

            ControllableObject obj = connectedClients[player];

            networkView.RPC("sync", player, player,OmniWorld.tick, obj.bounds.x, obj.bounds.y, obj.vel.x, obj.vel.y,obj.grounded);
        }

        if (Network.isClient)
        {
            ClientControllable obj = OmniWorld.getSpawnedObjs()[OmniLocal.LocalID] as ClientControllable;
            obj.vel.x = velx;
            obj.vel.y = vely;
            obj.setFixedPos(tick, x, y);
            
        }
    }
    float[] skelAngles = new float[OmniAnimation.Max_Skeletons]; //24 max joints for now
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        int tick = 0;
        int objCount = 0;
        int playerID = 0;
        int updateMask = 0;
        float posX = 0;
        float posY = 0;
        bool flipped = false;
        int animID = 0;
        int mount0 = 0;
        int mount1 = 0;
        int mount2 = 0;
        float mount0time = 0;
        float mount1time = 0;
        float mount2time = 0;
        float rotation = 0;
        float clientLookRot = 0;
        int skelUpdateMask = 0;
        
        



        if (Network.isServer)
        {
            if (stream.isWriting)
            {
                tick = OmniWorld.tick;
                updateList.Clear();
                for (int i = 0; i < OmniWorld.instance.SpawnedObjectsNew.Count; i++)
                {
                    /*
                    if (OmniWorld.instance.SpawnedObjectsNew[i] is PhysicsObject)
                        if(((PhysicsObject)OmniWorld.instance.SpawnedObjectsNew[i]).isProjectile)
                            continue;

                     * */

                    OmniObject obj = OmniWorld.instance.SpawnedObjectsNew[i];

                    if (obj == null)
                        continue;

                    if(obj.skeleton != null)
                        for (int j = 0; j < obj.skeleton.Length; j++)
                        {
                            if (obj.skeleton[j].hasChanged())
                            {
                                obj.skelUpdateMask |= 1 << j;
                            }
                        }
                        if (obj.skelUpdateMask != 0)
                            obj.updateMask |= 1 << ObjectNetworkUpdate.Skeleton;

                    
                    


                    if (obj.updateMask != 0)
                    {
                        objCount++;
                        updateList.Add(obj);
                    }
                }
            }
        }



        if (Network.isServer && stream.isWriting || Network.isClient && stream.isReading)
        {
            stream.Serialize(ref tick);
            stream.Serialize(ref objCount);

            for (int i = 0; i < objCount; i++)
            {
                if (stream.isWriting && Network.isServer)
                {
                    playerID = updateList[i].id;
                    updateMask = updateList[i].updateMask;
                    posX = updateList[i].bounds.x;
                    posY = updateList[i].bounds.y;
                    flipped = updateList[i].flipped;
                    animID = updateList[i].animId;
                    mount0 = updateList[i].mount0;
                    mount1 = updateList[i].mount1;
                    mount2 = updateList[i].mount2;
                    mount0time = updateList[i].mount0time;
                    mount1time = updateList[i].mount1time;
                    mount2time = updateList[i].mount1time;
                    rotation = updateList[i].rotation;
                    skelUpdateMask = updateList[i].skelUpdateMask;
                    if(updateList[i].skeleton != null)
                        for (int j = 0; j < updateList[i].skeleton.Length; j++)
                        {
                            skelAngles[j] = updateList[i].skeleton[j].rotation;
                        }

                }
                stream.Serialize(ref playerID);
                stream.Serialize(ref updateMask);

                if ((updateMask & 1 << ObjectNetworkUpdate.Position) != 0)
                {
                    stream.Serialize(ref posX);
                    stream.Serialize(ref posY);
                }
                if ((updateMask & 1 << ObjectNetworkUpdate.Direction) != 0)
                    stream.Serialize(ref flipped);
                if ((updateMask & 1 << ObjectNetworkUpdate.Animation) != 0)
                    stream.Serialize(ref animID);
                if ((updateMask & 1 << ObjectNetworkUpdate.Mount0) != 0)
                {
                    stream.Serialize(ref mount0);
                    stream.Serialize(ref mount0time);
                }
                if ((updateMask & 1 << ObjectNetworkUpdate.Mount1) != 0)
                {
                    stream.Serialize(ref mount1);
                    stream.Serialize(ref mount1time);
                }
                if ((updateMask & 1 << ObjectNetworkUpdate.Mount2) != 0)
                {
                    stream.Serialize(ref mount2);
                    stream.Serialize(ref mount2time);
                }
                if ((updateMask & 1 << ObjectNetworkUpdate.Rotation) != 0)
                    stream.Serialize(ref rotation);
                if ((updateMask & 1 << ObjectNetworkUpdate.Skeleton) != 0)
                {
                    stream.Serialize(ref skelUpdateMask);
                    for (int j = 0; j < skelAngles.Length; j++)
                    {
                        if ((skelUpdateMask & 1 << j) != 0)
                            stream.Serialize(ref skelAngles[j]);
                    }
                }


                if (stream.isReading && Network.isClient)
                {

                    if (playerID == OmniLocal.LocalID)
                    {
                        ClientControllable cl = OmniLocal.getLocalPlayer();
                        if (cl != null)
                        if (cl.spawned)
                        {
                            if ((updateMask & 1 << ObjectNetworkUpdate.Position) != 0)
                                if (!OmniLocal.instance.predictionPass(tick, posX, posY))
                                {
                                    networkView.RPC("sync", RPCMode.Server,Network.player, tick, 0f, 0f, 0f, 0f,false);
                                }
                        }
                    }
                    else
                    {
                        if (playerID >= OmniWorld.instance.SpawnedObjectsNew.Count)
                            continue;

                        OmniObject obj = OmniWorld.instance.SpawnedObjectsNew[playerID];

                        if (obj == null)
                            continue;

                        if (!obj.isGhost)
                            continue;

                        if ((updateMask & 1 << ObjectNetworkUpdate.Position) != 0)
                        {
                            bool flag = false;
                            for (int j = 0; j < obj.mountObjects.Count; j++)
                            {
                                if (j == 0 && obj.mountObjects[j].obj.id == OmniLocal.LocalID)
                                    flag = true;
                            }
                            if (!flag)
                            {
                                obj.ghost.lastGhostPos.x = obj.bounds.x;
                                obj.ghost.lastGhostPos.y = obj.bounds.y;
                                obj.ghost.ghostPos.x = posX;
                                obj.ghost.ghostPos.y = posY;
                                obj.ghost.interpCounter = 0;
                            }
                            
                            //obj.ghost.ghostUpdates.Add(new GhostObject.ghost_update(posX, posY, tick+2));
                        }
                        if ((updateMask & 1 << ObjectNetworkUpdate.Direction) != 0)
                            obj.flipped = flipped;
                        if ((updateMask & 1 << ObjectNetworkUpdate.Animation) != 0)
                            obj.animId = animID;
                        if ((updateMask & 1 << ObjectNetworkUpdate.Mount0) != 0)
                        {
                            obj.mount0 = mount0;
                            obj.ghost.mount0ghostLast = obj.ghost.mount0ghostTime;
                            obj.ghost.mount0ghostTime = mount0time;
                            obj.ghost.mount0interp = 0;
                        }
                        else
                        {
                            obj.mount0 = -1;
                            obj.ghost.mount0ghostLast = 0;
                            obj.ghost.mount0ghostTime = 0;

                        }
                        if ((updateMask & 1 << ObjectNetworkUpdate.Mount1) != 0)
                        {
                            obj.mount1 = mount1;
                            obj.ghost.mount1ghostLast = obj.ghost.mount1ghostTime;
                            obj.ghost.mount1ghostTime = mount1time;
                            obj.ghost.mount1interp = 0;
                        }
                        else
                        {
                            obj.mount1 = -1;
                            obj.ghost.mount1ghostLast = 0;
                            obj.ghost.mount1ghostTime = 0;
                        }
                        if ((updateMask & 1 << ObjectNetworkUpdate.Mount2) != 0)
                        {
                            obj.mount2 = mount2;
                            obj.ghost.mount2ghostLast = obj.ghost.mount2ghostTime;
                            obj.ghost.mount2ghostTime = mount2time;
                            obj.ghost.mount2interp = 0;
                        }
                        else
                        {
                            obj.mount2 = -1;
                            obj.ghost.mount2ghostLast = 0;
                            obj.ghost.mount2ghostTime = 0;
                        }

                        if ((updateMask & 1 << ObjectNetworkUpdate.Rotation) != 0)
                        {

                            obj.ghost.lastRot = obj.ghost.currentRot;
                            obj.ghost.currentRot = rotation;
                            obj.ghost.rotCounter = 0;
                        }
                        if ((updateMask & 1 << ObjectNetworkUpdate.Skeleton) != 0)
                        {
                            for (int j = 0; j < skelAngles.Length; j++)
                            {
                                if ((skelUpdateMask & 1 << j) != 0)
                                {
                                    obj.ghost.skelLast[j] = obj.ghost.skelInterp[j];
                                    obj.ghost.skelInterp[j] = skelAngles[j];
                                    obj.ghost.skelCounter[j] = 0;
                                }
                            }
                            obj.ghost.skelMask = skelUpdateMask;
                        }

                    }

                }
                OmniWorld.instance.SpawnedObjectsNew[playerID].updateMask = 0;
                OmniWorld.instance.SpawnedObjectsNew[playerID].skelUpdateMask = 0;
            }
        }
    }


    [RPC]
    void netEvent(NetworkPlayer player,int tick,string name, byte[] data)
    {
        MemoryStream stream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(stream);

        System.Type type = System.Type.GetType(name);
        if (type == null)
            return;
        if (!typeof(NetworkEvent).IsAssignableFrom(type))
            return;


        NetworkEvent eventType = Activator.CreateInstance(type,tick) as NetworkEvent;

        if (eventType.type == NetworkSendType.Everyone)
            if (player == Network.player)
            {
                return;
            }

        eventType.player = player;
        FieldInfo[] info = type.GetFields();
        int mask = reader.ReadInt32();
        for (int i = 0; i < info.Length; i++)
        {
            if ((mask & (1 << i+1)) == 0)
                continue;

            if (info[i].FieldType.Name == "Single")
            {
                float val = reader.ReadSingle();
                info[i].SetValue(eventType, val);                
            }
            else if (info[i].FieldType.Name == "Char")
            {
                char val = reader.ReadChar();
                
                info[i].SetValue(eventType, val);
            }
            else if (info[i].FieldType.Name == "Type")
            {
                string nm = reader.ReadString();
                Type t = Type.GetType(nm);
                info[i].SetValue(eventType, t);

            }
            else if (info[i].FieldType.Name == "Boolean")
            {
                bool val = reader.ReadBoolean();

                info[i].SetValue(eventType, val);
            }
            else if (info[i].FieldType.Name == "Byte")
            {
                byte val = reader.ReadByte();
                info[i].SetValue(eventType, val);
            }
            else if (info[i].FieldType.Name == "String")
            {
                string val = reader.ReadString();
                info[i].SetValue(eventType, val);
            }
            else if (info[i].FieldType.Name == "Int32")
            {
                int val = reader.ReadInt32();
                info[i].SetValue(eventType, val);
            }
            else if (info[i].FieldType.Name == "Vector2")
            {
                Vector2 val = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                info[i].SetValue(eventType, val);
            }
            else if (info[i].FieldType.Name == "Int32[]")
            {

                int len = reader.ReadInt32();

                int[] val = new int[len];
                for (int j = 0; j < val.Length; j++)
                    val[j] = reader.ReadInt32();

                info[i].SetValue(eventType, val);
            }
            else if (info[i].FieldType.Name == "tUpdate[]")
            {
                int len = reader.ReadInt32();
                tUpdate[] val = new tUpdate[len];
                for (int j = 0; j < val.Length; j++)
                {
                    byte t = reader.ReadByte();

                    if (t == 1)
                        val[j] = new terrainDmg();
                    else if (t == 2)
                        val[j] = new terrainAddBlock();

                    FieldInfo[] field = val[j].GetType().GetFields();
                    for (int k = 0; k < field.Length; k++)
                    {
                        if (field[k].FieldType.Name == "Int32")
                        {
                            int v = reader.ReadInt32();
                            field[k].SetValue(val[j], v);
                        }
                    }
                }
                info[i].SetValue(eventType, val);

            }
            else if (typeof(OmniObject).IsAssignableFrom(info[i].FieldType))
            {
                int id = reader.ReadInt32();
                info[i].SetValue(eventType, OmniWorld.instance.SpawnedObjectsNew[id]);
            }
            else if (typeof(OmniItemType).IsAssignableFrom(info[i].FieldType))
            {
                int id = reader.ReadInt32();
                info[i].SetValue(eventType, OmniItems.itemTypes[id]);
            }


        }
        bool flag = false;
        /*
        if (Network.isClient)
        {
            for (int i = 0; i < OmniEvents.toValidate.Count; i++)
            {
                if (type == OmniEvents.toValidate[i].GetType())
                {
                    OmniEvents.toValidate[i].validate(eventType);
                    flag = true;
                }
            }
        }
         * */
        if(Network.isClient)
            eventType.fromServer = true;

        OmniEvents.AddEvent(eventType);

    }

    bool isClientSide(NetworkEvent e)
    {

        if (e is SpawnEvent)
        {
            SpawnEvent ev = e as SpawnEvent;
            if (ev.projectile)
                return true;
        }
        return false;

    }
    [RPC]
    void giv(NetworkPlayer pl)
    {
        int[] giv = new int[OmniItems.itemTypes.Length * 2];
        int c = 0;
        for (int i = 0; i < OmniItems.itemTypes.Length; i++)
        {
            giv[c++] = OmniItems.itemTypes[i].id;
            giv[c++] = 1;
        }

        InventoryEvent e = new InventoryEvent(OmniWorld.tick, connectedClients[pl].id, 'a', giv);
        e.player = pl;
        OmniEvents.AddEvent(e);
    }
    [RPC]
    void dir(NetworkPlayer player, bool flipped)
    {
        connectedClients[player].flipped = flipped;
    }
    [RPC]
    void updateEvent(int tick, string name, byte[] data)
    {

        if (!Network.isClient)
            return;

        MemoryStream stream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(stream);
        System.Type type = System.Type.GetType(name);
        if (type == null)
            return;


        object eventType = Activator.CreateInstance(type, tick);



        FieldInfo[] info = type.GetFields();

        for (int i = 0; i < info.Length; i++)
        {
            if (info[i].FieldType.Name == "Single")
            {
                float val = reader.ReadSingle();
                info[i].SetValue(eventType, val);
            }
            else if (info[i].FieldType.Name == "String")
            {
                string val = reader.ReadString();
                info[i].SetValue(eventType, val);
            }
            else if (info[i].FieldType.Name == "Int32")
            {
                int val = reader.ReadInt32();
                info[i].SetValue(eventType, val);
            }
        }
        OmniEvents.AddEvent(eventType as OmniEvent);

    }
    [RPC]
    void SpawnHax(NetworkPlayer player, string name)
    {
        ControllableObject owner = connectedClients[player];
        SpawnEvent e = new SpawnEvent(OmniWorld.tick);
        e.target = name;
        e.onHandle += onClientSpawn;
        e.player = player;
        e.pos.x = owner.bounds.x;
        e.pos.y = owner.bounds.y;
        OmniEvents.AddEvent(e);
    }
    void onClientSpawn(OmniEvent e)
    {
        SpawnEvent se = (SpawnEvent)e;
        //se.spawnedObject.GetComponent<OmniAI>().setTarget(connectedClients[se.player]);
    }
}
