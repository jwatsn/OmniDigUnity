using System;
using System.Collections.Generic;
using UnityEngine;

public class VehicleObject : ClientControllable
{

    public VehicleObject(int id,int itemId)
        : base(id, itemId)
    {
        
    }

    public override void Dead(OmniObject attacker)
    {
        base.Dead(attacker);

        for (int i = 0; i < mountObjects.Count; i++)
        {
            MountEvent e = new MountEvent(OmniWorld.tick + 5);
                e.mounting = false;
            e.id = mountObjects[i].obj.id;
            e.to = this.id;
            OmniEvents.AddEvent(e);
        }
    }

}
