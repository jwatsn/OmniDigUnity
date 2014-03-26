using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DmgTxt
{
    public string Dmg;
    public Vector3 pos;
    public Rect bounds;
    public OmniObject dmged;
    public float counter = 0;
    public float max_time = 4;

    public float max_texttime = 2;
    public GUIContent g;
    public bool drawHealthBar = true;
    public bool draw
    {
        get
        {
            return (counter < max_texttime);
        }
    }
    public bool remove
    {
        get
        {
            return (counter >= max_time);
        }
    }

    public DmgTxt(int dmg,Vector3 pos,OmniObject dmged)
    {
        this.Dmg = ""+dmg;
        this.g = new GUIContent(Dmg);
        this.pos = pos;
        this.dmged = dmged;
        this.bounds = new Rect();
    }

    public void Update(float delta)
    {
        counter += delta;
    }
}

public class OmniInventory : OmniGUI
{
    

    //debug rect
    public static Rect DebugRectDraw;
    Rect DebugRect;
    Vector2 DebugRectPos = new Vector2();
    //Config
    //Bag Stuff
    public static int Bag_Scale = 4;
    public static int Bag_Width = 6;
    public static int Bag_Height = 4;
    public static int Bag_MaxItems = Bag_Width * Bag_Height;


    //Bag slot stuff
    public static int Slot_StartX = 3 * Bag_Scale;
    public static int Slot_StartY = 9 * Bag_Scale;
    public static int Slot_PaddingX = 11 * Bag_Scale;
    public static int Slot_PaddingY = 11 * Bag_Scale;
    public static int Slot_Size = 8 * Bag_Scale;

    //Bag button stuff
    //Drop item
    public static int Button_DropItem_x = 97 * Bag_Scale;
    public static int Button_DropItem_y = 2 * Bag_Scale;
    public static int Button_DropItem_width = 10 * Bag_Scale;
    public static int Button_DropItem_height = 10 * Bag_Scale;
    //Close
    public static int Button_Close_x = 108 * Bag_Scale;
    public static int Button_Close_y = 2 * Bag_Scale;
    public static int Button_Close_width = 10 * Bag_Scale;
    public static int Button_Close_height = 10 * Bag_Scale;


    //public KeyCode toggleKey = KeyCode.I;
    public float x, y, width, height;
    const int margin = 20;
    Texture InvTexture;
    private  Rect windowRect;
    Rect textureRect;
    public static bool show;
    float ratio = 0;

    //Bounding boxes
    Rect bagBounds;
    Rect[,] slotBounds;
    Rect[,] slotNumBounds;
    Rect[] equiptSlotBounds;
    Rect DropBounds;
    Rect CloseBounds;
    Rect heldBounds;

    OmniItemObject heldItem;
    int heldId,heldAmt;
    Vector2 clickPos;
    float lX, lY;

    OmniCamera cam;
    

    ContainerObject localBag;

    GUIStyle numTextStyle;


    public static List<DmgTxt> damageText = new List<DmgTxt>();
    public static List<OmniObject> dmgedObjects = new List<OmniObject>();

	void Start () {
        x = margin;
        y = margin;
        width = Screen.width - margin*2;
        height = Screen.height - margin*2;
        clickPos = new Vector2();
        InvTexture = Resources.Load<Texture>("bag");
        ratio = (float)InvTexture.height / (float)InvTexture.width;
        lX = -1;
        lY = -1;

        SetUpBounds();
        SetUpText();

        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<OmniCamera>();
       
    }

    void SetUpText()
    {
        numTextStyle = new GUIStyle();
        numTextStyle.alignment = TextAnchor.LowerRight;
        numTextStyle.normal.textColor = Color.white;
    }

