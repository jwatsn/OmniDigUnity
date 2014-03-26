using System;
using System.Collections.Generic;
using UnityEngine;

public struct clientUpdate
{
    public int tick;
    public float lookAngle;
    public float fireRot;
    public bool flipped;


    public clientUpdate(int tick, float lookAngle, float fireRot,bool flipped)
    {
        this.tick = tick;
        this.lookAngle = lookAngle;
        this.fireRot = fireRot;
        this.flipped = flipped;
    }
}

public class ClientControllable : ControllableObject
{

    public List<clientUpdate> updates;
    public Vector3 posCorrection;
    public Vector3 velCorrection;
    public int delay = 1;
    public string guid;

    public ClientControllable(int id, int itemid)
        : base(id, itemid)
    {
        updates = new List<clientUpdate>();
        posCorrection = Vector3.zero;
        Respawnable = true;
    }


    int getUpdate(int tick)
    {

        for (int i = 0; i < updates.Count-1; i++)
        {
            if (tick >= updates[i].tick && tick < updates[i + 1].tick)
                return i;
        }
        return -1;
    }

    public override void OmniUpdate(float delta)
    {
        /*
        int u = getUpdate(OmniWorld.tick);
        if (u >= 0)
        {
            float a = (float)(OmniWorld.tick - updates[u].tick) / (float)(updates[u + 1].tick - updates[u].tick);
            lookRot = Mathf.LerpAngle(updates[u].lookAngle, updates[u + 1].lookAngle, a);
            fireRot = Mathf.LerpAngle(updates[u].fireRot, updates[u + 1].fireRot, a);
        }
        else
        {
            if (updates.Count > 0)
            {
                lookRot = updates[updates.Count - 1].lookAngle;
                fireRot = updates[updates.Count - 1].fireRot;
            }
        }
         

        while (updates.Count > 15)
        {
            updates.RemoveAt(0);
        }
         * */
        if (Network.isServer)
        {
            for (int i = 0; i < updates.Count; i++)
            {
                if (OmniWorld.tick >= updates[i].tick)
                {
                    lookTo = updates[i].lookAngle;
                    fireRot = updates[i].fireRot;
                    flipped = updates[i].flipped;
                    updates.Remove(updates[i]);

                }
            }
        }

        base.OmniUpdate(delta);
    }

    public void setFixedPos(int tick,float x, float y)
    {


        bounds.x = x;
        bounds.y = y;
        int inputId = OmniLocal.GetInputBuffer(tick - OmniWorld.localDelay);
        int lastMask = inputMask;
        for (int i = 0; i < OmniWorld.localDelay; i++)
        {
            if (inputId >= 0)
            {
                if (inputId + i < OmniLocal.instance.inputBuffer.Count)
                {
                    inputMask = OmniLocal.instance.inputBuffer[inputId + i].inputMask;
                }
            }
            OmniUpdate(OmniWorld.timeStep);
            
                
        }
        inputMask = lastMask;


    }
}