using UnityEngine;
using System.Collections;

public class OmniDataBlocks : MonoBehaviour {


	GameObject[] DataBlocks;
	static OmniDataBlocks instance;
	// Use this for initialization
	void OnEnable() {
		instance = this;
		
		DataBlocks = GameObject.FindGameObjectsWithTag("DataBlock");
		for(int i=0; i<DataBlocks.Length; i++) {
			DataBlocks[i].SetActive(false);
		}
	
	}
	
	public static GameObject getDataBlock(string name) {
		for(int i=0; i<instance.DataBlocks.Length; i++) {
			if(name == instance.DataBlocks[i].name)
				return instance.DataBlocks[i];
		}
		return null;
	}

    public static GameObject getDataBlock(int id)
    {
        return instance.DataBlocks[id];
    }

}
