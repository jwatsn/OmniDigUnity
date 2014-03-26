using UnityEngine;

using System.Collections;

public class OmniHuman : OmniItemType
{

    public int LeftLeg, RightLeg, RightArm,LeftArm,Head;

    public override void Init()
    {
        base.Init();

        
    }

    public override void OnUse(ControllableObject player, Vector3 pos)
    {
        if (!Network.isClient)
        {
            SpawnEvent e = new SpawnEvent(OmniWorld.tick, gameObject.name,gameObject.name, pos,Vector2.zero, -1, typeof(ControllableObject));
            OmniEvents.AddEvent(e);
        }
    }

    public override void Spawned(OmniObject player)
    {
        base.Spawned(player);
        for (int i = 0; i < player.skeleton.Length; i++)
        {

            if (player.skeleton[i].name == "leftleg")
                LeftLeg = i;
            if (player.skeleton[i].name == "rightleg")
                RightLeg = i;
            if (player.skeleton[i].name == "rightarm")
                RightArm = i;
            if (player.skeleton[i].name == "leftarm")
                LeftArm = i;
            if (player.skeleton[i].name == "head")
                Head = i;
        }
        
    }

    public override void OnFlip(OmniObject owner)
    {
        base.OnFlip(owner);
        if (owner.flipped)
            owner.heldId = getSkelLoc(owner, "leftarm");
        else
            owner.heldId = getSkelLoc(owner, "rightarm");
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
            p.vel += vel*0.7f;

            if (damaged == attacker) //fall damage
            {
                dmg = (int)p.vel.magnitude;
            }
        }
        OmniInventory.drawDamageGUI(dmg, pos, damaged);
        if (damaged is DamageableObject)
        {
            ((DamageableObject)damaged).Damage(attacker, dmg);
        }


    }

    public int getQuadrant(float angle)
    {
        if (angle >= 0 && angle < 90)
            return 1;
        else if (angle >= 90 && angle < 180)
            return 2;
        else if (angle >= 180 && angle < 270)
            return 3;
        else if (angle >= 270 && angle < 360)
            return 4;
        else
            return -1;
    }

    public override void OmniUpdate(OmniObject owner, float delta)
    {
        ControllableObject o = owner as ControllableObject;
        
        

        if (o == null)
            return;

        if (o.grounded)
        {
            o.rotSpeed = 0;
        }

        if (o.item.animList[0].animationType != OmniAnimation.Type.Skeleton)
            return;



        float vel = 0;
        o.lookRot = Mathf.SmoothDampAngle(o.lookRot, o.lookTo,ref vel, 0.1f, Mathf.Infinity, delta);



        owner.skeleton[Head].rotation = o.lookRot;



            if (o.grounded)
            {

                if (o.vel.x != 0)
                {
                    owner.skeleton[LeftLeg].rotation -= delta * 40 * o.vel.x;
                    owner.skeleton[RightLeg].rotation -= delta * 40 * o.vel.x;
                }

                owner.skeleton[LeftArm].rotation = 0;
                owner.skeleton[RightArm].rotation = 0;

                if (o.stun > 0.3f)
                    o.stun = 0.3f;

               

                if (o.rotation != 0)
                {
                    if (o.inputMask != 0)
                        o.rotation = 0;
                }

            }
        else
        {
            owner.skeleton[LeftArm].rotation = 180;
            owner.skeleton[RightArm].rotation = 180;
        }

            if (o.mount0 >= 0)
            {
                
                
                o.fireAngle = Mathf.SmoothDampAngle(o.fireAngle, o.fireRot, ref o.fireVel, 1f / OmniItems.itemTypes[o.mount0].swingSpeed, Mathf.Infinity, delta);
                /*
                o.fireAngle += o.fireRot * delta * OmniItems.itemTypes[o.mount0].SwingSpeed;
                o.fireAngle = fixangle(o.fireAngle);
                */
                if (!owner.flipped)
                {

                   owner.skeleton[RightArm].rotation = o.fireAngle + 90;

                }
                else
                {
                    owner.skeleton[LeftArm].rotation = o.fireAngle + 90;
                }
            }

        base.OmniUpdate(owner, delta);
    }

    public static float fixangle(float rot, float max)
    {

        if (rot > max)
            rot = max;
        if (rot < -max)
            rot = -max;

        /*
        if (rot > 360 || rot < -360)
            rot = rot % 360;
        if (rot < 0)
            rot = 360 + rot;


        if (rot > max && rot < 180)
            rot = max;

        if (rot > 180 && rot < (360 - max))
            rot = 360 - max;

        */
        return rot;

    }

    public void OmniUpdateOLd(OmniObject player, float delta)
    {
        //base.OmniUpdate(player, delta);

        if (player is ControllableObject)
        {
            ControllableObject c = (ControllableObject)player;
            if (!c.grounded)
                c.animName = "falling";
            else if (c.vel.x != 0)
                c.animName = "walk";
            else
                c.animName = "idle";

            if (c.mount0 >= 0)
            {
                if (!c.grounded)
                    c.animName = "fallingattack";
                else if (c.accel.x != 0)
                    c.animName = "walkattack";
                else
                    c.animName = "idleattack";
            }

            if (c.grounded)
            {
                if (c.stun > 0.3f)
                    c.stun = 0.3f;
                c.rotSpeed = 0;
                if (c.rotation != 0)
                {
                    if (c.inputMask != 0)
                        c.rotation = 0;
                }
            }
        }
    }
}
