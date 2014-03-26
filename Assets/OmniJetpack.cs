using UnityEngine;
using System.Collections;

public class OmniJetpack : OmniArmor {

    ParticleEmitter emitter;
    public int DrainSpeed = 50; // per second

    public override void Init()
    {
        base.Init();


        
    }

    public override void AltFire(ControllableObject player)
    {
        player.Stamina -= DrainSpeed * OmniWorld.timeStep;
        if (player.Stamina < 0)
            player.Stamina = 0;

        if (player.Stamina > 0)
        {
            player.accel.y = 10;
        }
        else
        {
            player.mount1 = -1;
        }
    }

    public override void Anim(OmniObject player,float time,OmniAnimation.Type type, params Vector3[] mount)
    {

        Vector3 pos = player.position;
        pos.y += player.item.animList[0].mountPoint[0].y * player.item.scale.x;
        if (player.flipped)
        {
            pos.x += player.item.scale.x;
        }


        OmniWorld.instance.particleEmitter.Emit(pos, new Vector3(0, -2, 0), 1, 15, Color.red,360 * Random.value,0);
        
       
    }
}
