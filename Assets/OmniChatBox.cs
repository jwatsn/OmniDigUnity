using UnityEngine;
using System.Collections.Generic;

public class chatMessage
{
    
    public readonly int id;
    public readonly string msg;
    public chatMessage(int id, string msg)
    {
        this.id = id;
        this.msg = msg;
    }
}

public class activeMessage
{
    public float timer = 4f;
    public chatMessage msg;
    public GUIContent g;
    public Vector2 b;
    public Rect bounds;
 
    public activeMessage(chatMessage msg)
    {
        g = new GUIContent(msg.msg);
        this.msg = msg;
        bounds = new Rect();
    }
}

public class OmniChatBox : OmniGUI {


    static OmniChatBox instance;
    public KeyCode toggleKey = KeyCode.Return;
    Rect windowRect;
    Rect tfRect;
    Rect textRect;
    public static bool show;
    public static bool showInput;
    bool firstPress;
    string chatString = "";
    Vector2 scrollPosition;
    List<chatMessage> log;
    List<activeMessage> activeChat;
    float timer = -2;
    float charWidth;

	// Use this for initialization
	void Start () {
        
        instance = this;
        float y = Screen.height - OmniQuickBar.QuickBar_PaddingY - OmniQuickBar.QuickBar_SlotSpace - OmniQuickBar.QuickBar_Y - OmniQuickBar.HealthBar_Height * 2-100;

        windowRect = new Rect(10,y,300,150);
        tfRect = new Rect(3, 76, windowRect.width-6, 20);
        textRect = new Rect(3, 76, windowRect.width-6, 20);
        log = new List<chatMessage>();
        activeChat = new List<activeMessage>();
        
        Input.eatKeyPressOnTextFieldFocus = false;
	}
	
	// Update is called once per frame
	void Update () {

        if (!Console.show)
        {
            if (Input.GetKeyDown(toggleKey))
            {
                if (show)
                {
                    checkMessage();

                    if (!showInput)
                    {
                        showInput = true;
                        timer = -2;
                        return;
                    }

                }

                show = !show;

                timer = -2;
                showInput = show;

                chatString = "";
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
            show = false;


        if (!showInput)
        {
            if (timer != -2)
            {
                timer += Time.deltaTime;
                if (timer >= 5)
                {
                    show = false;
                    timer = -2;
                }

            }
        }
        for (int i = 0; i < activeChat.Count; i++)
        {
            activeChat[i].timer -= Time.deltaTime;
            OmniObject owner = OmniWorld.instance.SpawnedObjectsNew[activeChat[i].msg.id];
            if (owner == null)
            {
                activeChat[i].timer = 0;
            }
            else
            {
                Vector3 p = owner.position;
                p.x += owner.item.Size / 2f;
                p.y += owner.item.Size;
                p = camera.WorldToScreenPoint(p);
                p.y += activeChat[i].b.y + 4;
                activeChat[i].bounds.x = p.x - activeChat[i].b.x/2;
                activeChat[i].bounds.y = Screen.height - p.y;
                activeChat[i].bounds.width = activeChat[i].b.x;
                activeChat[i].bounds.height = activeChat[i].b.y;
            }
            if (activeChat[i].timer <= 0)
                activeChat.RemoveAt(i);
        }


        
	}

    void updateTimers(float delta)
    {

    }

    public static void OmniUpdate(float delta)
    {
        
    }
    void updateActiveMsgs(chatMessage msg)
    {
        for (int i = 0; i < activeChat.Count; i++)
        {
            if (activeChat[i].msg.id == msg.id)
                activeChat.RemoveAt(i);
        }
        activeChat.Add(new activeMessage(msg));
    }
    public static void Add(chatMessage msg)
    {
        if (OmniNetwork.Dedicated)
            return;

        instance.log.Add(msg);
        instance.updateActiveMsgs(msg);
        instance.scrollPosition.y = 100 * instance.log.Count;
        instance.timer = 0;
        show = true;
        showInput = false;

    }

    void checkMessage()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (chatString != "")
            {
                ChatEvent e = new ChatEvent(OmniWorld.tick, OmniLocal.LocalID, chatString);
                e.player = Network.player;
                OmniEvents.AddEvent(e);
                chatString = "";
            }
        }
    }

    void OnGUI()
    {

        for (int i = 0; i < activeChat.Count; i++)
        {

            activeChat[i].b = GUI.skin.box.CalcSize(activeChat[i].g);


            GUI.Box(activeChat[i].bounds, activeChat[i].msg.msg);
        }

            if (show)
            {

                GUI.Window(1234, windowRect, chatWindow, "");
                if (showInput)
                {
                    GUI.FocusWindow(1234);
                    GUI.FocusControl("ChatInput");
                }
            }
        
    }

    void sendMessage()
    {
    }

    void chatWindow(int windowId)
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < log.Count; i++)
        {
            GUILayout.Label(OmniWorld.instance.SpawnedObjectsNew[log[i].id].Name + ": "+log[i].msg);        
        }
        GUILayout.EndScrollView();
        GUILayout.BeginHorizontal();
        if (showInput)
        {
            GUI.SetNextControlName("ChatInput");
            chatString = GUILayout.TextField(chatString);
        }
        GUILayout.EndHorizontal();
    }
}
