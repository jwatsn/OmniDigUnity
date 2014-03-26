using UnityEngine;
using System.Collections;

public class OmniFlyable : OmniMountable
{
    public float turnSpeed = 3;
    public float hoverPower = 3;

    [HideInInspector]
    public int gunLocation;




    public override void OmniUpdate(OmniObject owner, float delta)
    {
        base.OmniUpdate(owner, delta);

        if ((owner.inputMask & (1 << OmniInput.Left)) != 0)
        {
            owner.rotation += turnSpeed;
        }
        else if ((owner.inputMask & (1 << OmniInput.Right)) != 0)
        {
            owner.rotation -= turnSpeed;
        }

        if ((owner.inputMask & (1 << OmniInput.Up)) != 0)
        {
            owner.accel.y = Acceleration;
        }
        else
            owner.accel.y = 0;
        
    }



    public override void OnMount(OmniObject owner, OmniObject mountObj)
    {
        base.OnMount(owner, mountObj);

        owner.movementMask |= 1<<MovementFlags.Flying;

    }

    public override void AltFire(ControllableObject player)
    {
        if (player.item == this)
        {
            player.vel.y += hoverPower * 0.5f;
        }
    }

    public override void OnDamage(OmniObject damaged, OmniObject attacker, OmniItemType itemType, Vector2 vel)
    {

        Vector2 pos = damaged.Position;
        pos.x += damaged.item.Size / 2;
        pos.y += damaged.item.Size + 0.5f;

        int dmg = (int)(itemType.str * (vel.magnitude * 0.3f));



        if (damaged is PhysicsObject)
        {
            PhysicsObject p = (PhysicsObject)damaged;
            p.vel += vel;

            if (damaged == attacker) //fall damage
            {
                dmg = (int)p.vel.magnitude*2;
            }
        }
        OmniInventory.drawDamageGUI(dmg, pos, damaged);
        if (damaged is DamageableObject)
        {
            ((DamageableObject)damaged).Damage(attacker, dmg);
        }


    }

    public override void OnUse(ControllableObject player, Vector3 pos2)
    {

        if (Network.isClient)
            return;

        bool flag = false;

        if(player.mountedTo != null)
        if (player.selected >= 0)
            if (player.bagItems[player.selected] != null)
                if (player.bagItems[player.selected].stack > 0)
                {
                    OmniItemType item = OmniItems.itemTypes[player.bagItems[player.selected].id];

                    Vector2 vel = new Vector2(Mathf.Cos((player.mountedTo.rotation + 90) * Mathf.Deg2Rad) * str, Mathf.Sin((player.mountedTo.rotation + 90) * Mathf.Deg2Rad) * str);
                    if (player.flipped)
                        vel.x *= -1;
                    Matrix4x4 tr = player.mountedTo.item.animList[0].skelMatrix2(player.mountedTo, player.mountedTo.Position, 1);

                    vel.x += player.mountedTo.vel.x;
                    vel.y += player.mountedTo.vel.y;

                    Vector2 pos = tr.MultiplyPoint(new Vector3(0,0,0));
                    pos.x -= item.Size / 2f;
                    pos.y -= item.Size / 2f;
                    pos += player.mountedTo.vel * OmniWorld.timeStep;
                    SpawnEvent e = new SpawnEvent(OmniWorld.tick, OmniItems.itemTypes[player.bagItems[player.selected].id].name, pos, vel, -1, true, typeof(DamageableObject));
                    e.firedBy = player.mountedTo;
                    OmniEvents.AddEvent(e);
                    flag = true;
                }
        if (!flag)
        base.OnUse(player, pos2);
    }


}