using UnityEngine;
using System.Collections;

public class OmniDeployable : OmniItemType {


    public override void OnUse(ControllableObject player, Vector3 pos)
    {

        SpawnEvent e = new SpawnEvent(OmniWorld.tick, this.name, player.Name + "'s " + this.name, pos, Vector2.zero, -1, typeof(VehicleObject));
        OmniEvents.AddEvent(e);
        base.OnUse(player, pos);
    }

}
