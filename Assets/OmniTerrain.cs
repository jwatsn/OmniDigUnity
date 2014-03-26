using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class tUpdate
{
    public int x,y,by;
    public short lastId, lastHP,counter;
    public tUpdate(int by,int x, int y)
    {
        this.by = by;

        this.x = x;
        this.y = y;

        int x2 = x / OmniTerrain.chunkSize;
        int y2 = y / OmniTerrain.chunkSize;

        if (x2 >= OmniTerrain.Width || x2 < 0 || y2 >= OmniTerrain.Height || y2 < 0)
            return;


        int x3 = x % OmniTerrain.chunkSize;
        int y3 = y % OmniTerrain.chunkSize;

        if (!OmniTerrain.chunks.ContainsKey(x2 + y2 * OmniTerrain.Width))
            return;

        lastId = (short)OmniTerrain.chunks[x2 + y2 * OmniTerrain.Width].tileMap[x3 + y3 * OmniTerrain.chunkSize];
        lastHP = (short)OmniTerrain.chunks[x2 + y2 * OmniTerrain.Width].damageMap[x3 + y3 * OmniTerrain.chunkSize];

    }

    public tUpdate()
    {
    }

    public virtual void handle()
    {
    }
}

public class terrainDmg : tUpdate
{
    public int itemID;

    public terrainDmg(OmniObject by, OmniItemType item,int x,int y)
        :base(by.id,x,y)
    {
        this.itemID = item.id;
    }

    public override void handle()
    {
        int x2 = x / OmniTerrain.chunkSize;
        int y2 = y / OmniTerrain.chunkSize;
        int x3 = x % OmniTerrain.chunkSize;
        int y3 = y % OmniTerrain.chunkSize;
        OmniItemType item = OmniItems.itemTypes[itemID];
        OmniTerrain.chunks[x2 + y2 * OmniTerrain.Width].damageMap[x3 + y3 * OmniTerrain.chunkSize] += (byte)item.str;
        OmniTerrain.chunks[x2 + y2 * OmniTerrain.Width].checkDamageMap(x3 + y3 * OmniTerrain.chunkSize);
    }

    public terrainDmg()
        :base()
    {
    }
}

public class terrainAddBlock : tUpdate
{
    public int itemID;

    public terrainAddBlock(OmniObject by, OmniItemType item, int x, int y)
        : base(by.id, x, y)
    {
        this.itemID = item.id;
    }

    public override void handle()
    {
        int x2 = x / OmniTerrain.chunkSize;
        int y2 = y / OmniTerrain.chunkSize;
        int x3 = x % OmniTerrain.chunkSize;
        int y3 = y % OmniTerrain.chunkSize;
        if(OmniTerrain.chunks[x2 + y2 * OmniTerrain.Width].tileMap[x3 + y3 * OmniTerrain.chunkSize] == -1)
        {
            OmniTerrain.chunks[x2 + y2 * OmniTerrain.Width].tileMap[x3 + y3 * OmniTerrain.chunkSize] = itemID;
            OmniTerrain.chunks[x2 + y2 * OmniTerrain.Width].GenSlant();
            OmniTerrain.chunks[x2 + y2 * OmniTerrain.Width].GenMesh(1);
        }
        
    }
    public terrainAddBlock()
        : base()
    {
    }
}

public class OmniTerrain : MonoBehaviour {

   

	public static int chunkSize = 16;
	public static int Width = 64;
	public static int Height = 64;

    public static int BlockUpdateSpeed = 1;

    public static OmniTerrain instance;

    public static Vector3[] frustrum;
	public static Dictionary<int,TerrainChunk> chunks;
    public static Dictionary<int, int> biomes;
    public static List<TerrainChunk> loadedChunks;
    public static List<TerrainChunk> activeChunks;
    public static List<tUpdate> terrainUpdates;

    List<tUpdate> toValidate;
    List<tUpdate> toRemove;
    int BlockCounter = 0;
    OmniWorld world;
	Camera camera;

	void OnEnable() {
			frustrum = new Vector3[] {
			new Vector3 (0, 0, 0),
			new Vector3 (1, 1, 0),
            new Vector3 (1, 0, 0),
		};
            instance = this;
	}
	// Use this for initialization
	void Start () {
        activeChunks = new List<TerrainChunk>();
        loadedChunks = new List<TerrainChunk>();
        terrainUpdates = new List<tUpdate>();
        toValidate = new List<tUpdate>();
        toRemove = new List<tUpdate>();
        OmniTerrain.chunks = new Dictionary<int, TerrainChunk>();
        OmniTerrain.biomes = new Dictionary<int, int>();
        world = GetComponent<OmniWorld>();
		camera = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<Camera> ();
	}

