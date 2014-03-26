
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OmniAnimation))]
public class OmniEditor : Editor {

    [HideInInspector]
    public int copyStep = 0;
    Component[] objs;
    FieldInfo toCopy;

	[MenuItem("GameObject/Create Other/OmniItem")]
	static void CreateItem() {
		GameObject obj = new GameObject ("Item");
		GameObject ItemGroup = GameObject.Find ("Items");
		obj.tag = "Item";
		obj.transform.parent = ItemGroup.transform;
        obj.AddComponent<OmniItemType>();

        GameObject obj2 = new GameObject("ItemImage");
        obj2.AddComponent<OmniAnimation>();
        GameObject ItemGroup2 = GameObject.Find("ItemImages");
        obj2.transform.parent = ItemGroup2.transform;
	}




    void OnValidateOld()
    {
        OmniAnimation anim = target as OmniAnimation;
        if (anim.texture != null)
        {
            int index = OmniAtlas.getIndex(anim.texture);
            if (index >= 0)
                anim.index = index;
        }
    }
    Vector2 scrollPos = new Vector2();
    void OnSceneGUI ()
    {
        
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(0,0,Screen.width,Screen.height));
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        if (copyStep == 0)
        {
            GUILayout.BeginVertical();
            if (GUILayout.Button("Copy Variable"))
            {
                copyStep++;               
            }
            GUILayout.EndVertical();
        }
        else if (copyStep == 1)
        {
            FieldInfo[] f = target.GetType().GetFields();

            for (int i = 0; i < f.Length; i++)
            {
                GUILayout.BeginVertical();
                if (GUILayout.Button(f[i].Name))
                {
                    toCopy = f[i];
                    Component obj = target as Component;
                    UnityEngine.Object[] o = GameObject.FindObjectsOfType(obj.GetType());
                    objs = o as Component[];
                    copyStep++;
                }
                GUILayout.EndVertical();
            }
        }
        else if (copyStep == 2)
        {
            for (int i = 0; i < objs.Length; i++)
            {
                GUILayout.BeginVertical();
                if (GUILayout.Button(objs[i].name))
                {
                    toCopy.SetValue(objs[i], toCopy.GetValue(target));
                    copyStep = 0;
                }
                GUILayout.EndVertical();
            }
        }

        GUILayout.EndScrollView();
           GUILayout.EndArea();
        Handles.EndGUI();
    }

   

	public override void OnInspectorGUI()
	{

		base.OnInspectorGUI();
        OmniAtlas.checkTextureList();
		OmniAnimation anim = target as OmniAnimation;
        if(anim.index >= 0)
        if (anim.texture != OmniAtlas.textureList[anim.index])
        {
            anim.index = OmniAtlas.getIndex(anim.texture);
        }
		Rect r = EditorGUILayout.BeginHorizontal();
		anim.index = EditorGUILayout.Popup("Texture:",
		                             anim.index, OmniAtlas.textureList, EditorStyles.popup);

        if(anim.index >= 0)
        anim.texture = OmniAtlas.textureList[anim.index];
        EditorGUILayout.EndHorizontal();

        //anim.texture = "";
	}
}
