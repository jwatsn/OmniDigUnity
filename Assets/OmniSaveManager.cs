using System;
using System.Collections.Generic;
using UnityEngine;

public class OmniSaveManager : MonoBehaviour
{
    List<WWW> saves = new List<WWW>();
    static OmniSaveManager instance;

    void Start()
    {
        instance = this;
    }

    public static void Save(ClientControllable obj)
    {
       
        WWWForm w = new WWWForm();
        w.AddField("guid", obj.guid);
        w.AddField("posx", ((int)obj.bounds.x));
        w.AddField("posy", ((int)obj.bounds.y));
        w.AddField("sid", SystemInfo.deviceUniqueIdentifier);
        WWW w2 = new WWW(OmniNetwork.url_save, w);

        if(!instance.saves.Contains(w2))
        {
            instance.saves.Add(w2);
        }

    }

    public void OmniUpdate()
    {
        for (int i = 0; i < saves.Count; i++)
        {
            if (saves[i].isDone)
            {
                Debug.Log(saves[i].text);
                saves.Remove(saves[i]);
            }
        }
    }

    public static void update()
    {
        if (instance != null)
            instance.OmniUpdate();
    }
}