	void GenBiomes() {
        //Biome = BiomeManager.generateBiomeHills(OmniTerrain.Width, OmniTerrain.Height, 2, 1, Biomes.Forest, Biomes.Underground, Biomes.Sky, true);
	}

	public TerrainChunk getChunk(int x, int y) {

		//int x2 = (x/chunkSize);
		//int y2 = (y/chunkSize);

		return chunks [x + y * chunkSize];

	}

    public static byte getSlant(float x, float y)
    {

        if (x >= (Width * chunkSize) - 1)
            x = x - (Width * chunkSize);
        if (x < 0)
            x = (Width * chunkSize) + x;

        if (y >= (Height * chunkSize) - 1)
            y = y - (Height * chunkSize);
        if (y < 0)
            y = (Height * chunkSize) + y;



        int x2 = (int)x / chunkSize;
        int y2 = (int)y / chunkSize;



        int x3 = (int)x % chunkSize;
        int y3 = (int)y % chunkSize;

        if (!chunks.ContainsKey(x2 + y2 * Width))
            return 0;
        if (chunks[x2 + y2 * Width].slant == null)
            return 0;


        return chunks[x2 + y2 * Width].slant[x3 + y3 * chunkSize];

    }

    public static bool isBlock(int x, int y)
    {

        if (x >= (Width * chunkSize) - 1)
            x = x - (Width * chunkSize);
        else if (x < 0)
            x = (Width * chunkSize) + x;

        if (y >= (Height * chunkSize) - 1)
            y = y - (Height * chunkSize);
        else if (y < 0)
            y = (Height * chunkSize) + y;
       
        int x2 = x / chunkSize;
        int y2 = y / chunkSize;




        int x3 = x % chunkSize;
        int y3 = y % chunkSize;

        if (!chunks.ContainsKey(x2 + y2 * Width))
            return false;

        int id = chunks[x2 + y2 * Width].tileMap[x3 + y3 * chunkSize];
        if (id >= 0)
        {
            if(OmniItems.itemTypes[id] is OmniBlock)
            {
                return true;
            }
        }

        return false;

    }
    public static int getDamage(int x, int y)
    {
        int x2 = x / chunkSize;
        int y2 = y / chunkSize;



        int x3 = x % chunkSize;
        int y3 = y % chunkSize;

        if (!chunks.ContainsKey(x2 + y2 * Width))
            return 0;

        return chunks[x2 + y2 * Width].damageMap[x3 + y3 * chunkSize];
    }

    public static void blockReverse(tUpdate update)
    {
        int x2 = update.x / chunkSize;
        int y2 = update.y / chunkSize;



        int x3 = update.x % chunkSize;
        int y3 = update.y % chunkSize;

        if (!chunks.ContainsKey(x2 + y2 * Width))
            return;

        chunks[x2 + y2 * Width].tileMap[x3 + y3 * chunkSize] = update.lastId;
        chunks[x2 + y2 * Width].damageMap[x3 + y3 * chunkSize] = update.lastHP;

        chunks[x2 + y2 * Width].GenSlant();
        chunks[x2 + y2 * Width].GenMesh(1);
    }

    public static void blockUpdate(BlockEvent e)
    {
        for (int i = 0; i < e.updates.Length; i++)
        {

            int x2 = e.updates[i].x / chunkSize;
            int y2 = e.updates[i].y / chunkSize;




            int x3 = e.updates[i].x % chunkSize;
            int y3 = e.updates[i].y % chunkSize;

            if (!chunks.ContainsKey(x2 + y2 * Width))
                return;

            if (e.fromServer && e.updates[i].by == OmniLocal.LocalID)
            {
                instance.toRemove.Clear();
                for (int j = 0; j < instance.toValidate.Count; j++)
                {
                    if (instance.toValidate[j].x == e.updates[i].x && instance.toValidate[j].y == e.updates[i].y)
                    {
                        instance.toRemove.Add(instance.toValidate[j]);
                        continue;
                    }
                }
                for (int k = 0; k < instance.toRemove.Count; k++)
                {
                    instance.toValidate.Remove(instance.toRemove[k]);
                }
               
            }

            e.updates[i].handle();
            
            if(!e.fromServer)
            if(e.updates[i].by == OmniLocal.LocalID)
            if (Network.isClient)
                instance.toValidate.Add(e.updates[i]);
            
            
        }
        

        
    }

