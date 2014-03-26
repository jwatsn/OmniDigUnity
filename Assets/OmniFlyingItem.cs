using UnityEngine;
using System.Collections;

public class OmniFlyingItem : OmniArmor
{

    public float flySpeed = 20;

    public override void AltFire(ControllableObject player)
    {
        player.movementMask |= 1 << MovementFlags.Floating;
        if ((player.inputMask & (1 << OmniInput.Left)) != 0)
        {
            player.accel.x = -flySpeed;
            if (player.vel.x - player.accel.x * OmniWorld.timeStep < -flySpeed)
                player.accel.x = 0;
        }
        else if ((player.inputMask & (1 << OmniInput.Right)) != 0)
        {
            player.accel.x = flySpeed;
            if (player.vel.x + player.accel.x * OmniWorld.timeStep > flySpeed)
                player.accel.x = 0;
        }
        else
            player.accel.x = 0;
        if ((player.inputMask & (1 << OmniInput.Up)) != 0)
        {
            player.accel.y = flySpeed;
            if (player.vel.y + player.accel.y * OmniWorld.timeStep > flySpeed)
                player.accel.y = 0;
        }
        else if ((player.inputMask & (1 << OmniInput.Down)) != 0)
        {
            player.accel.y = -flySpeed;
            if (player.vel.y - player.accel.y * OmniWorld.timeStep < -flySpeed)
                player.accel.y = 0;
        }
        else
            player.accel.y = 0;
    }
    public override void OnStop(OmniObject player)
    {
        if (player is PhysicsObject)
        {
            int m = (1 << MovementFlags.Floating);
            ((PhysicsObject)player).movementMask &= ~m;

            ((PhysicsObject)player).accel.y = 0;
        }
    }

    public override void Anim(OmniObject player, float time, OmniAnimation.Type type, params Vector3[] mount)
    {

        Vector3 pos = player.position;
        pos.y += player.item.animList[0].mountPoint[0].y * player.item.scale.x;
        if (player.flipped)
        {
            pos.x += player.item.scale.x;
        }


        OmniWorld.instance.particleEmitter.Emit(pos, new Vector3(0, 0, 0), 1, 3, Color.red, (Time.realtimeSinceStartup*1000) % 360, 0);

        
    }

}
