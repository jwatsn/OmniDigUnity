using System;
using System.Collections.Generic;
using UnityEngine;

public class LoadPlayerEvent : OmniEvent
{

    string guid;
    string name;
 //   WWW loadPost;
    NetworkPlayer player;

    public LoadPlayerEvent(int tick,string guid,NetworkPlayer player)
        : base(tick)
    {
        this.player = player;
        //loadPost = new WWW(OmniNetwork.url_load + "sid="+WWW.EscapeURL(SystemInfo.deviceUniqueIdentifier) + "&pid="+WWW.EscapeURL(guid));
    }

    public override void handle(int tick)
    {
        base.handle(tick);


        if (handled)
        {

            int posX = -1;
            int posY = -1;
            /*
            string[] args = loadPost.text.Split(':');
            for (int i = 0; i < args.Length; i++)
            {
                string[] item = args[i].Split(',');
                if (item.Length > 1)
                {
                    if (item[0] == "Name")
                        name = item[1];
                    else if (item[0] == "PosX")
                        posX = int.Parse(item[1]);
                    else if (item[0] == "PosY")
                        posY = int.Parse(item[1]);
                    else if (item[0] == "GUID")
                        guid = item[1];

                }
                
            }
             * */
            Vector2 spawnPos;
            if (posX == -1 || posY == -1) //new character
            {
                spawnPos = OmniWorld.getSpawnPoint();
            }
            else
            {
                spawnPos = new Vector2(posX, posY);
            }
            SpawnEvent e = new SpawnEvent(OmniWorld.tick, "Bob",name, spawnPos, Vector2.zero, OmniWorld.GetSpawnID(), typeof(ClientControllable));
            e.player = player;
            e.onHandle += PlayerSpawned;
            OmniEvents.AddEvent(e);
        }
    }

    void PlayerSpawned(OmniEvent e)
    {
        if (!Network.isServer)
            return;

        SpawnEvent ev = e as SpawnEvent;
        ClientControllable cl = OmniWorld.instance.SpawnedObjectsNew[ev.id] as ClientControllable;
        cl.Name = name;
        cl.guid = guid;

        

        cl.delay = (int)(Network.GetAveragePing(ev.player) * OmniWorld.timeStep) + 2;
        Debug.Log(cl.Name + " ping is " + Network.GetAveragePing(ev.player) + " setting delay to " + cl.delay);
        OmniWorld.netView.RPC("delay", ev.player, cl.delay);
        OmniNetwork.AddClient(cl, ev.player);


        int[] giv = new int[OmniItems.itemTypes.Length * 2];
        int c = 0;
        for (int i = 0; i < OmniItems.itemTypes.Length; i++)
        {
            giv[c++] = OmniItems.itemTypes[i].id;
            giv[c++] = 1;
        }


        InventoryEvent invevent = new InventoryEvent(OmniWorld.tick + 2 + cl.delay, ev.id, 'a', giv);
        invevent.player = ev.player;
        OmniEvents.AddEvent(invevent);
    }

}
