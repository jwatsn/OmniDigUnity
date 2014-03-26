using UnityEngine;
using System.Collections;

public class OmniMountable : OmniDeployable
{


    public override void OnClicked(OmniObject owner, OmniObject clicker)
    {
        if ((clicker.movementMask & (1 << MovementFlags.Mounted)) == 0)
        {
            clicker.movementMask |= 1 << MovementFlags.Mounted;
            MountEvent e = new MountEvent(OmniWorld.tick);
            e.mounting = true;
            e.id = clicker.id;
            e.to = owner.id;
            e.mid = 0;
            OmniEvents.AddEvent(e);
        }
        else
        {
            int m = 1 << MovementFlags.Mounted;
            clicker.movementMask = 0;
            MountEvent e = new MountEvent(OmniWorld.tick);
            e.mounting = false;
            e.id = clicker.id;
            e.to = owner.id;
            OmniEvents.AddEvent(e);
        }
        base.OnClicked(owner, clicker);
    }

}