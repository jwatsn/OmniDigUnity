using UnityEngine;
using System.Collections;

public class OmniDamageable : OmniItemType
{


    public override void OnHit(OmniObject player, OmniObject hitPlayer, Vector2 vel,Vector2 pos,int location)
    {
        if (hitPlayer is DamageableObject)
        {
            DamageableObject dmged = hitPlayer as DamageableObject;
            if (dmged.stun > 0)
                return;
            else
            {
                dmged.stun = stunTime;
            }
        }





        if (vel.magnitude < 0.7f)
            return;

        if (location >= 0)
        {
            if (player is DamageableObject)
            {

                if (((DamageableObject)player).isProjectile)
                {
                    DamageableObject pl = ((DamageableObject)player);




                    Vector2 b1 = pos;
                    Vector2 b2 = hitPlayer.boundsPos + new Vector2(hitPlayer.skeleton[location].bounds.x * hitPlayer.item.Size, hitPlayer.skeleton[location].bounds.y * hitPlayer.item.Size);

                    //b1.x += player.item.Size / 2f;
                    //b2.x += hitPlayer.item.Size / 2f;
                    //b1.y += player.item.Size / 2f;
                    //b2.y += hitPlayer.item.Size / 2f;
                    Vector2 offset = pos;
                    /*
                
                    Quaternion rot = Quaternion.AngleAxis(hitPlayer.skeleton[location].rotation, Vector3.forward);
                    Matrix4x4 tr = hitPlayer.skeleton[location].tr;
                    Matrix4x4 skelmove = Matrix4x4.TRS(hitPlayer.skeleton[location].rotationPoint, Quaternion.identity, Vector3.one);
                    Matrix4x4 skelrot = Matrix4x4.TRS(Vector3.zero, rot, Vector3.one);
                    tr *= skelmove;
                    tr *= skelrot;
                    tr *= skelmove.inverse;
                    offset = tr.MultiplyPoint(offset);
                    */
                    //offset /= hitPlayer.item.Size;
                    pl.mountedTo = hitPlayer;
                    pl.mountPos = offset;
                    pl.mountPosFlipped = offset;
                    pl.mountPosFlipped.x = pl.item.Size-offset.x;
                    pl.mountRot = player.rotation - hitPlayer.skeleton[location].rotation;
                    //Debug.Log(pl.mountRot);
                    //float a = ;
                    if (hitPlayer.skeleton[location].rotation >= 0)
                    {
                        //pl.mountPos.x += 1 * (hitPlayer.item.Size / player.item.Size);
                    }
                    //pl.mountPos.x -= Mathf.Cos(a);
                    //pl.mountPos.y -= Mathf.Sin(a);
                    //pl.mountRotOffset = player.rotation - hitPlayer.skeleton[location].rotation;
                    pl.mountLoc = location;
                    mountObject o = new mountObject();
                    o.obj = player;
                    o.location = location;
                    hitPlayer.mountObjects.Add(o);
                }
            }
        }
        DamageEvent e = new DamageEvent(OmniWorld.tick, hitPlayer, player, this, vel);
        OmniEvents.AddEvent(e);





    }
}
