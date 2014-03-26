using UnityEngine;
using System.Collections;
using System.Net;

public class OmniCreateCharacterNet : OmniGUI
{


    HostData[] servers;
    Rect window;
    Rect createCharButton;
    Rect delCharButton;
    public static bool show;
    bool last_show;
    public KeyCode toggle = KeyCode.Escape;
    string worldName;
    string characterName;
    bool create;
    int createStep = 0;
    bool login;
    WWW getCharPost;
    WWW createCharPost;
    WWW delCharPost;
    GUIStyle textStyle;

    void Start()
    {
        window = new Rect(0, 0, Screen.width, Screen.height);
        createCharButton = new Rect(0, 0, Screen.width/2, 20);
        delCharButton = new Rect(0, 0, 100, 20);
        
        characterName = "Bob" + (int)(UnityEngine.Random.value * 5000);


    }

    void Update()
    {

        if (last_show != show)
        {
            MasterServer.ClearHostList();
            MasterServer.RequestHostList("OmniDigMMO");

            getCharPost = new WWW(OmniNetwork.url_view + "guid=" + SystemInfo.deviceUniqueIdentifier);
            last_show = show;

            if (show)
            {
                OmniGUIManager.DisableAllExcept(this);
            }
            else
            {
                OmniGUIManager.EnableAllExcept();
            }
        }

        if (show)
        {
            if (MasterServer.PollHostList().Length > 0)
            {
                servers = MasterServer.PollHostList();
            }

            window.x = Screen.width / 2 - window.width / 2;
            window.y = Screen.height / 2 - window.height / 2;

            if (createCharPost != null)
                if (createCharPost.isDone)
                {
                    getCharPost = new WWW(OmniNetwork.url_view + "guid=" + SystemInfo.deviceUniqueIdentifier);
                    createCharPost = null;
                }

            if(getCharPost !=  null)
                if (getCharPost.isDone)
                {
                    if (getCharPost.text == "not_found")
                        create = true;
                    else if (getCharPost.text != "err")
                    {
                        characterName = getCharPost.text;
                        login = true;
                    }

                    getCharPost = null;
                }
            if(delCharPost != null)
                if (delCharPost.isDone)
                {

                    getCharPost = new WWW(OmniNetwork.url_view + "guid=" + SystemInfo.deviceUniqueIdentifier);
                    delCharPost = null;
                }


            if (create && createStep == 1)
                if (Input.GetKeyDown(KeyCode.Return))
                {

                    createStep = 0;
                    if (characterName.Length > 0)
                    {
                        create = false;
                        createCharPost = new WWW(OmniNetwork.url_add + "pName=" + WWW.EscapeURL(characterName) + "&guid=" + WWW.EscapeURL(SystemInfo.deviceUniqueIdentifier));
                    }

                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                    createStep = 0;
        }
    }

    void OnGUI()
    {
        if (!OmniServerBrowser.show)
            if (show)
            {
                if (textStyle == null)
                {
                    textStyle = new GUIStyle(GUI.skin.textField);
                    textStyle.alignment = TextAnchor.MiddleCenter;
                }

                GUI.FocusWindow(321);
                if(create && createStep == 1)
                GUI.FocusControl("inputName");
                window = GUI.Window(321, window, createWorldWindow, "Create Character");
            }
    }

    void createWorldWindow(int windowID)
    {
        if (login)
        {

            float height = createCharButton.height;
            createCharButton.x = Screen.width / 2 - createCharButton.width / 2;
            createCharButton.y = Screen.height / 2 - height / 2;

            delCharButton.x = Screen.width / 2 - delCharButton.width / 2;
            delCharButton.y = Screen.height / 2 + height / 2;

            if (GUI.Button(createCharButton, characterName))
            {
                if(servers != null)
                if (servers.Length > 0)
                {
                    Network.Connect(servers[0]);
                    login = false;
                }
            }
            if(GUI.Button(delCharButton, "Delete"))
            {
                delCharPost = new WWW(OmniNetwork.url_del + "guid=" + WWW.EscapeURL(SystemInfo.deviceUniqueIdentifier));
                login = false;
            }
        }
        else if (create)
        {
            createCharButton.x = Screen.width / 2 - createCharButton.width / 2;
            createCharButton.y = Screen.height / 2 - createCharButton.height / 2;
            if (createStep == 0)
            {
                if (GUI.Button(createCharButton, "Create Character"))
                {   
                    createStep = 1;
                }
            }
            else if (createStep == 1)
            {
                GUI.SetNextControlName("inputName");
                characterName = GUI.TextField(createCharButton, characterName, textStyle);
            }

        }
        
    }

}
