using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileObject : PhysicsObject
{

    public ProjectileObject(int id, int itemId)
        : base(id, itemId)
    {
        
    }

    public override void OmniUpdate(float delta)
    {
        base.OmniUpdate(delta);
        
        if(!grounded)
        rotation = (Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg) + item.animList[0].RotationOffset - 90;
    }
}

