using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct predictionStruct
{
    public int tick;

    public int inputMask;
    public float x;
    public float y;
    public float accelx;
    public float accely;

    

    public predictionStruct(int tick, float x, float y,float accelx,float accely,int inputMask)
    {
        this.tick = tick;
        this.x = x;
        this.y = y;
        this.accelx = accelx;
        this.accely = accely;
        this.inputMask = inputMask;
    }
}

public class OmniLocal : MonoBehaviour {

    OmniInventory inventoryManager;
    public OmniObject owner;
    public bool isFlying = false;
    ClientControllable controlOwner;
    public static int IgnoreInput = 0;
    public static string localName = "Bob";
    public static int LocalID = -1;
    public static OmniLocal instance;
    public static bool usingGamePad = false;
    public List<predictionStruct> inputBuffer;
    public static int LevelOfDetail = 0;
    Vector3 lastPos;
    int lastTick = -1;
    int lastDir;
    int lastMask = 0;
    int inputMask = 0;
    float ignoreCounter;
    bool lastFlipped;
    float dY;

	OmniCamera cam;


	void Start () {
        inputBuffer = new List<predictionStruct>();
		cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<OmniCamera>();
        inventoryManager = GameObject.FindGameObjectWithTag("Inv").GetComponent<OmniInventory>();
        lastPos = Vector3.zero;
        instance = this;
	}

    public static void UpdateCamera()
    {
        if (instance == null)
            return;
        else
        {
            if (instance.owner != null)
            {
                
                Camera cam = instance.cam.camera;
                cam.orthographicSize += Input.GetAxis("Zoom");

                Vector3 pos = cam.transform.position;
                pos.x = instance.owner.Position.x + instance.owner.item.Size / 2f;
                pos.y = instance.owner.Position.y + instance.owner.item.Size / 2f;
                pos.z = -10;
                cam.transform.position = pos;



                
                
            }
        }
    }

    public void setOwner(OmniObject owner)
    {

        
        if (owner is ClientControllable)
        {
            controlOwner = (ClientControllable)owner;
            inventoryManager.setOwner(controlOwner);
            controlOwner.lerpSpeed = OmniWorld.timeStep;
        }
        this.owner = owner;
        cam.owner = owner;
    }

