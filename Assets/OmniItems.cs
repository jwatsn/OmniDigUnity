using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OmniItems : MonoBehaviour {


	[HideInInspector] public static GameObject[] items;
    [HideInInspector] public static OmniItemType[] itemTypes;

	void OnEnable() {
			items = GameObject.FindGameObjectsWithTag ("Item");
//			itemAnim = new OmniAnimation[items.Length];
            itemTypes = new OmniItemType[items.Length];
			for (int i=0; i<items.Length; i++) {
//					itemAnim [i] = items [i].GetComponent<OmniAnimation> ();
                    OmniItemType type = items[i].GetComponent<OmniItemType>();
                    if (type != null)
                    {
                        itemTypes[i] = type;
                        type.id = i;
                    }
              
			}
		}

	void Start () {

	}
	public static int getItem(string name) {
		for (int i=0; i<items.Length; i++) {
			if(items[i].name == name)
				return i;
		}
        //failed the lookup so we're gonna match strings with everything and suggest the best match for my lazy brain

        int closestMatch = 9999;
        for (int i = 0; i < items.Length; i++)
        {
            int p = Compute(items[i].name, name);
            if (p < closestMatch)
            {
                closestMatch = i;
            }
        }

        if (closestMatch < items.Length)
            Debug.Log("Item lookup failed for: " + name + " did you mean " + items[closestMatch].name + "?");


		return -1;
	}

    public static int Compute(string s, string t)
    {
	int n = s.Length;
	int m = t.Length;
	int[,] d = new int[n + 1, m + 1];

	// Step 1
	if (n == 0)
	{
	    return m;
	}

	if (m == 0)
	{
	    return n;
	}

	// Step 2
	for (int i = 0; i <= n; d[i, 0] = i++)
	{
	}

	for (int j = 0; j <= m; d[0, j] = j++)
	{
	}

	// Step 3
	for (int i = 1; i <= n; i++)
	{
	    //Step 4
	    for (int j = 1; j <= m; j++)
	    {
		// Step 5
		int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

		// Step 6
		d[i, j] = Mathf.Min(
		    Mathf.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
		    d[i - 1, j - 1] + cost);
	    }
	}
	// Step 7
	return d[n, m];
    }
    

	
	// Update is called once per frame
	void Update () {
	
	}
}