    public void OmniUpdate(float delta)
    {

        for (int i = 0; i < OmniTerrain.activeChunks.Count; i++)
        {
            if (OmniTerrain.activeChunks[i].rebuildMesh)
            {
                OmniTerrain.activeChunks[i].GenMesh(1);
                OmniTerrain.activeChunks[i].rebuildMesh = false;
            }
        }

        if (terrainUpdates.Count > 0)
        {

            BlockEvent e = new BlockEvent(OmniWorld.tick, terrainUpdates.ToArray());
            OmniEvents.AddEvent(e);

            if (OmniWorld.isDebugging)
                Debug.Log("tUpdates: " + terrainUpdates.Count + " : " + e.updates[0].x + "," + e.updates[0].y);

            terrainUpdates.Clear();
        }

        bool flag = false;
        for (int i = 0; i < toValidate.Count; i++)
        {
            toValidate[i].counter++;
            if (toValidate[i].counter > OmniWorld.localDelay * 2)
            {
                flag = true;
                break;
            }
        }
        if (flag)
        {
            toValidate.Clear();
            for (int i = 0; i < OmniTerrain.activeChunks.Count; i++)
            {
                RChunkEvent e = new RChunkEvent(OmniWorld.tick);
                e.x = (int)OmniTerrain.activeChunks[i].pos.x / OmniTerrain.chunkSize;
                e.y = (int)OmniTerrain.activeChunks[i].pos.y / OmniTerrain.chunkSize;
                e.player = Network.player;
                OmniEvents.AddEvent(e);
            }


        }

    }

    public static bool isSolid(int x, int y)
    {
        if (x >= (Width * chunkSize))
            x = x - (Width * chunkSize);
        else if (x < 0)
            x = (Width * chunkSize) + x;

        if (y >= (Height * chunkSize))
            y = y - (Height * chunkSize);
        else if (y < 0)
            y = (Height * chunkSize) + y;

		int x2 = x / chunkSize;
		int y2 = y / chunkSize;

		int x3 = x % chunkSize;
		int y3 = y % chunkSize;

        if (!OmniTerrain.chunks.ContainsKey(x2 + y2 * Width))
            return false;

        if (!chunks.ContainsKey(x2 + y2 * Width))
            return false;

        if (!chunks[x2 + y2 * Width].loaded)
            return false;

		if (chunks [x2 + y2 * Width].tileMap [x3 + y3 * chunkSize] < 0)
						return false;

		return true;

	}


