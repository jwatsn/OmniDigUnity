using System;
using System.Collections.Generic;
using UnityEngine;

public class OmniInput
{
    public const int Left = 1;
    public const int Right = 2;
    public const int Up = 3;
    public const int Down = 4;
    public const int Clicked = 5;
    public const int RightClicked = 6;
}

public class ControllableObject : ContainerObject
{
    //Look angle linear interpolation (for the server(rightnow()))
    
    public float lookTo = -1;
    public float lookFrom = -1;
    public float looklerp = 0;
    public float looklerpSpeed;
    //public float angleSpeed = -1;
    public float lastLookCheck = -1;
    public bool lookChanged
    {
        get
        {
            if (lookRot != lastLookCheck)
            {
                lastLookCheck = lookRot;
                return true;
            }
            return false;
        }
    }


    float ClickDelay;
    float m0collision = 0;
    public Vector2 lookPos;
    public Vector2 clickPos;
    
    public float fireRot;

    public float fireVel;



    public ControllableObject(int id, int itemId)
        : base(id, itemId)
    {
            MaxSpeed = item.MaxSpeed;
        
        lookPos = new Vector2();


    }

    public override void OmniUpdate(float delta)
    {


            if (isDead)
            {
                if (Respawnable)
                {
                    if (inputMask != 0)
                        Respawn();
                }
                inputMask = 0;

            }


            if ((movementMask & (1 << MovementFlags.Floating)) == 0 && (movementMask & (1 << MovementFlags.Flying)) == 0)
            {
                if ((inputMask & (1 << OmniInput.Left)) != 0)
                {
                    accel.x = -item.Acceleration;
                }
                else if ((inputMask & (1 << OmniInput.Right)) != 0)
                {
                    accel.x = item.Acceleration;
                }
                else
                    accel.x = 0;

                if ((inputMask & (1 << OmniInput.Up)) != 0)
                {
                    if (grounded)
                    {
                        vel.y = item.JumpStrength;
                        rotation = 0;
                    }
                }
            }

            if ((inputMask & (1 << OmniInput.Clicked)) != 0)
            {
                bool flag = false;
                int chX = (int)(clickPos.x / OmniTerrain.chunkSize);
                int chY = (int)(clickPos.y / OmniTerrain.chunkSize);
                if (OmniTerrain.chunks.ContainsKey(chX + chY * OmniTerrain.Width))
                {
                    TerrainChunk ch = OmniTerrain.chunks[chX + chY * OmniTerrain.Width];
                    for (int i = 0; i < ch.objects.Count; i++)
                    {
                        if (ch.objects[i].bounds.Contains(clickPos))
                        {
                            if ((lastInputMask & (1 << OmniInput.Clicked)) == 0)
                            {
                                if(!ch.objects[i].isDead)
                                ch.objects[i].item.OnClicked(ch.objects[i], this);
                                

                            }
                            if (ch.objects[i].item.eatsMouse)
                                flag = true;
                        }
                    }
                }

                if (mountedTo != null)
                    if (mountedTo.mountObjects[0].obj.id == id)
                    {
                        flag = true;
                        if ((lastInputMask & (1 << OmniInput.Clicked)) == 0)
                        {
                            mountedTo.item.OnUse(this, Position);
                        }
                    }

                if (!flag)
                    if (selected >= 0)
                        if (bagItems[selected] != null)
                            if (bagItems[selected].type.Usable)
                            {
                                if (mount0time >= bagItems[selected].type.delay - 0.06f || mount0 < 0)
                                    if (bagItems[selected].stack > 0)
                                    {


                                        if (mount0 < 0)
                                            bagItems[selected].type.OnUse(this, clickPos);
                                        else
                                            bagItems[selected].type.OnUseTick(this, clickPos);
                                        mount0time = 0;
                                        mount0 = bagItems[selected].id;


                                    }

                            }
                lastInputMask |= 1 << OmniInput.Clicked;

            }
            else
            {
                int m = 1 << OmniInput.Clicked;
                lastInputMask &= ~m;
            }
            if ((inputMask & (1 << OmniInput.RightClicked)) != 0)
            {

                item.AltFire(this);

                for (int i = 0; i < equiptItems.Length; i++)
                {
                    if (equiptItems[i] == null)
                        continue;
                    if (equiptItems[i].stack <= 0)
                        continue;
                    mount1 = equiptItems[i].type.id;
                    break;
                    //equiptItems[i].type.AltFire(this);


                }

                if (selected >= 0)
                    if (bagItems[selected] != null)
                        if (mount0time >= bagItems[selected].type.delay - 0.06f || mount0time == 0)

                            if (bagItems[selected].type.Usable)
                                if (bagItems[selected].stack > 0)
                                {
                                    //bagItems[selected].type.AltFire(this);
                                }
            }
            else
            {
                if (mount1 >= 0)
                    OmniItems.itemTypes[mount1].OnStop(this);

                mount1 = -1;
            }
        


        if (mount1 >= 0)
            OmniItems.itemTypes[mount1].AltFire(this);





        ClickDelay -= delta;
        if (ClickDelay < 0)
            ClickDelay = 0;



        base.OmniUpdate(delta);

    }

    /*
    public void lookAt(float angle)
    {
        lookFrom = lookTo;
        if (lookFrom < 0)
            lookFrom = angle;

        lookTo = angle;
        looklerp = 0;
    }
    */

    public override void draw(int mirrorx,int mirrory,bool force=false)
    {

        base.draw(mirrorx,mirrory,force);


    }



    public void lookAt(float angle)
    {
        lookFrom = lookTo;
        lookTo = angle;

        looklerp = 0;
    }
}