    	void SetUpBounds() {
		
		//Inventory bounds
		bagBounds = new Rect();

		bagBounds.width = InvTexture.width*Bag_Scale;
		bagBounds.height = (InvTexture.height*Bag_Scale);
		bagBounds.x = ( 0 );
		bagBounds.y = ( 0 );

        windowRect = new Rect((Screen.width / 2) - (bagBounds.width / 2), (Screen.height / 2) - (bagBounds.height / 2), bagBounds.width, bagBounds.height);
		
		//Button Bounds
		DropBounds = new Rect(bagBounds.x + Button_DropItem_x, bagBounds.y + Button_DropItem_y, Button_DropItem_width, Button_DropItem_height);
		CloseBounds = new Rect(bagBounds.x + Button_Close_x, bagBounds.y + Button_Close_y, Button_Close_width, Button_Close_height);
		
		//The held item
		heldBounds = new Rect(0,0,Slot_Size,Slot_Size);
		
		//Slot bounds
		slotBounds = new Rect[Bag_Width,Bag_Height];
        slotNumBounds = new Rect[Bag_Width, Bag_Height];
        equiptSlotBounds = new Rect[4];

        for (int i = 0; i < 4; i++)
        {
            int slotX2 = (int)bagBounds.x + (Slot_StartX + (6 * Slot_PaddingX));
            int slotY2 = (int)bagBounds.y + (Slot_StartY + (i * Slot_PaddingY));
            equiptSlotBounds[i] = new Rect(slotX2, slotY2, Slot_Size, Slot_Size);
        }
		
		for(int x = 0; x < Bag_Width; x++) {
			for(int y = 0; y < Bag_Height; y++) {
				
				int slotX = (int)bagBounds.x + (Slot_StartX + (x * Slot_PaddingX));
				int slotY = (int)bagBounds.y + (Slot_StartY + (y * Slot_PaddingY));
				
				Rect r = new Rect(slotX, slotY, Slot_Size, Slot_Size);
				

				slotBounds[x,y] = r;
                slotNumBounds[x, y] = new Rect(slotX + Slot_Size / 2, slotY + Slot_Size / 2, Slot_Size / 2, Slot_Size / 2);
			}
		}
		
	}

        public void setOwner(ContainerObject obj)
        {
            localBag = obj;
            GetComponent<OmniQuickBar>().setOwner(localBag);
        }
	// Update is called once per frame
	void Update () {

        for (int i = 0; i < damageText.Count; i++)
        {
            damageText[i].Update(Time.deltaTime);
            if (damageText[i].remove)
                damageText.Remove(damageText[i]);
        }

        if (Console.show)
            return;

        if (localBag == null)
            return;

        if (!Console.show && !OmniChatBox.show)
        {
            if (Input.GetButtonDown("Inventory"))
            {
                show = !show;
            }
        }

        if (!show)
            return;


        

        CheckMouse();
	}

    public static void drawDamageGUI(int dmg, Vector2 pos, OmniObject damaged)
    {
        for (int i = 0; i < damageText.Count; i++)
        {
            if (damageText[i].dmged == damaged)
                damageText[i].drawHealthBar = false;
        }

        damageText.Add(new DmgTxt(dmg, pos, damaged));
    }

