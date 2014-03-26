using UnityEngine;
using System.Collections;

public class OmniInterpolate : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
    }
    static Vector3 lerp = new Vector3();
    public static void InterpolatePos(OmniObject owner)
    {

        int chX = (int)(owner.lastPos.x / OmniTerrain.chunkSize);
        int chX2 = (int)(owner.bounds.x / OmniTerrain.chunkSize);

        int chY = (int)(owner.lastPos.y / OmniTerrain.chunkSize);
        int chY2 = (int)(owner.bounds.y / OmniTerrain.chunkSize);

        if (chX == 0 && chX2 == OmniTerrain.Width - 1) // lerp over the boundary
        {        
            owner.lastPos.x = (OmniTerrain.Width * OmniTerrain.chunkSize) + owner.lastPos.x;
        }

        else if (chX == OmniTerrain.Width - 1 && chX2 == 0) // lerp over the boundary
        {           
            owner.lastPos.x = owner.lastPos.x - (OmniTerrain.Width * OmniTerrain.chunkSize);
        }


        if (chY == 0 && chY2 == OmniTerrain.Height - 1) // lerp over the boundary
        {
            owner.lastPos.y = (OmniTerrain.Height * OmniTerrain.chunkSize) + owner.lastPos.y;
        }

        else if (chY == OmniTerrain.Height - 1 && chY2 == 0) // lerp over the boundary
        {
            owner.lastPos.y = owner.lastPos.y - (OmniTerrain.Height * OmniTerrain.chunkSize);
        }

        lerp.Set(owner.bounds.x, owner.bounds.y, 0);
        float l = OmniWorld.updateCounter / OmniWorld.timeStep;
        Vector3 result = Vector3.Lerp(owner.lastPos, lerp, l);        
        owner.Position = result;

    }
    // Update is called once per frame
    void Update()
    {

    }
}
