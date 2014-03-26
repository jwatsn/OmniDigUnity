using System;
using System.Collections.Generic;
using UnityEngine;

public class MountEvent : NetworkEvent
{
    public bool mounting;
    public int id,to,mid;
    public MountEvent(int tick)
        : base(tick)
    {
        type = NetworkSendType.ServerCommandAll;

        //this.mounting = mount;
    }

    public override void handle(int tick)
    {
        base.handle(tick);
        if (handled)
        {
            if (Network.isClient)
                if(!fromServer)
            {
                return;
            }

            if (mounting)
            {

                OmniWorld.instance.SpawnedObjectsNew[id].Mount(OmniWorld.instance.SpawnedObjectsNew[to], mid);
            }
            else
            {
                if (Network.isClient)
                    if (id == OmniLocal.LocalID)
                        if (OmniWorld.instance.SpawnedObjectsNew[to] != null)
                        {
                            OmniWorld.instance.SpawnedObjectsNew[to].isGhost = true;
                            OmniWorld.instance.SpawnedObjectsNew[to].ghost.lastRot = OmniWorld.instance.SpawnedObjectsNew[to].rotation;
                            OmniWorld.instance.SpawnedObjectsNew[to].ghost.currentRot = OmniWorld.instance.SpawnedObjectsNew[to].rotation;
                        }

                OmniWorld.instance.SpawnedObjectsNew[id].UnMount();
            }
        }
    }
}