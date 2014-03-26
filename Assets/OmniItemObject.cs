using UnityEngine;
using System.Collections.Generic;


public class OmniItemObject
{
    public int id = -1;
    public int stack;
    public string name;
    public OmniItemType type;
    public Rect bounds;
    public Vector3 bPos;


//    OmniAnimation anim;
    List<Rect> r;

    public OmniItemObject(OmniItemType item, int amount)
    {
        this.name = item.gameObject.name;
        this.stack = amount;
        id = OmniItems.getItem(name);
        //anim = OmniItems.itemAnim[id];
        type = item;
        bPos = new Vector3();
        r = new List<Rect>();
    }

    public void OmniUpdate(ControllableObject owner, float delta)
    {

    }


    void collisionTest(ControllableObject owner)
    {

        r.Clear();
        int c1 = (int)((bounds.x + bounds.width) - bounds.x);
        int c2 = (int)((bounds.y + bounds.height) - bounds.y);

        for (int x = 0; x <= type.Size; x++)
            for (int y = 0; y <= type.Size; y++)
            {
                int c1x = (int)bounds.x + x;
                int c1y = (int)bounds.y + y;
                int id = x + y * (int)type.Size;
                if (OmniTerrain.isSolid(c1x, c1y))
                {
                    r.Add(new Rect(c1x, c1y, 1, 1));
                }
            }

        for (int i = 0; i < owner.activeChunks.Count; i++)
        {
            for (int j = 0; j < owner.activeChunks[i].objects.Count; j++)
            {

                if (owner.activeChunks[i].objects[j].id != owner.id)
                    r.Add(owner.activeChunks[i].objects[j].bounds);
            }
        }
    }
}
