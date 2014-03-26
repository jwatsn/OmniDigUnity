using UnityEngine;

public class RChunkEvent : NetworkEvent
{
    public int x,y;
    
    //SERVER VALUES
    public int[] tileMap;
    public int[] damageMap;
//    public int[] damageMap;


    public RChunkEvent(int tick)
        : base(tick)
    {
            type = NetworkSendType.ClientReliable;
    }

    public override void init()
    {
        if (Network.isServer)
        {
            if (!OmniTerrain.chunks.ContainsKey(x + y * OmniTerrain.Width))
                OmniTerrain.chunks[x + y * OmniTerrain.Width] = new TerrainChunk(OmniTerrain.instance, x, y, OmniTerrain.chunkSize);

            tileMap = OmniTerrain.chunks[x + y * OmniTerrain.Width].tileMap;
            damageMap = OmniTerrain.chunks[x + y * OmniTerrain.Width].damageMap;
        }
        
        base.init(); //always call this after network values are set
    }

    public override void handle(int tick)
    {
        base.handle(tick);
        if (handled)
        {



            if (tileMap == null || damageMap == null || !fromServer)
                return;

            

            OmniTerrain.chunks[x + y * OmniTerrain.Width].damageMap = new int[damageMap.Length];
            OmniTerrain.chunks[x + y * OmniTerrain.Width].tileMap = new int[tileMap.Length];
           
            
            for (int i = 0; i < tileMap.Length; i++) //assume damageMap and tileMap are the same length
            {
                OmniTerrain.chunks[x + y * OmniTerrain.Width].tileMap[i] = tileMap[i];
                OmniTerrain.chunks[x + y * OmniTerrain.Width].damageMap[i] = damageMap[i];
            }
            OmniTerrain.chunks[x + y * OmniTerrain.Width].loaded = true;
            OmniTerrain.chunks[x + y * OmniTerrain.Width].GenSlant();
            OmniTerrain.chunks[x + y * OmniTerrain.Width].GenMesh(1);
            
        }
    } 
}