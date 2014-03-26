using System.Collections.Generic;
using UnityEngine;

public class ObjectNetworkUpdate
{
    public const int Position = 1;
    public const int Animation = 2;
    public const int Direction = 3;
    public const int Mount0 = 4;
    public const int Mount1 = 5;
    public const int Mount2 = 6;
    public const int Rotation = 7;
    public const int Skeleton = 8;
    public const int LookRot = 9;
}

public struct mountObject
{
    public OmniObject obj;
    public Vector2 offset;
    public int location;

}

public class OmniObject
{
    int mountUpdateMask = -1;
    public int skelUpdateMask = 0;
    public int inputMask;
    public int lastInputMask;
    public int movementMask;

    public int firedBy;

    public Vector2 vel;
    public Vector2 accel;

    public string Name;
    public OmniObject mountedTo;
    public int mountPoint = -1;
    public int mountLoc = -1;

    public int heldId;

    public bool Respawnable;
    public bool isDead = false;
    [HideInInspector]
    public float rotation = 0;
    [HideInInspector]
    float lastRot;
    public float lerpSpeed;
    [HideInInspector]
    public int mount0 = -1;
    [HideInInspector]
    public int mount1 = -1;
    [HideInInspector]
    public int mount2 = -1;
    [HideInInspector]
    public bool isProjectile;
    [HideInInspector]
    public float mount0time,mount1time,mount2time;
    int lastmount0 = -1;
    int lastmount1 = -1;
    int lastmount2 = -1;
    [HideInInspector]
    public Vector3[] mountPos;
    [HideInInspector]
    public List<TerrainChunk> activeChunks;
    List<TerrainChunk> lastChunks;
    [HideInInspector]
    public float stateTime;
    [HideInInspector]
    public bool spawned;
    [HideInInspector]
    public bool isGhost;
    [HideInInspector]
    public GhostObject ghost;
    [HideInInspector]
    public OmniItemType item;
    [HideInInspector]
    public int id;
    [HideInInspector]
    public int animId;
    [HideInInspector]
    public int updateMask = 0;
    [HideInInspector]
    public Rect bounds;
    [HideInInspector]
    public Vector3 lastPos;
    [HideInInspector]
    public string animName;
    string lastanimName;
    [HideInInspector]
    public AnimSkeleton[] skeleton;
    [HideInInspector]
    public int numBounds;
    [HideInInspector]
    public float lookRot;
    [HideInInspector]
    public float fireAngle;
    [HideInInspector]
    public int updateTick;
    [HideInInspector]
    public List<mountObject> mountObjects;

    [HideInInspector]
    public int skelBoundsId;

    public float HP;
    [HideInInspector]
    public OmniAnimation.Type animType;

    public Rect[] itemBounds;
    public Matrix4x4[] itemMatrix;


    public Vector2[] itemBoundsLastPos;
    public Vector2[] itemBoundsVel;
    public Vector2[] itemBoundsLastVel;

    public bool flipped;
    bool lastFlipped;

    public Vector3 Position;
    public Vector3 position { get { return new Vector3(Position.x, Position.y, Position.z); } }
    public Vector2 boundsPos { get { return new Vector3(bounds.x, bounds.y); } }

    public OmniObject(int id,int itemId)
    {
        this.id = id;
        vel = new Vector2();
        accel = new Vector2();
        setItem(itemId);
        animType = item.animList[0].animationType;
        float bw = item.scale.x * item.animList[0].boundsSizeOffset.x;
        bounds = new Rect(0, 0, item.scale.x + bw, item.scale.y);
        Position = new Vector3();
        lastPos = new Vector3();
        animName = "idle"; // default

        activeChunks = new List<TerrainChunk>();
        lastChunks = new List<TerrainChunk>();
        lerpSpeed = OmniWorld.timeStep;
        mountObjects = new List<mountObject>();
        itemBounds = new Rect[OmniItemType.Max_Bounds];
        itemBoundsVel = new Vector2[OmniItemType.Max_Bounds];
        itemBoundsLastPos = new Vector2[OmniItemType.Max_Bounds];
        itemBoundsLastVel = new Vector2[OmniItemType.Max_Bounds];
        itemMatrix = new Matrix4x4[OmniItemType.Max_Bounds];
        for (int i = 0; i < itemBounds.Length; i++)
        {
            itemBounds[i] = new Rect();
            itemBoundsVel[i] = new Vector2();
            itemBoundsLastVel[i] = new Vector2();

        }
        HP = item.baseHP;
    }

