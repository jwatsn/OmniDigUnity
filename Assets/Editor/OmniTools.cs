
using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;
using System.Diagnostics;
using System.Threading;



[CustomEditor(typeof(OmniItemType),true)]
public class OmniTools : Editor
{

    /*
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {

        Process proc = new Process();
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.FileName = pathToBuiltProject;
        proc.StartInfo.EnvironmentVariables.Add("+connect", "localhost:7777");
        //
        proc.Start();
    }
     
    
     */
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

                OmniAtlas.checkTextureList();
        OmniAnimation[] anims = Component.FindObjectsOfType<OmniAnimation>();
        for (int i = 0; i < anims.Length; i++)
        {
            if (anims[i].texture == null)
            {
                anims[i].texture = OmniAtlas.textureList[anims[i].index];
            }
            if (anims[i].texture == "")
            {
                anims[i].texture = OmniAtlas.textureList[anims[i].index];
            }

            if (anims[i].texture != OmniAtlas.textureList[anims[i].index])
            {
                anims[i].index = OmniAtlas.getIndex(anims[i].texture);
            }
            if (anims[i].mountPoint.Length <= 0)
            {
                anims[i].mountPoint = new Vector3[] { new Vector3(0, 0, 0) };
            }
            int ind = anims[i].name.IndexOf("Image");
            if (ind > 0)
            {
                string itemName = anims[i].name.Substring(0,ind);
                GameObject o = GameObject.Find(itemName);

                if (o != null)
                {
                    if (anims[i].itemObject == null)
                        anims[i].itemObject = o;
                    else
                    {
                        if (anims[i].itemObject.name != itemName)
                            anims[i].name = itemName + "Image";
                    }
                    bool flag = false;
                    for (int j = 0; j < anims[i].skeletonRects.Length; j++)
                    {
                        if (anims[i].skeletonRects[j].name == "bounds")
                        {
                            
                            flag = true;
                        }
                    }

                    if (!flag)
                    {
                        AnimSkeleton[] sk = new AnimSkeleton[anims[i].skeletonRects.Length + 1];
                        sk[0] = new AnimSkeleton();
                        sk[0].name = "bounds";
                        sk[0].rotationPoint = new Vector3(0.5f, 0.5f, 0);
                        sk[0].bounds.width = anims[i].width;
                        sk[0].bounds.height = anims[i].height;
                        anims[i].skeletonRects.CopyTo(sk, 1);
                        anims[i].skeletonRects = sk;
                    }
                }
            }
            
        }

        OmniItemType[] it = Component.FindObjectsOfType<OmniItemType>();
        for (int i = 0; i < it.Length; i++)
        {
            if (it[i].itemImage == null)
                it[i].itemImage = GameObject.Find(it[i].name + "Image");
            else
                if (it[i].itemImage.name != it[i].name + "Image")
                    it[i].itemImage.name = it[i].name + "Image";

            
        }


    }
    
    


}