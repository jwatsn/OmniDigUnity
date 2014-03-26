using UnityEngine;
using System.Collections;
using System.Reflection;

public class OmniGUIManager : MonoBehaviour {

    public static OmniGUI[] GUIs;

	// Use this for initialization
	void Start () {

        GUIs = GetComponents<OmniGUI>();

	}

    public static void DisableAllExcept(params OmniGUI[] component)
    {
        for (int i = 0; i < GUIs.Length; i++)
        {
            bool flag = false;
            for (int j = 0; j < component.Length; j++)
            {
                if (GUIs[i] == component[j])
                    flag = true;
            }

            if (!flag)
                GUIs[i].enabled = false;
        }
    }

    public static void EnableAllExcept(params OmniGUI[] component)
    {
        for (int i = 0; i < GUIs.Length; i++)
        {
            bool flag = false;
            for (int j = 0; j < component.Length; j++)
            {
                if (GUIs[i] == component[j])
                    flag = true;
            }

            if (!flag)
                GUIs[i].enabled = true;
        }
    }

    public static void CloseAllGUIs()
    {
        for (int i = 0; i < GUIs.Length; i++)
        {
            FieldInfo f = GUIs[i].GetType().GetField("show");
            if (f != null)
                f.SetValue(null, false);
        }
    }

    // Update is called once per frame
    void Update()
    {
	
	}
}