    List<OmniObject> drawObjs = new List<OmniObject>();
    public void draw()
    {
        drawObjs.Clear();

        Vector3 bottomLeft = new Vector3();// OmniCamera.cam.ViewportToWorldPoint(OmniTerrain.frustrum[0]);
        Vector3 topRight = new Vector3();//OmniCamera.cam.ViewportToWorldPoint(OmniTerrain.frustrum[1]);
        //Vector3 bottomLeft2 = OmniCamera.cam.ViewportToWorldPoint(OmniTerrain.frustrum[0]);
        //Vector3 topRight2 = OmniCamera.cam.ViewportToWorldPoint(OmniTerrain.frustrum[1]);


        ControllableObject obj = OmniLocal.getLocalPlayer();
        if (obj != null)
        {
            bottomLeft.x = (obj.bounds.x + obj.item.Size / 2f) - OmniCamera.cam.orthographicSize*1.5f;
            bottomLeft.y = (obj.bounds.y + obj.item.Size / 2f) - OmniCamera.cam.orthographicSize*OmniCamera.cam.aspect;

            topRight.x = (obj.bounds.x + obj.item.Size / 2f) + OmniCamera.cam.orthographicSize*1.5f;
            topRight.y = (obj.bounds.y + obj.item.Size / 2f) + OmniCamera.cam.orthographicSize * OmniCamera.cam.aspect;

        
        }



            int x1 = (int)bottomLeft.x / OmniTerrain.chunkSize;
            int x2 = (int)topRight.x / OmniTerrain.chunkSize;
            int y1 = (int)bottomLeft.y / OmniTerrain.chunkSize;
            int y2 = (int)topRight.y / OmniTerrain.chunkSize;
            


            if (bottomLeft.x < 0)
            {
                if (x1 == 0)
                    x1 = -1;
                else
                {
                    x1 = x1 - 1;
                }


            }


            if (bottomLeft.y < 0)
            {
                if (y1 == 0)
                {
                    
                    y1 = -1;
                }
                else
                {
                    y1 = y1 - 1;
                }

            }




            for (int xx = x1; xx <= x2; xx++)
            {
                for (int yy = y1; yy <= y2; yy++)
                {

                    int x = xx;
                    int y = yy;

                   

                    if (x < 0)
                    {
                        x = (OmniTerrain.Width) + x;

                    }
                    else if (x >= OmniTerrain.Width)
                    {
                        x = x - OmniTerrain.Width;
                    }

                    if (y < 0)
                    {
                        y = (OmniTerrain.Height) + y;
                        
                    }
                    else if (y >= OmniTerrain.Height)
                    {
                        y = y - OmniTerrain.Height;
                        
                    }

                    int i = x + y * OmniTerrain.Width;



                    if (OmniTerrain.chunks.ContainsKey(i))
                    {                      



                            for (int j = 0; j < OmniTerrain.chunks[i].objects.Count; j++)
                            {
                                if(OmniTerrain.chunks[i].objects[j].mountedTo == null)
                                if (!drawObjs.Contains(OmniTerrain.chunks[i].objects[j]))
                                    drawObjs.Add(OmniTerrain.chunks[i].objects[j]);              
                            }
                        /*
                            if (x == 0)
                            {
                                for (int j = 0; j < OmniTerrain.chunks[i].objects.Count; j++)
                                {
                                    OmniTerrain.chunks[i].objects[j].draw(2, 0);
                                }
                            }
                            else if (x == OmniTerrain.Width-1)
                            {
                                for (int j = 0; j < OmniTerrain.chunks[i].objects.Count; j++)
                                {
                                    OmniTerrain.chunks[i].objects[j].draw(1, 0);
                                }
                            }

                            if (y == 0)
                            {
                                for (int j = 0; j < OmniTerrain.chunks[i].objects.Count; j++)
                                {
                                    OmniTerrain.chunks[i].objects[j].draw(0, 2);
                                    
                                }
                            }
                            else if (y == OmniTerrain.Height - 1)
                            {
                                for (int j = 0; j < OmniTerrain.chunks[i].objects.Count; j++)
                                {
                                    OmniTerrain.chunks[i].objects[j].draw(0,1);
                                    
                                }
                            }
                        */
                            Vector3 chunkPos = OmniTerrain.chunks[i].pos;
                            if (xx < 0)
                                chunkPos.x = -(OmniTerrain.Width * OmniTerrain.chunkSize - chunkPos.x);
                            else if (xx >= OmniTerrain.Width)
                                chunkPos.x = OmniTerrain.Width * OmniTerrain.chunkSize + chunkPos.x;

                            if (yy < 0)
                                chunkPos.y = -(OmniTerrain.Height * OmniTerrain.chunkSize - chunkPos.y);
                            else if (yy >= OmniTerrain.Height)
                                chunkPos.y = OmniTerrain.Height * OmniTerrain.chunkSize + chunkPos.y;

                            if (OmniTerrain.chunks[i].mesh[OmniLocal.LevelOfDetail] == null)
                                OmniTerrain.chunks[i].GenMesh(OmniLocal.LevelOfDetail + 1);

                            Graphics.DrawMesh(OmniTerrain.chunks[i].mesh[OmniLocal.LevelOfDetail], chunkPos, Quaternion.identity, OmniAtlas.texture, 0);



                    }
                }



            }
       

        /*
        for (int i = 0; i < activeChunks.Count; i++)
        {

            for (int j = 0; j < activeChunks[i].objects.Count; j++)
            {
                if (drawObjs.Contains(activeChunks[i].objects[j]))
                {
                    continue;
                }

                activeChunks[i].objects[j].draw();
                drawObjs.Add(activeChunks[i].objects[j]);
            }


            Graphics.DrawMesh(activeChunks[i].mesh, activeChunks[i].pos, Quaternion.identity, OmniAtlas.texture, 0);
        }

        */

            for (int i = 0; i < drawObjs.Count; i++)
            {
                drawObjs[i].draw(0, 0);

                /*
                int chX1 = (int)(drawObjs[i].bounds.x / OmniTerrain.chunkSize);
                int chY1 = (int)(drawObjs[i].bounds.y / OmniTerrain.chunkSize);
                if(chX1 == 0)
                    drawObjs[i].draw(2, 0);
                else if(chX1 == OmniTerrain.Width-1)
                    drawObjs[i].draw(1, 0);
                if (chY1 == 0)
                    drawObjs[i].draw(0, 2);
                else if (chY1 == OmniTerrain.Height - 1)
                    drawObjs[i].draw(0, 1);
                 * */


                
               



            }

    }

    public static void hitTerrain(OmniObject player, OmniItemType item)
    {

    }

 
}
