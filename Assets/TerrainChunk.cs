using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainChunk {


    public static int CollisionTileSize = 5;

    public bool loaded;
    public bool rebuildMesh;
	public Mesh[] mesh;
	public Vector3 pos;
	public int[] tileMap;
    public int[] damageMap;
    public byte[] slant;
    public List<OmniObject> objects;
    public List<int> pendingSlantUpdates;

	Vector3[] vert;
	Vector2[] uv;
	int[] tri;
	Rect drawRect;

	TPAtlas atlas;
	TPAtlasTexture dirt;
	TPAtlasTexture grasstex;



	int chunksize;
	int biome;

	OmniTerrain parent;


	public TerrainChunk(OmniTerrain parent,int x, int y,int size) {

        OmniTerrain.loadedChunks.Add(this);
        OmniTerrain.chunks[x + y * size] = this;
        if (OmniTerrain.biomes.ContainsKey(x + y * size))
            biome = OmniTerrain.biomes[x + y * size];


        objects = new List<OmniObject>();
        pendingSlantUpdates = new List<int>();
		atlas = TPackManager.getAtlas ("World");
		pos = new Vector3 ((int)(x*size), (int)(y*size), 0);
        mesh = new Mesh[] { new Mesh(), new Mesh() };
		chunksize = size;
        slant = new byte[chunksize * chunksize];
        for (int i = 0; i < slant.Length; i++)
        {
            slant[i] = 0;
        }
		this.parent = parent;

        if (Network.isClient)
        {
            RChunkEvent e = new RChunkEvent(OmniWorld.tick);
            e.x = (int)x;
            e.y = (int)y;
            e.player = Network.player;
            OmniEvents.AddEvent(e);
        }
        else
        {

            loaded = true;
            GenDamageMap(size);
            GenBiomes(size);
            GenSlant();
            GenMesh(1);

        }


	}
    void GenDamageMap(int size)
    {
        damageMap = new int[size * size];
        for (int i = 0; i < damageMap.Length; i++)
        {
            damageMap[i] = 0;
        }
    }
   void GenSlant(int x, int y)
    {


        int i = x + y * chunksize;

        int x2 = (int)(pos.x) / chunksize;
        int y2 = (int)(pos.y) / chunksize;

        if (!loaded)
            return;

            if (tileMap[i] >= 0)
            {
                int oldslant = slant[i];
                slant[i] = 0;
                int left = 0;
                int right = 0;
                int up = 0;
                int down = 0;

                int rightId = (x2 + 1) + y2 * OmniTerrain.Width;



                int leftId = (x2 - 1) + y2 * OmniTerrain.Width;
                int upId = x2 + (y2 + 1) * OmniTerrain.Width;
                int downId = x2 + (y2 - 1) * OmniTerrain.Width;

                if (x + 1 < chunksize)
                    right = tileMap[(x + 1) + y * chunksize];
                else if (OmniTerrain.chunks.ContainsKey(rightId))
                {
                    if ((x2 + 1) < OmniTerrain.Width)
                    {
                        if (OmniTerrain.chunks[rightId].loaded)
                        {
                            right = OmniTerrain.chunks[rightId].tileMap[0 + y * chunksize];
                        }
                    }
                    else
                    {
                        rightId = ((x2 + 1)-OmniTerrain.Width) + y2 * OmniTerrain.Width;
                        if (OmniTerrain.chunks.ContainsKey(rightId))
                        if (OmniTerrain.chunks[rightId].loaded)
                        {
                            right = OmniTerrain.chunks[rightId].tileMap[0 + y * chunksize];
                        }
                    }
                }

                if (x - 1 >= 0)
                    left = tileMap[(x - 1) + y * chunksize];
                else if (OmniTerrain.chunks.ContainsKey(leftId))
                {
                    if (x2 - 1 >= 0)
                    {
                        if (OmniTerrain.chunks[leftId].loaded)
                        {
                            left = OmniTerrain.chunks[leftId].tileMap[(chunksize - 1) + y * chunksize];
                        }
                    }
                    else
                    {
                        leftId = (OmniTerrain.Width+(x2 - 1)) + y2 * OmniTerrain.Width;
                        if (OmniTerrain.chunks.ContainsKey(leftId))
                        if (OmniTerrain.chunks[leftId].loaded)
                        {
                            left = OmniTerrain.chunks[leftId].tileMap[(chunksize - 1) + y * chunksize];
                        }
                    }
                }


                if (y + 1 < chunksize)
                    up = tileMap[x + (y + 1) * chunksize];
                else if (OmniTerrain.chunks.ContainsKey(upId))
                {
                    if (y2 + 1 < OmniTerrain.Height)
                    {
                        if (OmniTerrain.chunks[upId].loaded)
                        {
                            up = OmniTerrain.chunks[upId].tileMap[x + 0 * chunksize];
                        }
                    }
                    else
                    {
                        upId = x2 + (y2 - OmniTerrain.Height) * OmniTerrain.Width;
                        if (OmniTerrain.chunks.ContainsKey(upId))
                        if (OmniTerrain.chunks[upId].loaded)
                        {
                            up = OmniTerrain.chunks[upId].tileMap[x + 0 * chunksize];
                        }
                    }
                }

                if (y - 1 >= 0)
                    down = tileMap[x + (y - 1) * chunksize];
                else if (OmniTerrain.chunks.ContainsKey(downId))
                {
                    if (y2 - 1 >= 0)
                    {
                        if (OmniTerrain.chunks[downId].loaded)
                        {
                            down = OmniTerrain.chunks[downId].tileMap[x + (chunksize - 1) * chunksize];
                        }
                    }
                    else
                    {
                        downId = x2 + (OmniTerrain.Height+y2)* OmniTerrain.Width;
                        if (OmniTerrain.chunks.ContainsKey(downId))
                        if (OmniTerrain.chunks[downId].loaded)
                        {
                            down = OmniTerrain.chunks[downId].tileMap[x + (chunksize - 1) * chunksize];
                        }
                    }
                }
                

                if (up == -1)
                {

                    if (left == -1 && right != -1)
                    {
                        slant[i] = 1;
                    }
                    else if (right == -1 && left != -1)
                    {
                        slant[i] = 2;
                    }
                }
                else if (down == -1)
                {
                    if (left == -1 && right != -1)
                    {
                        slant[i] = 3;
                    }
                    else if (right == -1 && left != -1)
                    {
                        slant[i] = 4;
                    }
                }

                if (oldslant != slant[i])
                {
                    rebuildMesh = true;
                }

            }

        
    }

   public void GenSlant()
   {
        
        int x2 = (int)(pos.x)/chunksize;
        int y2 = (int)(pos.y)/chunksize;

        for (int i = 0; i < tileMap.Length; i++)
        {
            int x = i % chunksize;
            int y = i / chunksize;
            GenSlant(x, y);
        }

   }

    void checkTop()
    {
        int xx = (int)(pos.x / chunksize);
        int yy = (int)(pos.y / chunksize);
        //check the sides of other chunks if the exists (to fix border errors)
        if (yy + 1 < OmniTerrain.Height)
        {
        }
    }

    void GenBiomes(int size)
    {
        int y = (int)(pos.y / size);
        if (y > OmniTerrain.Height/2)
            biome = Biomes.Sky;
        else if (y < OmniTerrain.Height / 2)
            biome = Biomes.Underground;
        else
            biome = Biomes.Forest;


		switch (biome) {
		case Biomes.Sky:
			tileMap = BiomeManager.Fill(-1,size);
			break;
		case Biomes.Forest:
            int type = (int)(pos.x / size) % 2 == 1 ? 1 : -1;
			tileMap = BiomeManager.generateBiomeHills(size,size,2,type,OmniItems.getItem("Grass"),OmniItems.getItem("Dirt"),-1);
			break;
		case Biomes.Underground:
			tileMap = BiomeManager.generateCaves(size,size,OmniItems.getItem("Dirt"),4,40);
			break;
		case Biomes.CornerLeft:
			GenCorner (size,-1);
			break;
		case Biomes.CornerRight:
			GenCorner (size,1);
			break;
		default:
            tileMap = BiomeManager.Fill(-1, size);
			break;
		}


	}

	void GenCorner(int size, int dir) {
		int x = (int)pos.x/size;
		int y = (int)pos.y/size;

		tileMap = BiomeManager.generateCorner (size, dir,OmniItems.getItem("Grass"),OmniItems.getItem("Dirt"),-1);



	}

	public void GenMesh(int lod)
	{

        if (OmniNetwork.Dedicated)
            return;

        

        int size = chunksize;

        mesh[lod-1].Clear();
		vert = new Vector3[(size/lod) * (size/lod) * 4];
        uv = new Vector2[(size / lod) * (size / lod) * 4];
        tri = new int[(size / lod) * (size / lod) * 4 * 6];
        byte[] slanted = new byte[(size / lod) * (size / lod) * 4];	

		
		int indicies = 0;
		int verts = 0;


        for (int x = 0; x < (size / lod); x++)
        {
            for (int y = 0; y < (size / lod); y++)
            {

				int index = x+y*size;

                if (tileMap == null)
                    continue;

				if(tileMap[index] >= 0) {

                    TPAtlasTexture tex = OmniItems.itemTypes[tileMap[index]].animList[0].aTexture;

					float texX = tex.coords.x;
					float texY = tex.coords.y;
					float texWidth = tex.coords.x + tex.coords.width;
					float texHeight = tex.coords.y + tex.coords.height;

					

					uv[verts] = new Vector2(texX,texY);
                    uv[verts + 1] = new Vector2(texX, texHeight);
                    uv[verts + 2] = new Vector2(texWidth, texHeight);
                    uv[verts + 3] = new Vector2(texWidth, texY);

                    slanted[verts] = slant[index];

                    vert[verts++] = new Vector3(x, y, 0);
                    vert[verts++] = new Vector3(x, y + lod, 0);
                    vert[verts++] = new Vector3(x + lod, y + lod, 0);
                    vert[verts++] = new Vector3(x + lod, y, 0);
                        
                    
					
				}
		
			}
            
		}
		
		for (int i=0; i<verts; i+=4) {

            if (slanted[i] == 1)
            {
                tri[indicies++] = i;
                tri[indicies++] = i + 2;
                tri[indicies++] = i + 3;
                tri[indicies++] = i;
                tri[indicies++] = i + 2;
                tri[indicies++] = i + 3;
            }
            else if (slanted[i] == 2)
            {
                tri[indicies++] = i+3;
                tri[indicies++] = i;
                tri[indicies++] = i + 1;
                tri[indicies++] = i + 3;
                tri[indicies++] = i;
                tri[indicies++] = i + 1;
            }
            else if (slanted[i] == 3)
            {
                tri[indicies++] = i + 1;
                tri[indicies++] = i + 2;
                tri[indicies++] = i + 3;
                tri[indicies++] = i + 1;
                tri[indicies++] = i + 2;
                tri[indicies++] = i + 3;
            }
            else if (slanted[i] == 4)
            {
                tri[indicies++] = i;
                tri[indicies++] = i+1;
                tri[indicies++] = i + 2;
                tri[indicies++] = i;
                tri[indicies++] = i+1;
                tri[indicies++] = i + 2;
            }

            else
            {
                tri[indicies++] = i;
                tri[indicies++] = i + 1;
                tri[indicies++] = i + 2;
                tri[indicies++] = i + 2;
                tri[indicies++] = i + 3;
                tri[indicies++] = i;
            }
		}
        //int index2 = indicies / 4;

        mesh[lod - 1].vertices = vert;
        mesh[lod - 1].uv = uv;
        mesh[lod - 1].triangles = tri;

	}

    public void checkDamageMap(int id)
    {
        if (tileMap[id] < 0)
            return;
        if (damageMap[id] >= OmniItems.itemTypes[tileMap[id]].baseHP)
        {
            damageMap[id] = 0;
            tileMap[id] = -1;
            GenSlant();
            GenMesh(1);
        }
    }
	
	public void renderOld() {
	
		for (int x=0; x<chunksize; x++) {
			for (int y=0; y<chunksize; y++) {
				
				int index = x+y*chunksize;
				
				
				if(tileMap[index] >= 0) {
					//drawRect.x = pos.x + x;
					//drawRect.y = pos.y + y;
					Vector3 drawPos = new Vector3(pos.x + x, pos.y + y);
					//OmniGfxBatch.draw(drawPos,OmniItems.itemAnim[tileMap[index]].mesh);
				}
			}
		}
	
	}
}

