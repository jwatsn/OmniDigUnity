using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class OmniServerBrowser : OmniGUI
{
    Rect window;
    Vector2 scrollPos;
    public static bool show;
    //public KeyCode toggle = KeyCode.F8;
    HostData[] servers;
    Ping[] serverPings;
    bool connect;
    int connectSelection;
    Ping p;
    void Start()
    {
        scrollPos = Vector2.zero;
        window = new Rect(0, 0, 400, 300);
        window.x = (Screen.width / 2f) - (window.width / 2f);
        window.y = (Screen.height / 2f) - (window.height / 2f);
        serverPings = new Ping[100]; //100 max connections...for now


    }

    void Update()
    {
        if (Input.GetButtonDown("Server List"))
        {
            show = !show;
            if (show)
            {
                MasterServer.ClearHostList();
                MasterServer.RequestHostList("OmniDig");
            }
        }

        if (MasterServer.PollHostList().Length > 0)
        {
            servers = MasterServer.PollHostList();
            
            for(int i=0; i<servers.Length; i++)
            {
                if (serverPings[i] == null)
                {
                    serverPings[i] = new Ping(servers[i].ip[0]);
                }
            }
        }

        if (connect)
        {
            show = false;
            connect = false;
            OmniNetwork.Dedicated = false;
            Network.Connect(servers[connectSelection]);
        }
    }

    void OnGUI()
    {

        if(show)
        window = GUILayout.Window(123, window, browserGUI,"Server Browser");
    }
    void browserGUI(int windowID)
    {
        
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUILayout.BeginVertical();
        if(servers != null)
        for (int i = 0; i < servers.Length; i++)
        {

            string ping = "";

            if (serverPings[i] != null)
                if (serverPings[i].isDone)
                    ping += " Ping: " + serverPings[i].time;

            connect = GUILayout.Button(servers[i].gameName + " by: " + servers[i].comment +ping);
           if (connect)
               connectSelection = i;
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
    }
}