    public static void giveall(int id)
    {
        int[] giv = new int[OmniItems.itemTypes.Length * 2];
        int c = 0;
        for (int i = 0; i < OmniItems.itemTypes.Length; i++)
        {
            giv[c++] = OmniItems.itemTypes[i].id;
            giv[c++] = 1;
        }
        InventoryEvent e = new InventoryEvent(OmniWorld.tick+3, id, 'a', giv);
        OmniEvents.AddEvent(e);
    }
    /*
    public static void giveall(int id,NetworkPlayer pl)
    {
        int[] giv = new int[OmniItems.itemTypes.Length * 2];
        int c = 0;
        for (int i = 0; i < OmniItems.itemTypes.Length; i++)
        {
            giv[c++] = OmniItems.itemTypes[i].id;
            giv[c++] = 1;
        }
        //InventoryEvent e = new InventoryEvent(OmniWorld.tick + , id, 'a', giv);
       // e.player = pl;
       // OmniEvents.AddEvent(e);
    }
    */
    void DmgGUI()
    {
        for (int i = 0; i < damageText.Count; i++)
        {
            int chX = (int)(damageText[i].dmged.position.x / OmniTerrain.chunkSize);
            int chY = (int)(damageText[i].dmged.position.x / OmniTerrain.chunkSize);






            Vector3 dmgpos = damageText[i].pos;
            Vector3 hppos = damageText[i].dmged.position;
            DrawHPBar(hppos, i);

            DrawDmg(dmgpos, i);


                if (chX == 0)
                {
                    hppos.x = OmniTerrain.Width * OmniTerrain.chunkSize + damageText[i].dmged.position.x;
                    dmgpos.x = OmniTerrain.Width * OmniTerrain.chunkSize + dmgpos.x;
                    DrawHPBar(hppos, i);
                    DrawDmg(dmgpos, i);
                }
                else if (chX == OmniTerrain.Width - 1)
                {
                    hppos.x = -(OmniTerrain.Width * OmniTerrain.chunkSize - damageText[i].dmged.position.x);
                    dmgpos.x = -(OmniTerrain.Width * OmniTerrain.chunkSize - dmgpos.x);
                    DrawHPBar(hppos, i);
                    DrawDmg(dmgpos, i);
                }
                if (chY == 0)
                {
                    hppos.y = OmniTerrain.Height * OmniTerrain.chunkSize + damageText[i].dmged.position.y;
                    dmgpos.y = OmniTerrain.Height * OmniTerrain.chunkSize + dmgpos.y;
                    DrawHPBar(hppos, i);
                    DrawDmg(dmgpos, i);
                }
                else if (chY == OmniTerrain.Height - 1)
                {
                    hppos.y = (OmniTerrain.Height * OmniTerrain.chunkSize - damageText[i].dmged.position.y);
                    dmgpos.y = (OmniTerrain.Height * OmniTerrain.chunkSize - dmgpos.y);
                    DrawHPBar(hppos, i);
                    DrawDmg(dmgpos, i);
                }




        }

    }

    void DrawDmg(Vector3 pos, int i)
    {
        if (damageText[i].draw)
        {
            pos = OmniCamera.cam.WorldToScreenPoint(pos);
            Vector2 size = GUI.skin.label.CalcSize(damageText[i].g);
            damageText[i].bounds.x = pos.x - size.x / 2;
            damageText[i].bounds.y = Screen.height - pos.y;
            damageText[i].bounds.width = size.x;
            damageText[i].bounds.height = size.y;

            GUI.Label(damageText[i].bounds, damageText[i].g);
        }
    }

    void DrawHPBar(Vector3 hppos, int i)
    {
        if (damageText[i].drawHealthBar)
        {
            Vector2 dmgsize = new Vector2(50, 4);
            hppos.x += damageText[i].dmged.item.Size / 2f;
            hppos.y += damageText[i].dmged.item.Size + 0.6f;
            Vector3 dmgpos = OmniCamera.cam.WorldToScreenPoint(hppos);
            damageText[i].bounds.x = dmgpos.x - (dmgsize.x / 2f) - 5f;
            damageText[i].bounds.y = Screen.height - dmgpos.y;
            damageText[i].bounds.width = dmgsize.x;
            damageText[i].bounds.height = dmgsize.y;
            GUI.DrawTexture(damageText[i].bounds, OmniQuickBar.healthbarBG_texture);
            float a = ((float)damageText[i].dmged.HP / (float)damageText[i].dmged.item.baseHP);
            if (a > 1)
                a = 1;
            if (a < 0)
                a = 0;
            damageText[i].bounds.width = a * dmgsize.x;
            GUI.DrawTexture(damageText[i].bounds, OmniQuickBar.healthbar_texture);
        }
    }

    float tt = 1;

    float vv = 1;
    float qq = 1;
    void OnGUI()
    {

        if (DebugRectDraw != null)
        {
            DebugRect = DebugRectDraw;
            DebugRectPos.x = DebugRectDraw.x;
            DebugRectPos.y = DebugRectDraw.y;
            DebugRectPos = cam.camera.WorldToScreenPoint(DebugRectPos);
            DebugRect.width = DebugRectDraw.width * cam.camera.orthographicSize;
            DebugRect.height = DebugRectDraw.height * cam.camera.orthographicSize;

            DebugRect.x = DebugRectPos.x;
            DebugRect.y = Screen.height - DebugRectPos.y - DebugRect.height;

            GUI.DrawTexture(DebugRect, OmniQuickBar.healthbar_texture);
        }

        DmgGUI();


        if (!show)
        {
            return;
        }

        //GUI.DrawTexture(windowRect, InvTexture);
        windowRect = GUILayout.Window(123466, windowRect, InvWindow, "",GUIStyle.none);
    }
    
