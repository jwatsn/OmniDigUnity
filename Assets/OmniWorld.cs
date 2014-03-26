using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class OmniWorld : MonoBehaviour {


	GameObject obj;
	OmniObject player;
	OmniTerrain terrain;
	Vector2 spawn;

    public static bool isDebugging = false;

    public bool MakeDedicated;
    public static int localDelay = 2;
    OmniEvents eventManager;
    OmniNetwork networkManager;
    OmniLocal localManager;
    public List<OmniObject> SpawnedObjectsNew;
    public List<Int32> openSlots;
    public static OmniWorld instance;
    public static List<Vector2> SpawnPoints;
    public static NetworkView netView;
	public static int tick = 0;
    public static string worldName = "World";
    public static Rigidbody2D physTest;
    public static bool RegMaster;
    public static float timeStep = 0.032f;
    public static float updateCounter = 0f;
    public float networkCounter = 0f;
    public static float networkRate = 0.06f;

    bool masterServerFlag;


    void OnEnable()
    {


        Network.natFacilitatorIP = "jessewatson.org";
        MasterServer.ipAddress = "jessewatson.org";
        Application.runInBackground = true;
        instance = this;
        SpawnPoints = new List<Vector2>();
        SpawnedObjectsNew = new List<OmniObject>();
        openSlots = new List<int>();
        eventManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<OmniEvents>();
        networkManager = GetComponent<OmniNetwork>();
        localManager = GetComponent<OmniLocal>();
        physTest = GetComponent<Rigidbody2D>();
        netView = networkManager.networkView;

    }

    void OnStart()
    {
        particleEmitter.enabled = true;
        particleEmitter.emit = true;
      
    }


    public static List<OmniObject> getSpawnedObjs()
    {
        return instance.SpawnedObjectsNew;
    }


    void Start()
    {
        terrain = GetComponent<OmniTerrain>();

        GUICreateWorld.show = true;
        bool dedicated = false;
/*
//        if (Application.platform != RuntimePlatform.WindowsWebPlayer)// && Application.platform != RuntimePlatform.WindowsEditor)
 //       {
            
            string[] args = Environment.GetCommandLineArgs();
            if (args != null)
                for (int i = 0; i < args.Length; i++)
                {

                    if (args[i] == "-dedicated")
                    {
                        if (i < args.Length - 2)
                        {

                            worldName = args[i + 1];
                            OmniLocal.localName = args[i + 2];

                        }
                        dedicated = true;

                    }
                }
//        }
            /*
        else
        {
            OmniCreateCharacterNet.show = true;
        }
             * */
        if (MakeDedicated)
            dedicated = true;

        if (dedicated)
        {
           
            if (worldName == "")
                worldName = "OmniWorld";
            if (OmniLocal.localName == "")
                OmniLocal.localName = "Player";
            OmniNetwork.Dedicated = true;
            StartWorld();          
            GUICreateWorld.show = false;
        }
    }

    public void StartWorld()
    {
         ClearWorld();

         if (OmniNetwork.Dedicated)
             Network.InitializeServer(128, 7777, false);

             


         if (!OmniNetwork.Dedicated)
         {
  
             SpawnEvent e = new SpawnEvent(OmniWorld.tick + 2, "Bob","LocalPlayer", getSpawnPoint(), Vector2.zero, -1, typeof(ClientControllable));
             OmniLocal.LocalID = OmniWorld.GetSpawnID();
             OmniInventory.giveall(OmniLocal.LocalID);
             OmniEvents.AddEvent(e);
         }
         else
         {
             MasterServer.RegisterHost("OmniDigMMO", worldName, OmniLocal.localName);
         }
            
        
	}
    bool flag;
    void OnGUI()
    {
        /*
        if (!Network.isServer)
        {
            HostData[] d = MasterServer.PollHostList();
            if (d.Length > 0)
            {
                if (!flag)
                {
                    Debug.Log(d[0]);
                    flag = true;
                }
            }
        }
         * */
    }

    public static Vector2 getSpawnPoint()
    {
       Vector2 SpawnPoints = new Vector2(OmniTerrain.Width / 2, (OmniTerrain.Height / 2)+0.2f);

        return new Vector2(SpawnPoints.x * OmniTerrain.chunkSize + OmniTerrain.chunkSize / 2, SpawnPoints.y * OmniTerrain.chunkSize + OmniTerrain.chunkSize);
    }

    void SetUpSpawnList()
    {

    }

    public void ClearWorld()
    {
        if(OmniTerrain.loadedChunks != null)
        OmniTerrain.loadedChunks.Clear();
        if (OmniTerrain.activeChunks != null)
        OmniTerrain.activeChunks.Clear();
        if (SpawnedObjectsNew != null)
        SpawnedObjectsNew.Clear();
        OmniLocal.LocalID = -1;
        if (OmniLocal.instance != null)
        OmniLocal.instance.owner = null;
        if(OmniTerrain.chunks != null)
            for (int i = 0; i < OmniTerrain.loadedChunks.Count; i++)
        {
            OmniTerrain.chunks.Remove(i);
        }

        if (OmniCreateCharacterNet.show)
            OmniCreateCharacterNet.show = false;
        if (GUICreateWorld.show)
            GUICreateWorld.show = false;
    }

    public static void Clear()
    {
        instance.ClearWorld();
    }


    public static void AddToSpawnListNew(OmniObject o, int id)
    {


            if (id < 0)
            {              
                return;
            }
            else
            {
                o.id = id;
                if (id >= instance.SpawnedObjectsNew.Count-1)
                {
                    for (int i = 0; i <= (id - instance.SpawnedObjectsNew.Count)+3; i++)
                    {

                        instance.SpawnedObjectsNew.Add(null);
                    }
                }

                    if (instance.SpawnedObjectsNew[id] != null)
                        instance.SpawnedObjectsNew[id].destroy(true);
                    instance.SpawnedObjectsNew[id] = o;
            }
            o.spawned = true;
            o.item.Spawned(o);
        

    }

    public static int GetSpawnID()
    {
        for (int i = 0; i < instance.openSlots.Count; i++)
        {
            if (instance.SpawnedObjectsNew[instance.openSlots[i]] == null)
                return instance.openSlots[i];
            else
                instance.openSlots.Remove(instance.SpawnedObjectsNew[instance.openSlots[i]].id);
        }

        return instance.SpawnedObjectsNew.Count;
    }

    void Update()
    {

        if (terrain == null)
            return;

        if (RegMaster)
        {
            RegMaster = false;
            MasterServer.RegisterHost("OmniDig", OmniWorld.worldName, OmniLocal.localName);
        }

        if(Input.GetButtonDown("Host Server"))
            Network.InitializeServer(128, 0, true);

        OmniLocal.UpdateCamera();
        //particleEmitter.Simulate(Time.deltaTime);
        
        while (updateCounter >= timeStep)
        {
            
            eventManager.OmniUpdate(timeStep);
            localManager.OmniUpdate(timeStep);
            
            OmniChatBox.OmniUpdate(timeStep);
            
            for (int i = 0; i < SpawnedObjectsNew.Count; i++)
            {
                if (SpawnedObjectsNew[i] == null)
                    continue;
                if (SpawnedObjectsNew[i].id != i)
                {
                    Debug.Log("Error: please report to Omnicide");
                }
                SpawnedObjectsNew[i].OmniUpdate(timeStep);

            }
            terrain.OmniUpdate(timeStep);
            localManager.CheckActiveChunks();
            OmniSaveManager.update();

            tick++;
            updateCounter -= timeStep;


        }
        
        if (!OmniNetwork.Dedicated)
        {
            
            terrain.draw();
            
        }

        updateCounter += Time.deltaTime;
        networkCounter += Time.deltaTime;
    }

    /*
    public static void RPC(string name, params object[] args)
    {
        
        instance.networkManager.networkView.RPC(name, RPCMode.Server,Network.player, args);
    }
     * */
}
