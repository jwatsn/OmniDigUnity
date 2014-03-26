using System;
using System.Collections.Generic;
using UnityEngine;

public class DestroyEvent : NetworkEvent
{
    public int objid = -1;

    public DestroyEvent(int tick)
    :base(tick)
    {
        type = NetworkSendType.ServerCommandAll;

        
    }

    public override void handle(int tick)
    {
        base.handle(tick);

        if (handled)
        {
            if(objid >= 0)
                if(objid < OmniWorld.instance.SpawnedObjectsNew.Count)
                    if(OmniWorld.instance.SpawnedObjectsNew[objid] != null)
                        OmniWorld.instance.SpawnedObjectsNew[objid].destroy(true);
        }
    }
}
