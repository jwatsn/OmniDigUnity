using UnityEngine;
using System.Collections;

public class OmniPickaxe : OmniDamageable
{

    float mine_step = 0.1f;
    public int mineSpeed = 1;
    public float maxDistance = 6;


    public override void OnUse(ControllableObject player, Vector3 pos)
    {
			
    }

    public override void OnHit(OmniObject player, OmniObject hitPlayer,Vector2 vel,Vector2 pos, int location)
    {
        
        //DamageEvent e = new DamageEvent(OmniWorld.tick, hitPlayer, player, this);
        //OmniEvents.AddEvent(e);
         
        
    }

    public override void OnTerrainCollision(OmniObject player, int x, int y)
    {

        if (OmniWorld.isDebugging)
            Debug.Log("OnTerrainCollision called by " + player.Name);

        OmniTerrain.terrainUpdates.Add(new terrainDmg(player, this,x,y));
        /*
        BlockEvent e = new BlockEvent(OmniWorld.tick, x, y, BlockEventType.Damaged, mineSpeed);
        OmniEvents.AddEvent(e);
         * */
    }
}
