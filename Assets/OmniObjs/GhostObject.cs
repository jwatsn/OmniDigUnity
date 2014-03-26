using System;
using System.Collections.Generic;
using UnityEngine;

public class GhostObject
{

    public class ghost_update
    {
        public Vector2 pos = new Vector2();
        public int tick;

        public ghost_update(float x, float y, int tick)
        {
            this.pos.x = x;
            this.pos.y = y;
            this.tick = tick;
        }
    }


    public float interpCounter = 0;

    public float mount0ghostTime = 0;
    public float mount1ghostTime = 0;
    public float mount2ghostTime = 0;

    public float mount0interp = 0;
    public float mount1interp = 0;
    public float mount2interp = 0;

    public float mount0ghostLast = 0;
    public float mount1ghostLast = 0;
    public float mount2ghostLast = 0;


    public float lastRot = 0;
    public float currentRot = 0;
    public float rotCounter = 0;

    public OmniObject owner;

    public List<ghost_update> positionUpdates = new List<ghost_update>();

    public float[] skelInterp = new float[OmniAnimation.Max_Skeletons];
    public float[] skelLast = new float[OmniAnimation.Max_Skeletons];
    public int skelMask;
    public float[] skelCounter = new float[OmniAnimation.Max_Skeletons];
    public List<ghost_update> ghostUpdates;
    public Vector2 ghostPos;
    public Vector2 lastGhostPos;
    public int ghostTick = -1;
    public float lastGhostTick = -1;




    public void OmniUpdate(float delta)
    {
        
        interpCounter += delta;
        mount0interp += delta;
        mount1interp += delta;
        mount2interp += delta;
        rotCounter += delta;

        for (int i = 0; i < skelCounter.Length; i++)
            skelCounter[i] += delta;

        ghostLerp();
        OmniGhost.InterpolatePos(this);
    }
    
    public void ghostLerp()
    {

        float a = interpCounter / OmniWorld.networkRate;
        if (a > 1)
            a = 1;

        int chX = (int)(lastGhostPos.x / OmniTerrain.chunkSize);
        int chX2 = (int)(ghostPos.x / OmniTerrain.chunkSize);

        int chY = (int)(lastGhostPos.y / OmniTerrain.chunkSize);
        int chY2 = (int)(ghostPos.y / OmniTerrain.chunkSize);

        if (chX == 0 && chX2 == OmniTerrain.Width - 1) // lerp over the boundary
        {
            lastGhostPos.x = OmniTerrain.Width * OmniTerrain.chunkSize + lastGhostPos.x;
           
        }

        else if (chX == OmniTerrain.Width - 1 && chX2 == 0) // lerp over the boundary
        {            
            lastGhostPos.x = -(OmniTerrain.Width * OmniTerrain.chunkSize - lastGhostPos.x);
        }

        if (chY == 0 && chY2 == OmniTerrain.Height - 1) // lerp over the boundary
        {
            lastGhostPos.y = (OmniTerrain.Height * OmniTerrain.chunkSize) + lastGhostPos.y;
        }

        else if (chY == OmniTerrain.Height - 1 && chY2 == 0) // lerp over the boundary
        {
            lastGhostPos.y = lastGhostPos.y - (OmniTerrain.Height * OmniTerrain.chunkSize);
        }

        Vector3 lerp = Vector2.Lerp(lastGhostPos, ghostPos, a);

        owner.lastPos.x = owner.bounds.x;
        owner.bounds.x = lerp.x;

        owner.lastPos.y = owner.bounds.y;
        owner.bounds.y = lerp.y;


    }
    
    public void draw()
    {



        if (Network.isClient && owner.id != OmniLocal.LocalID)
        {



            /*
            if (lastGhostTick != Time.realtimeSinceStartup)
            {
                
                interpCounter += Time.deltaTime;
                mount0interp += Time.deltaTime;
                mount1interp += Time.deltaTime;
                mount2interp += Time.deltaTime;
                rotCounter += Time.deltaTime;
                //Position.x = bounds.x;
                //Position.y = bounds.y;



                

                lastGhostTick = Time.realtimeSinceStartup;
            }
            
            */
        }
        
    }
}