    public void setItem(int itemid)
    {
        item = OmniItems.itemTypes[itemid];
        if (item == null)
            Debug.Log("ERR: " + id + " item null");
    }

    public void setPos(float x, float y)
    {
        Position.Set(x, y, 0);
        lastPos.Set(x, y, 0);
        bounds.x = x;
        bounds.y = y;

        if (isGhost)
        {
            ghost.lastGhostPos.x = x;
            ghost.lastGhostPos.y = y;
            ghost.ghostPos.x = x;
            ghost.ghostPos.y = y;

        }
    }

    public virtual void OmniUpdate(float delta)
    {
        if (spawned)
        {
            CheckChunks();
            if (isGhost)
                ghost.OmniUpdate(delta);
            else
            {
                item.OmniUpdate(this, delta);
                if (mount0 >= 0)
                {
                    OmniItems.itemTypes[mount0].CheckBounds(this);
                }


                if (mount0 >= 0)
                {

                    mount0time += delta;
                    if (mount0time > OmniItems.itemTypes[mount0].delay + 0.06f)
                    {
                        OmniItems.itemTypes[mount0].OnStop(this);
                        mount0 = -1;
                        mount0time = 0;
                    }
                    mountUpdateMask |= 1 << ObjectNetworkUpdate.Mount0;
                    updateMask |= 1 << ObjectNetworkUpdate.Mount0;
                }
                else
                {
                    if ((mountUpdateMask & 1 << ObjectNetworkUpdate.Mount0) != 0)
                    {
                        int m = 1 << ObjectNetworkUpdate.Mount0;
                        mountUpdateMask &= ~m;




                        for (int i = 0; i < numBounds; i++)
                        {
                            itemBoundsVel[i].Set(0, 0);
                            itemBoundsLastVel[i].Set(0, 0);
                        }
                    }
                }

                if (mount1 >= 0)
                {
                    mount1time += delta;
                    mountUpdateMask |= 1 << ObjectNetworkUpdate.Mount1;
                    updateMask |= 1 << ObjectNetworkUpdate.Mount1;
                }
                else
                {
                    int m = 1 << ObjectNetworkUpdate.Mount0;
                    mountUpdateMask &= ~m;
                }
                if (mount2 >= 0)
                {
                    mount2time += delta;
                    mountUpdateMask |= 1 << ObjectNetworkUpdate.Mount2;
                    updateMask |= 1 << ObjectNetworkUpdate.Mount2;
                }
                else
                {
                    int m = 1 << ObjectNetworkUpdate.Mount2;
                    mountUpdateMask &= ~m;
                }

                stateTime += delta;
                if (rotation != lastRot)
                {
                    updateMask |= 1 << ObjectNetworkUpdate.Rotation;
                    lastRot = rotation;
                }

                if (lastFlipped != flipped)
                {
                    for (int i = 0; i < mountObjects.Count; i++)
                    {
                        mountObjects[i].obj.flipped = !mountObjects[i].obj.flipped;
                    }
                    updateMask |= 1 << ObjectNetworkUpdate.Direction;
                    lastFlipped = flipped;

                    item.OnFlip(this);
                }
            }
            

        }

        
        if(mountedTo != null)
        {

            Vector2 p2 = new Vector2(mountedTo.lastPos.x, mountedTo.lastPos.y);
            Vector2 p = new Vector2(mountedTo.bounds.x, mountedTo.bounds.y);
                


                lastPos = p2;
                bounds.x = p.x;
                bounds.y = p.y;

                vel = mountedTo.vel;

            
        }
         
        updateTick = OmniWorld.tick;
    }

    public virtual void Spawned()
    {
    }

