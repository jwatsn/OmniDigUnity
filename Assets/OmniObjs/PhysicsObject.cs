using System;
using System.Collections.Generic;
using UnityEngine;

public class MovementFlags
{
    public const int Floating = 1;
    public const int Flying = 2;
    public const int Mounted = 3;
}

public class PhysicsObject : OmniObject
{

    public class cObj
    {
        public Rect b;
        public Vector2 v;

        public cObj(Rect b, Vector2 v)
        {
            this.b = b;
            this.v = v;
        }
    }

    public Vector2 lerpTo;

    public Vector2 lerpFrom;
    public float lerpCounter;

    public bool grounded;

    public bool physicsPaused;
    public float MaxSpeed;
    Rect boundsTest;
    float projTimer;



    public List<Rect> r;
    public List<OmniObject> r2;

    public PhysicsObject(int id, int itemId)
        :base(id,itemId)
    {
        
        r = new List<Rect>();
        r2 = new List<OmniObject>();
        lerpTo = new Vector2();
        lerpFrom = new Vector2();
        


        
    }

    public override void OmniUpdate(float delta)
    {

        if (!Network.isClient)
        {
            if (projTimer >= 30)
            {
                DestroyEvent e = new DestroyEvent(OmniWorld.tick);
                e.objid = id;
                return;
            }
        }

        if (isProjectile)
        {
            animType = OmniAnimation.Type.Skeleton;
            projTimer += delta;
        }
        else
        {
            animType = item.animList[0].animationType;
        }

        if (spawned)
        {


                Move(delta);


            /*
                if ((movementMask &( 1 << MovementFlags.Mounted)) != 0)
                {
                    if (mountedTo.updateTick == OmniWorld.tick)
                    {
                        bounds.x = mountedTo.bounds.x;
                        bounds.y = mountedTo.bounds.y;
                        lastPos.x = mountedTo.lastPos.x;
                        lastPos.y = mountedTo.lastPos.y;
                        updateMask |= 1 << ObjectNetworkUpdate.Position;
                    }
                }
            */





                if (lastPos.x != bounds.x || lastPos.y != bounds.y)
                    updateMask |= 1 << ObjectNetworkUpdate.Position;



            


            lerpFrom.x = lerpTo.x;
            lerpFrom.y = lerpTo.y;

            lerpTo.x = bounds.x;
            lerpTo.y = bounds.y;
            lerpCounter = 0;

            if (isProjectile)
                if (!grounded)
                    rotation = (Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg) + item.animList[0].RotationOffset - 90;
        }
        base.OmniUpdate(delta);
    }

    public override void destroy(bool trueremove)
    {
        base.destroy(trueremove);
    }

    public override void Spawned()
    {
        base.Spawned();
    }


    public void MoveOld(float delta)
    {
        lastPos.x = bounds.x;
        lastPos.y = bounds.y;
        bounds.x += vel.x * delta;
        
        collisionTest();
        for (int i = 0; i < r.Count; i++)
            if (bounds.Overlaps(r[i]))
            {
                collidedWithTerrain(r[i],true,false);
            }
        bounds.x = lastPos.x;


        bounds.y += vel.y * delta;
        collisionTest();
        for (int i = 0; i < r.Count; i++)
            if (bounds.Overlaps(r[i]))
            {
                collidedWithTerrain(r[i], false, true);
            }

        
        bounds.y = lastPos.y;
    }

