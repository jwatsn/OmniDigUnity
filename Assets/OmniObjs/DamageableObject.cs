using System;
using System.Collections.Generic;
using UnityEngine;

public class DamageableObject : PhysicsObject
{

    public float Stamina;
    public float stun;
    public float stamRecoverRate = 25f;
    public float rotSpeed = 0;
    public float rotRecovery = -1;



    public float mountRot;
    public float mountRotOffset;
    public Vector2 mountPos;
    public Vector2 mountPosFlipped;

    public DamageableObject(int id, int itemId)
        : base(id, itemId)
    {
        HP = item.baseHP;
        Stamina = 100;
    }

    public override void OmniUpdate(float delta)
    {

        if (mountedTo != null)
        {
            vel.x = 0;
            vel.y = 0;
        }
        else
        {
            
            Stamina += stamRecoverRate * delta;
            if (Stamina > 100)
                Stamina = 100;
            stun -= delta;
            if (stun <= 0)
            {

                rotSpeed = 0;
                stun = 0;
            }
            else
            {
                rotation += rotSpeed * delta;
            }

            /*
            if (rotation < 0)
            {
                rotation = 360 + rotation;
            }
            if (rotation > 360)
                rotation = rotation % 360;
            */
            if (isProjectile)
            {
                item.CheckBounds(this);
                mount0time += delta;
            }
        

        
        }
        base.OmniUpdate(delta);
    }

    public override void Mount(OmniObject mountTo, int mid)
    {
        base.Mount(mountTo, mid);
        item.OnMount(this, mountTo);
        mountTo.item.OnMount(mountTo, this);
        mountLoc = mid;
        mountedTo = mountTo;
        mountPos = mountTo.item.animList[0].skeletonRects[mountLoc].mountPoint[0] * mountTo.item.Size;
        mountObject mobj = new mountObject();
        mobj.obj = this;
        mobj.location = mountLoc;
        mobj.offset = mountPos;
        
    }

    public override void UnMount()
    {
        base.UnMount();
        if (mountedTo == null)
            return;
        item.OnDismount(this, mountedTo);
        mountedTo.item.OnDismount(mountedTo, this);
        setPos(mountedTo.bounds.x + mountedTo.item.animList[0].skeletonRects[mountLoc].mountPoint[0].x * mountedTo.item.Size,mountedTo.bounds.y + mountedTo.item.animList[0].skeletonRects[mountLoc].mountPoint[0].y * mountedTo.item.Size);
        vel.x += 10 * Mathf.Cos(mountedTo.item.animList[0].skeletonRects[mountLoc].mountPoint[0].z);
        vel.y += 10 * Mathf.Sin(mountedTo.item.animList[0].skeletonRects[mountLoc].mountPoint[0].z);

        
        mountedTo = null;
        mountLoc = -1;
        
    }

    public virtual void Damage(OmniObject attacker,int dmg)
    {
        HP -= dmg;

        if (HP <= 0)
        {
            if (!isDead)
            {
                Dead(attacker);
                isDead = true;
            }
        }
    }

    public virtual void Dead(OmniObject attacker)
    {
        stateTime = 0;
        stun = 10;
        rotSpeed = 900;

        if (!Respawnable)
        {
            DestroyEvent e = new DestroyEvent(OmniWorld.tick+10);
            e.objid = id;
            stateTime = 0;
            OmniEvents.AddEvent(e);
        }
    }

    public virtual void Respawn()
    {
        Vector2 s = OmniWorld.getSpawnPoint();
        setPos(s.x, s.y);
        HP = item.baseHP;
        rotSpeed = 0;
        rotation = 0;
        isDead = false;
    }
}