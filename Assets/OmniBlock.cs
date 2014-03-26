using UnityEngine;
using System.Collections;

public class OmniBlock : OmniItemType {

    public override void OnUse(ControllableObject player, Vector3 pos)
    {
        
        int x2 = (int)(pos.x) / OmniTerrain.chunkSize;
        int y2 = (int)(pos.y) / OmniTerrain.chunkSize;
        int x = (int)(pos.x) % OmniTerrain.chunkSize;
        int y = (int)(pos.y) % OmniTerrain.chunkSize;

        if(OmniTerrain.chunks.ContainsKey(x2 + y2 * OmniTerrain.Width))
        {
        TerrainChunk ch = OmniTerrain.chunks[x2 + y2 * OmniTerrain.Width];
            if (ch.tileMap[x + y * OmniTerrain.chunkSize] == -1)
            {
                terrainAddBlock t = new terrainAddBlock(player, this, (int)pos.x, (int)pos.y);
                OmniTerrain.terrainUpdates.Add(t);
            }
           
        }
    }

}