    public void Move(float delta)
    {

        if (!isGhost)
        {



            if ((movementMask&(1<<MovementFlags.Floating)) == 0)
            {
                float lastAccelX = accel.x;
                if (vel.x > 0)
                    if (accel.x > 0)
                        if (accel.x * delta + vel.x > MaxSpeed)
                            accel.x = 0;
                if (vel.x < 0)
                    if (accel.x < 0)
                        if (accel.x * delta + vel.x < -MaxSpeed)
                            accel.x = 0;

                if (grounded)
                {
                    if (lastAccelX == 0)
                    {
                        if (vel.x > 0)
                        {
                            vel.x -= item.Friction * delta;
                            if (vel.x < 0)
                                vel.x = 0;
                        }
                        else if (vel.x < 0)
                        {
                            vel.x += item.Friction * delta;
                            if (vel.x > 0)
                                vel.x = 0;

                        }
                    }
                }

            }

            if ((movementMask & (1 << MovementFlags.Flying)) != 0)
            {


                float vy = (Mathf.Sin((rotation + 90) * Mathf.Deg2Rad) * accel.y) * delta * item.Size;
                float vx = (Mathf.Cos((rotation + 90) * Mathf.Deg2Rad) * accel.y) * delta * item.Size;


                vel.y += vy;
                vel.x += vx;

                if (vel.y > MaxSpeed)
                    vel.y = MaxSpeed;
                if (vel.x > MaxSpeed)
                    vel.x = MaxSpeed;

                if (vel.y < -MaxSpeed)
                    vel.y = -MaxSpeed;
                if (vel.x < -MaxSpeed)
                    vel.x = -MaxSpeed;

               

                if(accel.y == 0)
                vel.y -= item.Gravity * delta * item.Size;
            }
            else
            {
                vel += accel * delta * item.Size;
                if ((movementMask & (1 << MovementFlags.Floating)) == 0)
                    vel.y -= item.Gravity * delta * item.Size;
            }

                










        lastPos.x = bounds.x;

        int velX = (int)(vel.x * delta * 1000); // limiting precision.. lets see how this goes

        bounds.x += vel.x * delta;



        collisionTest();
        for (int i = 0; i < r.Count; i++)
            if (bounds.Overlaps(r[i]))
            {
                
                if (isProjectile && numBounds > 0)
                {
                    for (int x = 0; x < numBounds; x++)
                    {
                        if (itemBounds[x].Overlaps(r[i]))
                        {
                            bounds.x = lastPos.x;
                            vel.x = 0;
                            
                        }
                    }
                }
                else
                {
                 
                    collidedWithTerrain(r[i], true, false);
                    
                    //break;
                }
            }


        lastPos.y = bounds.y;
        int velY = (int)(vel.y * delta * 1000); // limiting precision.. lets see how this goes
        bounds.y += vel.y * delta;



        collisionTest();
        for (int i = 0; i < r.Count; i++)
            if (bounds.Overlaps(r[i]))
            {
                
                if (isProjectile && numBounds > 0)
                {
                    for (int x = 0; x < numBounds; x++)
                    {
                        if (itemBounds[x].Overlaps(r[i]))
                        {
                            bounds.y = lastPos.y;
                            vel.y = 0;
                            grounded = true;
                            //break;
                        }
                    }
                }
                else
                {
                 
                    collidedWithTerrain(r[i], false, true);
                    //break;
                }
            }





        if (vel.x < 0.01f && vel.x > -0.01f)
            vel.x = 0;
        if (vel.y != 0 && mountedTo == null)
        {
            grounded = false;
        }
        else
            grounded = true;
        }

        if (bounds.x <= 0)
        {
            lastPos.x = (OmniTerrain.Width * OmniTerrain.chunkSize) + lastPos.x;
            bounds.x = (OmniTerrain.Width * OmniTerrain.chunkSize) + bounds.x;

        }
        else if (bounds.x >= OmniTerrain.Width * OmniTerrain.chunkSize)
        {
            lastPos.x = -(OmniTerrain.Width * OmniTerrain.chunkSize-lastPos.x);
            bounds.x = bounds.x - (OmniTerrain.Width * OmniTerrain.chunkSize);

        }

        if (bounds.y <= 0)
        {
            lastPos.y = (OmniTerrain.Height * OmniTerrain.chunkSize) + lastPos.y;
            bounds.y = (OmniTerrain.Height * OmniTerrain.chunkSize) + bounds.y;

        }
        else if (bounds.y >= OmniTerrain.Height * OmniTerrain.chunkSize)
        {
            lastPos.y = lastPos.y - (OmniTerrain.Height * OmniTerrain.chunkSize);
            bounds.y = bounds.y - (OmniTerrain.Height * OmniTerrain.chunkSize);

        }

    }

