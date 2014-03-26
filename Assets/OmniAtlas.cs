using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OmniAtlas : MonoBehaviour {

	public static string atlasName = "World";
	public TPAtlas atlas;
	public static string[] textureList;
	public static Material texture;

	void OnValidate() {
		atlas = TPackManager.getAtlas (atlasName);
		textureList = atlas.frameNames;
	}

	void OnEnable() {
		texture = Resources.Load ("WorldMaterial") as Material;
	}

	void Start () {

	}
	public void load() {
		atlas = TPackManager.getAtlas(atlasName);
	}
	// Update is called once per frame
	void Update () {
	
	}

    public static void checkTextureList()
    {
            textureList = TPackManager.getAtlas(atlasName).frameNames;
    }

    public static int getIndex(string name)
    {
        if (textureList == null)
        {

            textureList = TPackManager.getAtlas(atlasName).frameNames;
        }

        for(int i=0; i<textureList.Length; i++)
            if(textureList[i] == name)
                return i;

        return -1;
    }
}