    public int getInput(int tick)
    {
        for (int i = 0; i < inputBuffer.Count; i++)
        {
            if (inputBuffer[i].tick == tick)
                return i;
        }
        return -1;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0))
        {
            Vector3 pos = OmniCamera.cam.ScreenToWorldPoint(Input.mousePosition);
            pos.x = (int)pos.x;
            pos.y = (int)pos.y;
            Debug.Log(pos);
        }


        if (Input.GetAxisRaw("HoldItem") < -0.5f || Input.GetAxisRaw("HorizAim") != 0 || Input.GetAxisRaw("VertAim") != 0)
        {
            usingGamePad = true;
        }

        if (lastPos != Input.mousePosition)
            usingGamePad = false;

        if (OmniLocal.LocalID < 0)
            return;

        if(Input.GetKeyDown(KeyCode.F1))
        {
            //Debug.Log(owner.bounds);
        }
        while (inputBuffer.Count > 30)
            inputBuffer.RemoveAt(0);


        

	}
    public static int GetInputBuffer(int tick)
    {
        for (int i = 0; i < instance.inputBuffer.Count; i++)
        {
            if (instance.inputBuffer[i].tick == tick)
                return i;
        }
        return -1;
    }


    void OnSceneGUI()
    {
    }


    public static ClientControllable getLocalPlayer()
    {
        if (LocalID < 0 || LocalID >= OmniWorld.instance.SpawnedObjectsNew.Count)
            return null;


        return OmniWorld.instance.SpawnedObjectsNew[LocalID] as ClientControllable;
    }



    public void OmniUpdate(float delta)
    {
        TryGetLocal();

        if (owner == null || LocalID < 0)
            return;


        if (owner.id != LocalID)
            LocalID = owner.id;
        
//        if (owner.fireAnim < 0)
//            CheckDirection();
//        else
//            owner.direction = lastDir;



        if (controlOwner != null)
            CheckInput();
        
        
        

        if (!Network.isClient) 
        CheckMousePos();
       

        if (usingGamePad)
            Screen.showCursor = false;
        else
            Screen.showCursor = true;
        
       if(controlOwner != null)
           inputBuffer.Add(new predictionStruct(OmniWorld.tick, owner.bounds.x, owner.bounds.y, controlOwner.accel.x, controlOwner.accel.y, controlOwner.inputMask));

       IgnoreInput -= 1;
       if (IgnoreInput < 0)
           IgnoreInput = 0;

    
    }
    void TryGetLocal()
    {
        if (owner != null)
        {
            if (owner.id != LocalID)
                if (LocalID >= 0)
                    LocalID = owner.id;

            return;
        }


        if (LocalID >= 0)
            if (OmniWorld.instance.SpawnedObjectsNew.Count > LocalID)
                if (OmniWorld.instance.SpawnedObjectsNew[LocalID] != null)
                {
                    setOwner(OmniWorld.instance.SpawnedObjectsNew[LocalID]);
                }
    }
    void CheckMousePos()
    {
        lastPos = Input.mousePosition;
        Vector2 mousePos = OmniCamera.cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 playerCenter = new Vector2(owner.bounds.x + owner.item.Size / 2, owner.bounds.y + owner.item.Size / 2);
        if (controlOwner != null)
        {

            
            if (usingGamePad)
            {
                float fireAngle = OmniHuman.fixangle(Input.GetAxis("VertAim") * 90, 90);
                float lookAngle = OmniHuman.fixangle(Input.GetAxis("VertAim") * 90, 45);

                

                controlOwner.fireRot = fireAngle;
                controlOwner.lookTo = lookAngle;

                float horizAim = Input.GetAxisRaw("HorizAim");
                if (horizAim > 0.5f)
                {

                    owner.flipped = false;
                }
                else if (horizAim < -0.5f)
                {
                    owner.flipped = true;
                }
            }
            else
            {
                Vector2 r = mousePos - playerCenter;
                float lookRot = Mathf.Atan2(r.y, r.x) * Mathf.Rad2Deg;
                
                if (controlOwner.flipped)
                {
                    if (lookRot > 0)
                        lookRot = 180 - lookRot;
                    else if (lookRot < 0)
                        lookRot = (180 + lookRot) * -1;
                }
                controlOwner.lookTo = lookRot;
                /*
                if (lookRot < 0)
                    lookRot = 360 + lookRot;

                


                if (controlOwner.flipped)
                {
                    if (controlOwner.lookTo > 180)
                        controlOwner.lookTo = 360 - (controlOwner.lookTo - 180);
                    else
                        controlOwner.lookTo = 180 - controlOwner.lookTo;

                }
                 * */
                controlOwner.fireRot = OmniHuman.fixangle(controlOwner.lookTo, 90);
                controlOwner.lookTo = OmniHuman.fixangle(controlOwner.lookTo,25);
                


                lastFlipped = owner.flipped;

                if (owner.mount0time == 0)
                {
                    if (mousePos.x > playerCenter.x)
                    {
                        owner.flipped = false;
                    }
                    else
                    {
                        owner.flipped = true;
                    }


                }

            }
        }
    }

    void CheckInput()
    {
        if (Input.GetKey(KeyCode.F2))
        {
            controlOwner.bounds.x += 0.3f;
        }


        
        lastMask = inputMask;
        inputMask = 0;

        if (!Console.show && !OmniInventory.show && !OmniChatBox.showInput && IgnoreInput <= 0)
        {


            if (Input.GetKey(KeyCode.A) || Input.GetAxisRaw("Strafe")<-0.5f)
            {
                inputMask |= (1 << OmniInput.Left);
            }
            if (Input.GetKey(KeyCode.D) || Input.GetAxisRaw("Strafe") > 0.5f)
            {
                inputMask |= (1 << OmniInput.Right);
            }
            if (Input.GetKey(KeyCode.W) || Input.GetAxisRaw("Fire1") > 0.5f)
            {
                inputMask |= (1 << OmniInput.Up);
            }
            if (Input.GetKey(KeyCode.S) || Input.GetAxisRaw("Fire1") < -0.5f)
            {
                inputMask |= (1 << OmniInput.Down);
            }
            if (Input.GetMouseButton(0) || Input.GetAxisRaw("HoldItem") < -0.5f)
            {
                if (!OmniInventory.show)
                    if (!OmniQuickBar.collides(Input.mousePosition))
                        inputMask |= (1 << OmniInput.Clicked);
            }
            if (Input.GetMouseButton(1))
            {
                if (!OmniInventory.show)
                    if (!OmniQuickBar.collides(Input.mousePosition))
                        inputMask |= (1 << OmniInput.RightClicked);
            }
        }
        if (lastMask != inputMask)
        {
            Vector3 mousePos = cam.camera.ScreenToWorldPoint(Input.mousePosition);
            InputEvent e = new InputEvent(OmniWorld.tick+1,controlOwner,inputMask,mousePos);
            OmniEvents.AddEvent(e);
        }
    }


    //called every network update before we send server info
    public void NetworkUpdate()
    {
        CheckMousePos();
    }

    /*
    void CheckDirection()
    {
        Vector3 mousePos = Input.mousePosition;
        
        if (mousePos.x > Screen.width / 2)
            owner.direction = 1;
        else
            owner.direction = -1;

        if (Network.isClient)
        {
            if (lastDir != owner.direction)
            {
                OmniWorld.netView.RPC("dir", RPCMode.Server, Network.player, owner.direction);
            }
        }
        else
        {
            owner.needsUpdate = true;
        }

        lastDir = owner.direction;
    }
    */
    void CheckMirrorChunks(int x2,int y2)
    {
        int x = OmniTerrain.Width + x2;
        int y = y2;

        int i = x + y * OmniTerrain.Width;
        if (!OmniTerrain.chunks.ContainsKey(i))
        {
            OmniTerrain.chunks[i] = new TerrainChunk(OmniTerrain.instance, x, y, OmniTerrain.chunkSize);
        }
        
    }

    public void CheckActiveChunks()
    {
        if (owner == null)
            return;

        if (owner.spawned || Network.isClient)
        {

            
            //Vector3 bottomLeft = cam.camera.ViewportToWorldPoint(OmniTerrain.frustrum[0]);
            //Vector3 topRight = cam.camera.ViewportToWorldPoint(OmniTerrain.frustrum[1]);

            Vector3 bottomLeft = new Vector3();
            Vector3 topRight = new Vector3();


            bottomLeft.x = (owner.bounds.x + owner.item.Size / 2f) - OmniCamera.cam.orthographicSize * 2;
            bottomLeft.y = (owner.bounds.y + owner.item.Size / 2f) - OmniCamera.cam.orthographicSize * OmniCamera.cam.aspect;

            topRight.x = (owner.bounds.x + owner.item.Size / 2f) + OmniCamera.cam.orthographicSize * 2;
            topRight.y = (owner.bounds.y + owner.item.Size / 2f) + OmniCamera.cam.orthographicSize * OmniCamera.cam.aspect;


            

            int x1 = (int)bottomLeft.x / OmniTerrain.chunkSize;
            int x2 = (int)topRight.x / OmniTerrain.chunkSize;
            int y1 = (int)bottomLeft.y / OmniTerrain.chunkSize;
            int y2 = (int)topRight.y / OmniTerrain.chunkSize;



            for (int xx = x1; xx <= x2; xx++)
            {
                for (int yy = y1; yy <= y2; yy++)
                {
                    int x = xx;
                    int y = yy;

                    if (x < 0)
                    {
                            x = (OmniTerrain.Width - 1) + x;
                    }
                    else if (x >= OmniTerrain.Width)
                    {
                        x = x - OmniTerrain.Width;
                    }
                    if (y < 0)
                    {
                        y = (OmniTerrain.Height - 1) + y;
                    }
                    else if (y >= OmniTerrain.Height)
                    {
                        y = y - OmniTerrain.Height;
                    }

                    int i = x + y * OmniTerrain.Width;

                    if (!OmniTerrain.chunks.ContainsKey(i))
                    {
                        OmniTerrain.chunks[i] = new TerrainChunk(OmniTerrain.instance, x, y, OmniTerrain.chunkSize);
                    }
                    else
                    {
                        //OmniTerrain.activeChunks.Add(OmniTerrain.chunks[i]);
                    }
                    if (x == 0)
                    {
                        int i2 = (OmniTerrain.Width - 1) + y * OmniTerrain.Width;
                        if (!OmniTerrain.chunks.ContainsKey(i2))
                        {
                            OmniTerrain.chunks[i2] = new TerrainChunk(OmniTerrain.instance, (OmniTerrain.Width - 1), y, OmniTerrain.chunkSize);
                        }
                        else
                        {
                            //OmniTerrain.activeChunks.Add(OmniTerrain.chunks[i2]);
                        }
                    }
                }

            }
        }
    }

    public bool predictionPass(int tick,float x, float y)
    {
        for (int i = 0; i < inputBuffer.Count; i++)
        {
                if (inputBuffer[i].x == x && inputBuffer[i].y == y)
                    return true;                  
        }

        return false;
    }

    void OnConnectedToServer()
    {
		Debug.Log("Connected to server");
	}

	void DebugKeys() {


	
	}
}