    public virtual void collidedWithObject(Rect rect, Vector2 v)
    {
        float bMidWidth = bounds.x + bounds.width / 2;
        float rMidWidth = rect.x + rect.width / 2;
        float bMidHeight = bounds.y + bounds.height / 2;
        float rMidHeight = rect.y + rect.height / 2;
        grounded = false;
        if (bMidWidth - rMidWidth >= 0)
        {
            float a = (rect.x + rect.width) - bounds.x;
            vel.x += a;
            
        }
        else
        {
            float a = (bounds.x + bounds.width) - rect.x;
            vel.x = a;
        }

        if (bMidHeight - rMidHeight >= 0)
        {
            float a = (rect.y + rect.height) - bounds.y;
            vel.y = a;
        }
        else
        {
            float a = (bounds.y + bounds.height) - rect.y;
            vel.y = a;
        }
    }

    public virtual void collidedWithTerrainOld(Rect rect, bool x, bool y)
    {
        float bMidWidth = bounds.x + bounds.width / 2;
        float bWidth = bounds.x + (bounds.width);
        float rMidWidth = rect.x + rect.width / 2;
        float bMidHeight = bounds.y + bounds.height / 2;
        float rMidHeight = rect.y + rect.height / 2;



        if (x)
        {
            if (OmniTerrain.getSlant(rect.x, rect.y) == 1)
            {
                vel.x += -vel.x * Mathf.Cos(45);
            }
            else
            vel.x += -vel.x;
        }
        if (y)
        {
            if (OmniTerrain.getSlant(rect.x, rect.y) == 1)
            {
                vel.y += -vel.y * Mathf.Sin(225);
            }
            else
            vel.y += -vel.y;
        }
    }

    public virtual void collidedWithTerrain(Rect rect, bool x, bool y)
    {

        float bMidWidth = bounds.x + bounds.width / 2;
        float bWidth = bounds.x + (bounds.width);
        float rMidWidth = rect.x + rect.width / 2;
        float bMidHeight = bounds.y + bounds.height / 2;
        float rMidHeight = rect.y + rect.height / 2;
        bool flag = false;
        if (x)
        {

            if (Mathf.Abs(vel.x) > 50)
            {
                Vector3 dmgvel = vel;
                dmgvel *= -0.5f;

                DamageEvent e = new DamageEvent(OmniWorld.tick, this, this, item, dmgvel);

                OmniEvents.AddEvent(e);
            }

            if (bMidHeight - rMidHeight >= 0)
            {
                if (Mathf.Abs(bounds.y - (rect.y + rect.height)) < 0.5f)
                {
                    flag = true;
                }
            }
            else
            {
                if (Mathf.Abs((bounds.y+bounds.height) - (rect.y)) < 0.5f)
                {
                    flag = true;
                }
            }
            if (flag)
                return;

                if (bMidWidth - rMidWidth >= 0)
                {
                    bounds.x = rect.x + rect.width + OmniWorld.timeStep * 0.5f;
                    vel.x = 0;
                }
                else
                {

                    vel.x = 0;
                    bounds.x = rect.x - bounds.width - OmniWorld.timeStep * 0.5f;

                }           
        }
        else if (y)
        {
            if (Mathf.Abs(vel.y) > 50)
            {
                Vector3 dmgvel = vel;
                dmgvel *= -0.5f;

                DamageEvent e = new DamageEvent(OmniWorld.tick, this, this, item, dmgvel);
                
                OmniEvents.AddEvent(e);
            }


                if (bMidHeight - rMidHeight >= 0)
                {


                    bounds.y = rect.y + rect.height + OmniWorld.timeStep * 0.5f;
                    vel.y = 0;
                    grounded = true;

                }
                else
                {
                    vel.y = -OmniWorld.timeStep;
                    bounds.y = rect.y - bounds.height - OmniWorld.timeStep * 0.5f;
                }

            }
        
        float moved = bounds.y - (rect.y + rect.height);
      
        
        /*
        if (x)
        {
            float bMid = bounds.x + bounds.width / 2;
            float rMid = rect.x + rect.width / 2;

            if (bMid - rMid >= 0)
                bounds.x = rect.x + rect.width + 0.001f;
            else
                bounds.x = rect.x - bounds.width - 0.001f;
            

            vel.x = 0;
        }
        else if (y)
        {
            if (!grounded)
            {
                if (vel.y < 0)
                    grounded = true;
            }
            float bMid = bounds.y + bounds.height / 2;
            float rMid = rect.y + rect.height / 2;

            if (bMid - rMid < 0)
            {
                bounds.y = rect.y - bounds.height - 0.001f;
                vel.y = -0.01f;
            }
            else
            {
                bounds.y = rect.y + rect.height + 0.001f;
                vel.y = 0;
            }

        }
        */
    }

