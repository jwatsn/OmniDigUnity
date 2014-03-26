using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class OmniItemType : MonoBehaviour {

    public static int Max_Bounds = 12;

    public float MaxSpeed = 10;
    public float Acceleration = 20;
    public float JumpStrength = 10;
    public float RecoverySpeed = 0.2f;
    public int layer = 1;

        [HideInInspector]
    public int id;
        [HideInInspector]
    public int mountId = -1;
    public int baseHP = 1;
    public int max_stack = 32;
    public int str = 1;
    public float KnockBack = 1;
    public float delay = 0.5f;
        [HideInInspector]
    public bool eatsMouse = false;
    [HideInInspector]
    public float collisionDelay = 0.06f;
    public float stunTime = 0.1f;
    public float Size = 1;
    [HideInInspector]
    public float halfSize;
    public float Gravity = 9.6f;
    public float Friction = 40;
    public float swingSpeed = 30;

    public bool DrawOntop = false;
    public bool Usable = true;
    public Vector2 DamageVelScale = new Vector2(1, 1);
    public float damageRotationSpeed = 0;

    [HideInInspector]public Vector3 scale;
    [HideInInspector]public OmniAnimation[] animList;
    [HideInInspector]public GameObject itemImage;
    List<Rect> r;
    List<TerrainChunk> activeChunks;


	void Start () {
        scale = new Vector3(Size, Size,0);
        itemImage = GameObject.Find(name + "Image");
        if (itemImage != null)
        {
            animList = itemImage.GetComponents<OmniAnimation>();
            
        }
        //bounds = new Rect();
        r = new List<Rect>();
        activeChunks = new List<TerrainChunk>();
        Init();
        halfSize = Size / 2;


	}

    

    void Update()
    {
	


	}

    public int getAnimId(string name)
    {
        for (int i = 0; i < animList.Length; i++)
            if (animList[i].animName == name)
                return i;

        return -1;
    }

    public virtual void OnFlip(OmniObject owner)
    {

    }

    public virtual void OnClicked(OmniObject owner, OmniObject clicker)
    {
    }

    public virtual void OnMount(OmniObject owner, OmniObject mountObj)
    {
    }

    public virtual void OnDismount(OmniObject owner, OmniObject mountObj)
    { }

    public virtual void CheckBounds(OmniObject owner)
    {



        if (owner is PhysicsObject)
        {
            PhysicsObject physowner = (PhysicsObject)owner;
            animList[0].GetBoundsPos((PhysicsObject)owner, scale, owner.mount0time);
            physowner.numBounds = animList[0].boundsList.Length;

            if (owner.mount0time < 0.06)
                return;

            collisionTest((PhysicsObject)owner);
            int maskx = 0;
            int masky = 0;
            for (int i = 0; i < r.Count; i++)
            {
                
                for (int j = 0; j < physowner.numBounds; j++)
                {
                    
                    if (physowner.itemBounds[j].Overlaps(r[i]))
                    {
                        int xx = (int)r[i].x;
                        int yy = (int)r[i].y;

                        

                        if (xx < 0)
                            xx = OmniTerrain.Width * OmniTerrain.chunkSize + xx;
                        else if (xx >= OmniTerrain.Width * OmniTerrain.chunkSize)
                        {
                            xx = (xx - (OmniTerrain.Width * OmniTerrain.chunkSize));
                        }
                        if (yy < 0)
                            yy = OmniTerrain.Height * OmniTerrain.chunkSize + yy;
                        else if (yy >= OmniTerrain.Height * OmniTerrain.chunkSize)
                        {
                            
                            yy = (yy - (OmniTerrain.Height * OmniTerrain.chunkSize));

                        }

                        OnTerrainCollision(owner, xx, yy);                       
                    }
                }
            }

            for (int i = 0; i < physowner.numBounds; i++)
            {
                for (int k = 0; k < activeChunks.Count; k++)
                {
                    for (int j = 0; j < activeChunks[k].objects.Count; j++)
                    {

                        if (activeChunks[k].objects[j].id != owner.id)
                        {
                            CheckIfPlayerHit(owner.itemBounds[i], i, owner, activeChunks[k].objects[j]);

                            if (owner.itemBounds[i].x < 0)
                            {
                                Rect r = owner.itemBounds[i];
                                r.x = OmniTerrain.Width * OmniTerrain.chunkSize + r.x;
                                CheckIfPlayerHit(r, i, owner, activeChunks[k].objects[j]);
                            }
                            else if (owner.itemBounds[i].x + owner.itemBounds[i].width >= OmniTerrain.Width * OmniTerrain.chunkSize)
                            {
                                Rect r = owner.itemBounds[i];
                                r.x = r.x - (OmniTerrain.Width * OmniTerrain.chunkSize-1);
                                CheckIfPlayerHit(r, i, owner, activeChunks[k].objects[j]);
                            }

                            if (owner.itemBounds[i].y < 0)
                            {
                                Rect r = owner.itemBounds[i];
                                r.y = OmniTerrain.Height * OmniTerrain.chunkSize + r.y;
                                CheckIfPlayerHit(r, i, owner, activeChunks[k].objects[j]);
                            }
                            else if (owner.itemBounds[i].y + owner.itemBounds[i].height >= OmniTerrain.Height * OmniTerrain.chunkSize)
                            {
                                Rect r = owner.itemBounds[i];
                                r.y = r.y - (OmniTerrain.Height * OmniTerrain.chunkSize - 1);
                                CheckIfPlayerHit(r, i, owner, activeChunks[k].objects[j]);
                            }
                            
                        }

                    }
                }
            }
        }



        /*
        bounds.width = animList[0].boundsScale.x * Size;
        bounds.height = animList[0].boundsScale.y * Size;
        Vector3 pos = new Vector3(owner.bounds.x, owner.bounds.y, 0);
        if (owner.flipped)
        {

            pos = animList[0].GetBoundsPos(owner, scale, owner.mount0time);
            bounds.x = pos.x - bounds.width;// -bounds.width / 2;
            bounds.y = pos.y;// -bounds.height / 2;
        }
        else
        {
            pos = animList[0].GetBoundsPos(owner, scale, owner.mount0time);
            bounds.x = pos.x;// -bounds.width / 2;
            bounds.y = pos.y;// -bounds.height / 2;
        }

        

         * */
    }

    public int getSkelLoc(OmniObject player, string loc)
    {

        for (int i = 0; i < player.skeleton.Length; i++)
        {

            if (player.skeleton[i].name == loc)
                return i;
        }

        return -1;
    }

    void CheckIfPlayerHit(Rect b,int i,OmniObject owner,OmniObject victim)
    {
        if (b.Overlaps(victim.bounds))
        {

            Vector2 vel = Vector2.zero;
            if (owner is ControllableObject)
                vel = owner.itemBoundsVel[i].normalized * Mathf.Abs(((ControllableObject)owner).fireVel * 0.01f);
            if (owner is PhysicsObject)
            {
                PhysicsObject p = ((PhysicsObject)owner);
                Vector2 pos = Vector2.zero;
                if (p.isProjectile)
                {
                    vel = p.vel * 0.3f;

                    int limb = getLimbHit(victim, owner.itemBounds[i], ref pos);
                    owner.item.OnHit(owner, victim, vel, pos, limb);
                }
                else
                {
                    if (owner.mount0 >= 0)
                        OmniItems.itemTypes[owner.mount0].OnHit(owner, victim, vel, new Vector2(owner.itemBounds[i].x, owner.itemBounds[i].y), getLimbHit(owner, owner.itemBounds[i], ref pos));
                }
            }

        }
    }

    int getLimbHit(OmniObject player, Rect r, ref Vector2 pos)
    {
        
        if (player.skeleton == null)
            return -1;

        Rect b = new Rect();

                for (int i = 0; i < player.skeleton.Length; i++)
                {
                    if (player.skeleton[i].name == "bounds")
                        continue;

                    b.x = player.bounds.x;
                    b.y = player.bounds.y;
                    b.x += player.skeleton[i].bounds.x * player.item.Size;
                    b.y += player.skeleton[i].bounds.y * player.item.Size;
                    b.width = player.skeleton[i].bounds.width * player.item.Size;
                    b.height = player.skeleton[i].bounds.height * player.item.Size;
                    if (r.Overlaps(b))
                    {
                        pos = new Vector2(r.x - b.x, r.y - b.y);
                        return i;
                    }
                }
            
        

        return -1;
    }
    public virtual void OmniUpdate(OmniObject owner, float delta)
    {
        owner.animId = id;
    }

    void collisionTest(PhysicsObject owner)
    {
        
        r.Clear();
        activeChunks.Clear();
        
        for (int i = 0; i < owner.numBounds; i++)
            {
                int c1x = (int)(owner.itemBounds[i].x + owner.itemBounds[i].width/2f);
                int c1y = (int)(owner.itemBounds[i].y + owner.itemBounds[i].height/2f);

            /*
                if ((owner.itemBounds[i].x + owner.itemBounds[i].width / 2f) < 0)
                {
                    if (c1x == 0)
                        c1x = -1;
                    else
                        c1x = c1x - 1;
                    
                }
            */
                if (owner.itemBounds[i].x < 0 || owner.itemBounds[i].x >= OmniTerrain.Width * OmniTerrain.chunkSize || owner.itemBounds[i].y < 0 || owner.itemBounds[i].y >= OmniTerrain.Height * OmniTerrain.chunkSize)
                {
                    int xx = c1x;
                    int yy = c1y;
                    if (owner.itemBounds[i].x < 0)
                    {
                        if (xx == 0)
                            xx = -1;
                        else
                            xx = xx - 1;
                    }
                    if (owner.itemBounds[i].y < 0)
                    {
                        if (yy == 0)
                            yy = -1;
                        else
                            yy = yy - 1;
                    }

                    if (OmniTerrain.isSolid(xx, yy))
                    {
                        r.Add(new Rect(xx, yy, 1, 1));
                    }
                }
                if (OmniTerrain.isSolid(c1x, c1y))
                {
                    r.Add(new Rect(c1x, c1y, 1, 1));
                }
            
                int c2x = c1x / OmniTerrain.chunkSize;
                int c2y = c1y / OmniTerrain.chunkSize;

                if (!OmniTerrain.chunks.ContainsKey(c2x + c2y * OmniTerrain.Width))
                {
                    continue;
                }

                TerrainChunk ch = OmniTerrain.chunks[c2x + c2y * OmniTerrain.Width];
                
                if (!activeChunks.Contains(ch))
                    activeChunks.Add(ch);
             
            }
        
        
         

    }

    public virtual void OnUse(ControllableObject player, Vector3 pos) { }

    public virtual void OnUseTick(OmniObject player, Vector3 pos) { }

    public virtual void Init() { }

    public virtual void OnTerrainCollision(OmniObject player, int x, int y) { }

    public virtual void OnHit(OmniObject player, OmniObject hitPlayer, Vector2 vel,Vector2 pos,int location) { }

    public virtual void Spawned(OmniObject player)
    {
        player.Spawned();
        player.skeleton = new AnimSkeleton[animList[0].skeletonRects.Length];
        for (int i = 0; i < animList[0].skeletonRects.Length; i++)
        {
            player.skeleton[i] = new AnimSkeleton(animList[0].skeletonRects[i]);
            if (player.skeleton[i].name == "bounds")
                player.skelBoundsId = i;
        }
    }


    public virtual void OnDamage(OmniObject dmged, OmniObject attacker, OmniItemType itemType, Vector2 vel) { }

    public virtual void AltFire(ControllableObject player) { }

    public virtual void OnStop(OmniObject player) { }

    public virtual void Anim(OmniObject player, float time,OmniAnimation.Type type,params Vector3[] mount)
    {

        animList[0].drawFrame(player, time, scale,type,mount);
    }

    public static float getAngle(OmniObject obj, Vector3 pos, out float len)
    {

        float x2 = ((int)pos.x + 0.5f) - ((int)obj.bounds.x + 0.5f);
        float y2 = ((int)pos.y + 0.5f) - ((int)obj.bounds.y + 0.5f);
        float angle = Mathf.Atan2(y2, x2);
        len = new Vector2(x2, y2).magnitude;
        return angle;
    }

    public virtual void draw(OmniObject player)
    {
        if (player.animId < 0)
            return;

        if (animList[0].animationType != OmniAnimation.Type.Skeleton)
        {
            if (player.animId < animList.Length)
                animList[player.animId].drawFrame(player, player.stateTime, scale, animList[player.animId].animationType);
            else
                Debug.Log(player.animId);
        }
        else
            animList[0].drawFrame(player, player.stateTime, scale, animList[0].animationType);
    }


}
