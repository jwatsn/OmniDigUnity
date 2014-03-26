
using System.Collections.Generic;
using UnityEngine;

public class GUICreateWorld : OmniGUI
{
    Rect window;
    public static bool show;
    public KeyCode toggle = KeyCode.Escape;
    string worldName;
    string characterName;
    bool create;

    void Start()
    {
        window = new Rect(0, 0, Screen.width, 0);
        worldName = "World" + (int)(Random.value * 5000);
        characterName = "Bob" + (int)(Random.value * 5000);
    }

    void Update()
    {
        if(!OmniChatBox.show)
            if(Input.GetKeyDown(toggle))
                show = !show;

        if (show)
        {
            window.x = Screen.width / 2 - window.width / 2;
            window.y = Screen.height / 2 - window.height / 2;

            if (create)
            {
                show = false;
                create = false;
                OmniWorld.worldName = worldName;
                OmniLocal.localName = characterName;
                OmniWorld.instance.StartWorld();
            }

        }
    }

    void OnGUI()
    {
        if(!OmniServerBrowser.show)
        if (show)
        {
            window = GUILayout.Window(123, window, createWorldWindow, "Create Character");
        }
    }

    void createWorldWindow(int windowID)
    {
        GUILayout.ExpandHeight(true);
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Player name:");
        characterName = GUILayout.TextField(characterName);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("World name:");
        worldName = GUILayout.TextField(worldName);
        GUILayout.EndHorizontal();
        create = GUILayout.Button("Create world");
        GUILayout.EndVertical();
    }

}