    void InvWindow(int windowID)
    {
        GUILayout.BeginHorizontal();
        GUI.DrawTexture(bagBounds, InvTexture);
        for (int i = 0; i < Bag_MaxItems; i++)
        {
            if(localBag.bagItems[i] != null)
                if(localBag.bagItems[i].stack > 0)
                {
                    OmniAnimation anim = localBag.bagItems[i].type.animList[0];
                    GUI.DrawTextureWithTexCoords(slotBounds[i % Bag_Width, i / Bag_Width], OmniAtlas.texture.mainTexture, anim.thumbnailRect);
                    GUI.Label(slotNumBounds[i % Bag_Width, i / Bag_Width], ""+localBag.bagItems[i].stack, numTextStyle);
                }
        }
        for (int i = 0; i < 4; i++)
        {
            if (localBag.equiptItems[i] != null)
            {
                if (localBag.equiptItems[i].stack > 0)
                {

                    OmniAnimation anim = localBag.equiptItems[i].type.animList[0];
                    GUI.DrawTextureWithTexCoords(equiptSlotBounds[i], OmniAtlas.texture.mainTexture, anim.thumbnailRect);
                    
                }
            }
        }
        if (heldItem != null)
        {
            OmniAnimation anim = heldItem.type.animList[0];
            GUI.DrawTextureWithTexCoords(heldBounds, OmniAtlas.texture.mainTexture, anim.thumbnailRect);
        }

        GUILayout.EndHorizontal();
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    void CheckMouse()
    {
        if (Input.GetMouseButton(1))
        {
            clickPos = Input.mousePosition;
            clickPos.x = clickPos.x - windowRect.x;
            clickPos.y = windowRect.height - (clickPos.y - windowRect.y);
            if (lX < 0)
            {
                Clicked(1);
            }
            lX = clickPos.x;
            lY = clickPos.y;
        }
        else if (Input.GetMouseButton(0))
        {
            clickPos = Input.mousePosition;
            clickPos.x = clickPos.x - windowRect.x;
            clickPos.y = windowRect.height - (clickPos.y - windowRect.y);
            if (lX < 0)
            {
                Clicked(0);
            }
            else
            {
                if (lX != clickPos.x || lY != clickPos.y)
                    Dragged();

            }
            lX = clickPos.x;
            lY = clickPos.y;
        }
        else if (lX >= 0)
        {
            Released();
            lX = -1;
            lY = -1;
        }

    }

    void Clicked(int button)
    {
        for (int i = 0; i < Bag_MaxItems; i++)
        {
            if (slotBounds[i % Bag_Width, i / Bag_Width].Contains(clickPos))
                if (localBag != null)         
                    if(localBag.bagItems[i] != null)
                        if (localBag.bagItems[i].stack > 0)
                        {
                            if (button == 0)
                            {
                                heldItem = localBag.bagItems[i];
                                heldId = i;
                                heldAmt = localBag.bagItems[i].stack;
                                heldBounds.x = clickPos.x - (heldBounds.width / 2);
                                heldBounds.y = clickPos.y - (heldBounds.height / 2);
                            }
                            else
                            {
                                if (Network.isClient)
                                    OmniWorld.netView.RPC("eq", RPCMode.Server, Network.player, i);

                                localBag.TryEquiptItem(i);
                            }
                        }
        }
        
    }

    void Dragged()
    {
        if (heldItem != null)
        {
            heldBounds.x = clickPos.x - (heldBounds.width / 2);
            heldBounds.y = clickPos.y - (heldBounds.height / 2);
        }
    }

    void Released()
    {
        if (heldItem != null)
        {
            for (int i = 0; i < Bag_MaxItems; i++)
            {
                if (slotBounds[i % Bag_Width, i / Bag_Width].Contains(clickPos))
                    if (localBag != null)
                    {
                        InventoryEvent e = new InventoryEvent(OmniWorld.tick, localBag.id, 'm', heldId, i, heldAmt);
                        OmniEvents.AddEvent(e);                       
                        break;
                    }
            }
            heldItem = null;
        }
    }


}
