using UnityEngine;
using System.Collections;

public class OmniBow : OmniItemType {


    public bool rapidFire;

    public override void CheckBounds(OmniObject owner)
    {
        base.CheckBounds(owner);
        
    }


    public override void OnUse(ControllableObject cplayer, Vector3 pos)
    {
        base.OnUse(cplayer, pos);

            if(cplayer.selected >= 0 && cplayer.selected < OmniQuickBar.QuickBar_Capacity)
                if (cplayer.bagItems[cplayer.selected + 1] != null)
                {
                    if (cplayer.bagItems[cplayer.selected + 1].stack > 0)
                    {
                        cplayer.mount2 = cplayer.bagItems[cplayer.selected + 1].type.id;
                    }
                }
        


    }

    public override void OnUseTick(OmniObject player, Vector3 pos)
    {
        if (rapidFire)
        {
            if (player.mount2 >= 0)
            {
                player.mount2time = 10;
                Fire(player);
            }
        }
    }

    public void Fire(OmniObject player)
    {
        if (Network.isClient)
            return;

        Vector2 vel = new Vector2(Mathf.Cos(player.fireAngle * Mathf.Deg2Rad) * (player.mount2time * 3) * str, Mathf.Sin(player.fireAngle * Mathf.Deg2Rad) * (player.mount2time * 3) * str);
        if (player.flipped)
            vel.x *= -1;
        Matrix4x4 tr = player.itemMatrix[0];

        Vector3 pos = tr.MultiplyPoint(Vector3.zero);

        SpawnEvent e = new SpawnEvent(OmniWorld.tick, OmniItems.itemTypes[player.mount2].name, pos, vel, -1, true, typeof(DamageableObject));
        OmniEvents.AddEvent(e);


    }

    public override void OnStop(OmniObject player)
    {
        base.OnStop(player);

        if (player.mount2 < 0)
            return;

        if (!rapidFire)
            Fire(player);

        player.mount2 = -1;
        player.mount2time = 0;
    }

}