    public virtual void draw(int mirrorx, int mirrory,bool force=false)
    {
        //if (mountedTo != null && !force)
            //return;

        Vector3 lastPos1 = Position;
        if (mirrorx == 1)
        {
            Position.x = -((OmniTerrain.Width * OmniTerrain.chunkSize) - Position.x);
        }
        else if (mirrorx == 2)
        {
            Position.x = (OmniTerrain.Width * OmniTerrain.chunkSize) + Position.x;
        }

        if (mirrory == 1)
        {
            Position.y = -((OmniTerrain.Height * OmniTerrain.chunkSize) - Position.y);
        }
        else if (mirrory == 2)
        {
            Position.y = (OmniTerrain.Height * OmniTerrain.chunkSize) + Position.y;
        }

        if (isGhost)
            ghost.draw();

        
        for (int i = 0; i < mountObjects.Count; i++)
            mountObjects[i].obj.draw(mirrorx, mirrory, true);
        

        if (Input.GetKeyDown(KeyCode.F1))
        {
            Matrix4x4 test = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(10, 10));
            test *= Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.AngleAxis(90,Vector3.forward), Vector3.one);
            Vector3 test2 = test.MultiplyPoint(new Vector3(1, 1, 0));
            Debug.Log(test2);
        }

        if (mount0 >= 0)
        {
            if(OmniItems.itemTypes[mount0].DrawOntop)
                item.Anim(this, stateTime, animType);

            OmniItems.itemTypes[mount0].Anim(this, mount0time,OmniAnimation.Type.EngineRotation);         
        }

        if (mount1 >= 0)
            OmniItems.itemTypes[mount1].Anim(this, mount1time, OmniAnimation.Type.EngineRotation);

        if (mount2 >= 0)
            OmniItems.itemTypes[mount2].Anim(this, mount1time,OmniAnimation.Type.EngineRotation,new Vector3(OmniItems.itemTypes[mount0].animList[0].boundsList[0].x,OmniItems.itemTypes[mount0].animList[0].boundsList[0].y, 90));

        // OmniItems.itemTypes[mount0].animList[0].mountPoint[1]

        if (mount0 < 0 || !OmniItems.itemTypes[mount0].DrawOntop && mount0 >= 0)
            item.Anim(this, stateTime, animType);

        Position = lastPos1;


    }

    public virtual void Mount(OmniObject mountTo, int mid)
    {


        mountObject m = new mountObject();
        m.obj = this;
        mountTo.mountObjects.Add(m);
        if (Network.isClient)
            if (id == OmniLocal.LocalID)
                if (mountTo.mountObjects[0].obj.id == id)
                    mountTo.isGhost = false;
    }

    public virtual void UnMount()
    {
        if (mountedTo != null)
        {
            if (Network.isClient)
                if (id == OmniLocal.LocalID)
                    if (mountedTo.mountObjects[0].obj.id == id)
                        mountedTo.isGhost = true;

            mountedTo.removeMount(this);
        }
    }

    public virtual void removeMount(OmniObject obj)
    {
        
        for (int i = 0; i < mountObjects.Count; i++)
            if (mountObjects[i].obj.id == obj.id)
            {
                mountObjects.RemoveAt(i);
                
                return;
            }

    }

    void OffMap(int x, int y)
    {
        bounds.x = OmniWorld.SpawnPoints[0].x * OmniTerrain.chunkSize + OmniTerrain.chunkSize / 2;
        bounds.y = OmniWorld.SpawnPoints[0].y * OmniTerrain.chunkSize + OmniTerrain.chunkSize;
    }

    public virtual void destroy(bool trueremove)
    {
        if(trueremove)
        for (int i = 0; i < activeChunks.Count; i++)
        {
            activeChunks[i].objects.Remove(this);
            
        }
        for (int i = 0; i < mountObjects.Count; i++)
        {
                mountObjects[i].obj.mountedTo = null;
                mountObjects[i].obj.mountLoc = -1;
        }
        activeChunks.Clear();
        OmniWorld.instance.SpawnedObjectsNew[id] = null;
        if(!OmniWorld.instance.openSlots.Contains(id))
            OmniWorld.instance.openSlots.Add(id);

    }

    public void CheckChunks()
    {



        TerrainChunk[] lastChunks = new TerrainChunk[activeChunks.Count];
        activeChunks.CopyTo(lastChunks);
        activeChunks.Clear();
        int c1 = (int)((bounds.x + bounds.width) - bounds.x);
        int c2 = (int)((bounds.y + bounds.height) - bounds.y);

       
            

        for (int x = 0; x <= item.Size; x++)
            for (int y = 0; y <= item.Size; y++)
            {
                int c1x = (int)(bounds.x + x) / OmniTerrain.chunkSize;
                int c1y = (int)(bounds.y + y) / OmniTerrain.chunkSize;

                if (c1x == 0)
                {
                    if (OmniTerrain.chunks.ContainsKey((OmniTerrain.Width-1)+ c1y * OmniTerrain.Width))
                    {
                        TerrainChunk ch2 = OmniTerrain.chunks[(OmniTerrain.Width - 1) + c1y * OmniTerrain.Width];
                        if (!activeChunks.Contains(ch2))
                        {
                            ch2.GenSlant();
                            activeChunks.Add(ch2);
                        }
                        if (!ch2.objects.Contains(this))
                            ch2.objects.Add(this);
                    }
                }
                else if (c1x == OmniTerrain.Width-1)
                {
                    if (OmniTerrain.chunks.ContainsKey((0) + c1y * OmniTerrain.Width))
                    {
                        TerrainChunk ch2 = OmniTerrain.chunks[(0) + c1y * OmniTerrain.Width];
                        if (!activeChunks.Contains(ch2))
                        {
                            ch2.GenSlant();
                            activeChunks.Add(ch2);
                        }
                        if (!ch2.objects.Contains(this))
                            ch2.objects.Add(this);
                    }
                }

                if (c1y == 0)
                {
                    if (OmniTerrain.chunks.ContainsKey(c1x + (OmniTerrain.Height-1) * OmniTerrain.Width))
                    {
                        TerrainChunk ch2 = OmniTerrain.chunks[c1x + (OmniTerrain.Height - 1) * OmniTerrain.Width];
                        if (!activeChunks.Contains(ch2))
                        {
                            ch2.GenSlant();
                            activeChunks.Add(ch2);
                        }
                        if (!ch2.objects.Contains(this))
                            ch2.objects.Add(this);
                    }
                }
                else if (c1y == OmniTerrain.Height-1)
                {
                    if (OmniTerrain.chunks.ContainsKey(c1x + (0) * OmniTerrain.Width))
                    {
                        TerrainChunk ch2 = OmniTerrain.chunks[c1x + (0) * OmniTerrain.Width];
                        if (!activeChunks.Contains(ch2))
                        {
                            ch2.GenSlant();
                            activeChunks.Add(ch2);
                        }
                        if (!ch2.objects.Contains(this))
                            ch2.objects.Add(this);
                    }
                }

                if (!OmniTerrain.chunks.ContainsKey(c1x + c1y * OmniTerrain.Width))
                {
                    continue;
                }

                TerrainChunk ch = OmniTerrain.chunks[c1x + c1y * OmniTerrain.Width];

                if (!ch.loaded)
                    continue;

                if (!activeChunks.Contains(ch))
                {
                    ch.GenSlant();
                    activeChunks.Add(ch);
                }
                if (!ch.objects.Contains(this))
                    ch.objects.Add(this);
            }


        if (mount0 >= 0)
        {
            for (int i = 0; i < itemBounds.Length; i++)
            {

                int c1x = (int)(itemBounds[i].x / OmniTerrain.chunkSize);
                int c1y = (int)(itemBounds[i].y / OmniTerrain.chunkSize);
                int c2x = (int)((itemBounds[i].x + itemBounds[i].width)/OmniTerrain.chunkSize);
                int c2y = (int)((itemBounds[i].y + itemBounds[i].height) / OmniTerrain.chunkSize);

                
                CheckItemChunk(c1x, c1y);
                CheckItemChunk(c2x, c2y);

               
            }
        }
        for (int i = 0; i < activeChunks.Count; i++)
        {
            if(!OmniTerrain.activeChunks.Contains(activeChunks[i]))
            {
                OmniTerrain.activeChunks.Add(activeChunks[i]);
            }
        }
        for (int i = 0; i < lastChunks.Length; i++)
        {
            if (!activeChunks.Contains(lastChunks[i]))
            {
                lastChunks[i].objects.Remove(this);
                
            }
        }

    }

    void CheckItemChunk(int c2x, int c2y)
    {
        int id = c2x + c2y * OmniTerrain.Width;
        if (OmniTerrain.chunks.ContainsKey(id))
        {


            if (!activeChunks.Contains(OmniTerrain.chunks[id]))
                activeChunks.Add(OmniTerrain.chunks[id]);
            if (!OmniTerrain.chunks[id].objects.Contains(this))
                OmniTerrain.chunks[id].objects.Add(this);
        }
    }
}