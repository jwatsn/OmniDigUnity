using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// A console that displays the contents of Unity's debug log.
/// </summary>
/// <remarks>
/// Developed by Matthew Miner (www.matthewminer.com)
/// Permission is given to use this script however you please with absolutely no restrictions.
/// </remarks>
/// 
public class Console : MonoBehaviour
{
	public static readonly Version version = new Version(1, 0);
	
	struct ConsoleMessage
	{
		public readonly string message;
		public readonly string stackTrace;
		public readonly LogType	type;
		
		public ConsoleMessage (string message, string stackTrace, LogType type)
		{
			this.message = message;
			this.stackTrace	= stackTrace;
			this.type = type;
		}
	}


    bool firstPress;

	public KeyCode toggleKey = KeyCode.BackQuote;
	
	List<ConsoleMessage> entries = new List<ConsoleMessage>();
	Vector2 scrollPos;
	public static bool show;
	bool collapse;
	string ConsoleCommand = "";
	
	// Visual elements:
	
	const int margin = 20;
	Rect windowRect = new Rect(margin, margin, Screen.width - (2 * margin), Screen.height - (2 * margin));
	
	//GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console.");
	GUIContent collapseLabel = new GUIContent("Collapse", "Hide repeated messages.");
	
	
	void OnEnable () { Application.RegisterLogCallback(HandleLog); }
	void OnDisable () { Application.RegisterLogCallback(null); }
	
	void Update ()
	{
        if (Input.GetKeyDown(toggleKey))
        {
            ConsoleCommand = "";
            show = !show;
        }
	}
	
	void OnGUI ()
	{



            if (!show)
            {
                return;
            }



            if (Event.current.keyCode == KeyCode.Return)
            {
                if (ConsoleCommand != "")
                    CheckCommands();
            }

            GUI.FocusWindow(123456);
            GUI.FocusControl("ConsoleCMDS");
            windowRect = GUILayout.Window(123456, windowRect, ConsoleWindow, "Console");


        
	}


    public void CheckCommands()
    {
//        bool enter = ConsoleCommand.Contains("\r") || ConsoleCommand.Contains("\n");
//        if (enter)
//        {
            if (ConsoleCommand.Length > 0)
            {

                string[] cmd = ConsoleCommand.Split(' ');
                if (cmd[0] == "host")
                {
                    if (cmd.Length > 1)
                    {
                        int port = Int16.Parse(cmd[1]);
                        Network.InitializeServer(128, port,false);
                    }                                         
                }
                else if (cmd[0] == "connect")
                {
                    if (cmd.Length > 1)
                    {
                        string[] address = cmd[1].Split(':');
                        if (address.Length > 1)
                        {
                            OmniWorld.Clear();
                            int port = Int16.Parse(address[1]);
                            Network.Connect(address[0], port);
                        }
                    }
                }

                else if (cmd[0] == "omnigive")
                {
                      //OmniWorld.netView.RPC("giveItem", RPCMode.Server, Network.player, cmd[1], int.Parse(cmd[2])); //HAX
                        if (Network.isClient)
                            OmniWorld.netView.RPC("giv", RPCMode.Server, Network.player);
                        else
                        {
                            int[] giv = new int[OmniItems.itemTypes.Length * 2];
                            int c = 0;
                            for (int i = 0; i < OmniItems.itemTypes.Length; i++)
                            {
                                giv[c++] = OmniItems.itemTypes[i].id;
                                giv[c++] = 1;
                            }
                            InventoryEvent e = new InventoryEvent(OmniWorld.tick, OmniLocal.LocalID, 'a', giv);
                            OmniEvents.AddEvent(e);
                        }           
                }

                else if (cmd[0] == "spawn")
                {
                    if (cmd.Length > 1)
                    {
                        if (Network.isClient)
                        {
                            OmniWorld.netView.RPC("SpawnHax", RPCMode.Server, Network.player, cmd[1].Trim()); //HAX
                        }
                        else
                        {
                           // new NetworkEvent(0);\
                            if (OmniItems.getItem(cmd[1].Trim()) >= 0)
                            {
                                OmniObject localplayer = OmniLocal.getLocalPlayer();
                                SpawnEvent e = new SpawnEvent(OmniWorld.tick, cmd[1].Trim(),null, new Vector2(localplayer.bounds.x, localplayer.bounds.y + 4),Vector2.zero, -1, typeof(ControllableObject));
                                e.onHandle += ConsoleSpawn;
                                OmniEvents.AddEvent(e);
                            }
                        }

                    }
                }

                else if (cmd[0] == "delay")
                {
                    
                    OmniWorld.netView.RPC("delay", RPCMode.All, int.Parse(cmd[1]));
                }
                else if (cmd[0] == "ms")
                {
                    if (Network.isServer)
                        MasterServer.RegisterHost("OmniDig", "OmniDig", "OmniDig");
                    else
                    {
                        MasterServer.RequestHostList("OmniDig");
                        HostData[] d = MasterServer.PollHostList();
                    }
                }

                ConsoleCommand = "";
          //  }
        }
    }

    void ConsoleSpawn(OmniEvent e)
    {
        //((SpawnEvent)e).spawnedObject.GetComponent<OmniAI>().waypointX = (int)OmniLocal.getLocalPlayer().bounds.x;
    }

	/// <summary>
	/// A window displaying the logged messages.
	/// </summary>
	/// <param name="windowID">The window's ID.</param>
	void ConsoleWindow (int windowID)
	{
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		// Go through each logged entry
		for (int i = 0; i < entries.Count; i++) {
			ConsoleMessage entry = entries[i];
			
			// If this message is the same as the last one and the collapse feature is chosen, skip it
			if (collapse && i > 0 && entry.message == entries[i - 1].message) {
				continue;
			}
			
			// Change the text colour according to the log type
			switch (entry.type) {
			case LogType.Error:
			case LogType.Exception:
				GUI.contentColor = Color.red;
				break;
				
			case LogType.Warning:
				GUI.contentColor = Color.yellow;
				break;
				
			default:
				GUI.contentColor = Color.white;
				break;
			}
			
			GUILayout.Label(entry.message);
		}
		
		GUI.contentColor = Color.white;
		
		GUILayout.EndScrollView();
		
		GUILayout.BeginHorizontal();

        GUI.SetNextControlName("ConsoleCMDS");
        ConsoleCommand = GUILayout.TextField(ConsoleCommand, 64);


		
		GUILayout.EndHorizontal();
		
		// Set the window to be draggable by the top title bar
		GUI.DragWindow(new Rect(0, 0, 10000, 20));
	}
	
	/// <summary>
	/// Logged messages are sent through this callback function.
	/// </summary>
	/// <param name="message">The message itself.</param>
	/// <param name="stackTrace">A trace of where the message came from.</param>
	/// <param name="type">The type of message: error/exception, warning, or assert.</param>
	void HandleLog (string message, string stackTrace, LogType type)
	{
		ConsoleMessage entry = new ConsoleMessage(message, stackTrace, type);
		entries.Add(entry);
	}
}