    public override void draw(int mirrorx,int mirrory,bool force=false)
    {
        
        
        base.draw(mirrorx, mirrory,force);
        OmniInterpolate.InterpolatePos(this);
        /*
        if (isProjectile)
        {
            for (int i = 0; i < numBounds; i++)
            {
                Graphics.DrawMesh(OmniItems.itemTypes[7].animList[0].meshs[0], itemMatrix[i], OmniAtlas.texture, 0);
            }
        }
         * */
    }

    void objcollisionTest()
    {
        r2.Clear();
        for (int i = 0; i < activeChunks.Count; i++)
        {
            for (int j = 0; j < activeChunks[i].objects.Count; j++)
            {
                
                if (activeChunks[i].objects[j].mount0 >= 0)
                {
                    if(!r2.Contains(activeChunks[i].objects[j]))
                        r2.Add(activeChunks[i].objects[j]);
                }
                    
            }
        }

    }

    void old()
    {
        /*
        if (OmniTerrain.getSlant(bWidth, bounds.y) == 1)
        {
            float yy = (rect.y) + (bWidth - rect.x);
            bounds.y = yy;
            vel.y = 0;
            grounded = true;
        }
        else
        {
        }
         * */
    }

    void collisionTest()
    {
        
        r.Clear();
        int c1 = (int)(bounds.x + bounds.width);
        int c2 = (int)(bounds.y + bounds.height);
        int c3 = c1 - ((int)bounds.x);
        int c4 = c2 - ((int)bounds.y);

        //Debug.Log(c3 + " " + c4);

        //int c1 = (int)((bounds.x + bounds.width) - bounds.x);
        
        //int c2 = (int)((bounds.y + bounds.height) - bounds.y);
        for (int x = 0; x <= c3; x++)
            for (int y = 0; y <= c4; y++)
            {
                int c1x = (int)(bounds.x + x);
                int c1y = (int)(bounds.y + y);

                if ((bounds.x + x) < 0)
                {
                    if (c1x == 0)
                        c1x = -1;
                    else
                        c1x = c1x - 1;
                }
                if ((bounds.y + y) < 0)
                {
                    if (c1y == 0)
                        c1y = -1;
                    else
                        c1y = c1y - 1;
                }


                int id = x + y * (int)item.Size;
                if (OmniTerrain.isSolid(c1x, c1y))
                {
                    Rect rr = new Rect(c1x, c1y, 1, 1);
                    if (OmniTerrain.getSlant(c1x, c1y) == 1)
                    {
                        if (c1x == c1)
                        {
                            float a = (bounds.x + bounds.width) - c1x;
                            rr.height = a;
                        }
                        
                    }
                    else if (OmniTerrain.getSlant(c1x, c1y) == 2)
                    {
                        if (x == 0)
                        {
                            float a = (bounds.x) - c1x;
                            a = 1 - a;
                            if (a < 0)
                                a = 0;
                            rr.height = a;
                            
                        }
                        
                    }
                    else if (OmniTerrain.getSlant(c1x, c1y) == 3)
                    {
                        if (y == c4 && x == c3)
                        {
                            float a = (bounds.x + bounds.width) - c1x;
                            float b = (c1y + 1) - a;
                            rr.y = b;
                            rr.height = a;
                            
                        }
                        
                    }
                    else if (OmniTerrain.getSlant(c1x, c1y) == 4)
                    {
                        if (y == c4 && x == 0)
                        {
                            float a = (bounds.x) - c1x;
                            a = 1 - a;
                            if (a < 0)
                                a = 0;

                            float b = (c1y + 1) - a;
                            rr.y = b;
                            rr.height = a;

                        }

                    }
                    r.Add(rr);
                }
            }



    }
}
