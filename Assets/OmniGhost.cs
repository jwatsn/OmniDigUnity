using UnityEngine;
using System.Collections;

public class OmniGhost
{
    


    static Vector3 lerp = new Vector3();
    public static void InterpolatePos(GhostObject owner)
    {

     
        float l2 = owner.mount0interp / OmniWorld.networkRate; 
        float l3 = owner.mount1interp / OmniWorld.networkRate;
        float l5 = owner.mount2interp / OmniWorld.networkRate; 
        float l4 = owner.rotCounter / OmniWorld.networkRate;

        if (l2 > 1)
            l2 = 1;
        if (l3 > 1)
            l3 = 1;
        if (l4 > 1)
            l4 = 1;
        if (l5 > 1)
            l5 = 1;


        float mount0lerp = Mathf.Lerp(owner.mount0ghostLast, owner.mount0ghostTime, l2);
        float mount1lerp = Mathf.Lerp(owner.mount1ghostLast, owner.mount0ghostTime, l3);
        float mount2lerp = Mathf.Lerp(owner.mount2ghostLast, owner.mount2ghostTime, l5);
        float rotlerp = Mathf.Lerp(owner.lastRot, owner.currentRot, l4);

        owner.owner.mount0time = mount0lerp;
        owner.owner.mount1time = mount1lerp;
        owner.owner.mount2time = mount2lerp;
        owner.owner.rotation = rotlerp;

        if (owner.currentRot == 0)
            owner.owner.rotation = 0;

        if (owner.owner.item.animList[0].animationType == OmniAnimation.Type.Skeleton)
        {
            for (int i = 0; i < OmniAnimation.Max_Skeletons; i++)
            {
                float l6 = owner.skelCounter[i] / OmniWorld.networkRate;
                if (l6 > 1)
                    l6 = 1;

                float skelLerp = Mathf.LerpAngle(owner.skelLast[i], owner.skelInterp[i], l6);
                if (i < owner.owner.skeleton.Length)
                    owner.owner.skeleton[i].rotation = skelLerp;



            }
        }

    }
}
