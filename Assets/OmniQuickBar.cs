using UnityEngine;
using System.Collections;

public class OmniQuickBar : OmniGUI
{

    //Quickbar
    public static int QuickBar_Scale = 4;
    public static int QuickBar_Capacity = 4;
    public static int QuickBar_PaddingX = 1 * QuickBar_Scale;
    public static int QuickBar_PaddingY = 1 * QuickBar_Scale;
    public static int QuickBar_Size = 8 * QuickBar_Scale;
    public static int QuickBar_SlotSpace = 12 * QuickBar_Scale;
    public static int QuickBar_Y = 15;
    public static int QuickBar_SelectedOffset = QuickBar_SlotSpace - QuickBar_Size;
    public static int HealthBar_Width = 120;
    public static int HealthBar_Height = 8;
    public static int ownerID = -1;


    public Rect quickbar_bounds;
    public Rect selected_bounds;
	public Rect bagbutton_bounds;
	public Rect[] slot_bounds;
    public Rect healthbar_bounds;
    public Rect healthbarBG_bounds;
    public Rect staminabar_bounds;
    public Rect staminabarBG_bounds;


    ContainerObject localBag;

    Texture quickbar_texture;
    Texture quickbar_box_texture;
    Texture staminabar_texture;
    public static Texture healthbar_texture;
    public static Texture healthbarBG_texture;

    static OmniQuickBar instance;

    public bool enabled = true;
    bool showBounds = false;

	// Use this for initialization
	void Start () {

        instance = this;

        SetUpTextures();
        SetUpBounds();
        
	}

    void SetUpBounds() {
		
		int qbwidth = quickbar_texture.width * QuickBar_Scale;
		int qbheight = quickbar_texture.height * QuickBar_Scale;
		
        
		quickbar_bounds = new Rect((Screen.width/2)-(qbwidth/2), Screen.height-qbheight-15,qbwidth,qbheight);
        selected_bounds = new Rect(0, 0, QuickBar_Size + QuickBar_Scale*2, QuickBar_Size + QuickBar_Scale*2);
		slot_bounds = new Rect[QuickBar_Capacity];
		
		float startX = quickbar_bounds.x + QuickBar_PaddingX;
        healthbar_bounds = new Rect((Screen.width / 2) - HealthBar_Width / 2, Screen.height - QuickBar_Y - QuickBar_PaddingY - QuickBar_Size - 15, HealthBar_Width, HealthBar_Height);
        healthbarBG_bounds = new Rect((Screen.width / 2) - HealthBar_Width / 2, Screen.height - QuickBar_Y - QuickBar_PaddingY - QuickBar_Size - 15, HealthBar_Width, HealthBar_Height);

        staminabar_bounds = new Rect((Screen.width / 2) - HealthBar_Width / 2, Screen.height - QuickBar_Y - QuickBar_PaddingY - QuickBar_Size - 25, HealthBar_Width, HealthBar_Height);
        staminabarBG_bounds = new Rect((Screen.width / 2) - HealthBar_Width / 2, Screen.height - QuickBar_Y - QuickBar_PaddingY - QuickBar_Size - 25, HealthBar_Width, HealthBar_Height);

		for(int i = 0; i < QuickBar_Capacity; i++) {
            slot_bounds[i] = new Rect(startX, Screen.height - QuickBar_Y - QuickBar_PaddingY - QuickBar_Size, QuickBar_Size, QuickBar_Size);
			startX += QuickBar_SlotSpace;
		}
		
	}
    public void setOwner(ContainerObject obj)
    {
        localBag = obj;
    }
    public static bool collides(Vector2 pos)
    {
        pos.y = Screen.height - pos.y;
        for (int i = 0; i < QuickBar_Capacity; i++)
        {
            if (instance.slot_bounds[i].Contains(pos))
                return true;
        }
        return false;
    }

    void SetUpStyles()
    {

    }

    void SetUpTextures()
    {
        quickbar_texture = Resources.Load<Texture>("GUI_Quickbar");
        quickbar_box_texture = Resources.Load<Texture>("selected");
        staminabar_texture = Resources.Load<Texture>("GUI_staminabar");
        healthbar_texture = Resources.Load<Texture>("GUI_healthbar");
        healthbarBG_texture = Resources.Load<Texture>("GUI_healthbarBG");
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.F3))
        {
            showBounds = !showBounds;
        }



        if (localBag == null)
            return;

        checkClick();
    
    }

    void checkClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = Input.mousePosition;
            pos.y = Screen.height - pos.y;
            for (int i = 0; i < QuickBar_Capacity; i++)
            {
                if (slot_bounds[i].Contains(pos))
                {

                    selected_bounds.x = slot_bounds[i].x - QuickBar_Scale;
                    selected_bounds.y = slot_bounds[i].y - QuickBar_Scale;
                    InventoryEvent e = new InventoryEvent(OmniWorld.tick, localBag.id, 's', i);
                    OmniEvents.AddEvent(e);
                }
            }
        }
    }

    void OnGUI()
    {

        if(showBounds)
        if (localBag != null)
        {
            if (localBag.mount0 >= 0)
            {
                for (int i = 0; i < localBag.numBounds; i++)
                {
                    Vector3 pos1 = new Vector3(localBag.itemBounds[i].x,localBag.itemBounds[i].y,0);
                    Vector3 pos = OmniCamera.cam.WorldToScreenPoint(pos1);
                    Vector3 size = new Vector3(localBag.itemBounds[i].width, localBag.itemBounds[i].height, 0);
                    size *= OmniCamera.cam.orthographicSize;
                    Rect d = new Rect(pos.x, Screen.height-pos.y - size.y, size.x, size.y);
                    GUI.DrawTexture(d, healthbar_texture);
                }
            }
        }

        if (enabled)
        {
            GUI.DrawTexture(quickbar_bounds, quickbar_texture);

            float a = 0;
            float a2 = 0;
            if (localBag != null)
            {
                DrawItems();

                if (localBag.selected >= 0)
                    GUI.DrawTexture(selected_bounds, quickbar_box_texture);

                a = (float)localBag.HP / (float)localBag.item.baseHP;
                a2 = localBag.Stamina / 100.0f;
                if (a2 < 0)
                    a2 = 0;
                if (a2 > 1)
                    a2 = 1;
                if (a < 0)
                    a = 0;
                if (a > 1)
                    a = 1;


            }
            healthbar_bounds.width = healthbarBG_bounds.width * a;
            staminabar_bounds.width = staminabarBG_bounds.width * a2;
            GUI.DrawTexture(healthbarBG_bounds, healthbarBG_texture);
            GUI.DrawTexture(healthbar_bounds, healthbar_texture);

            GUI.DrawTexture(staminabarBG_bounds, healthbarBG_texture);
            GUI.DrawTexture(staminabar_bounds, staminabar_texture);
            
        }
    }

    void DrawItems()
    {
        for (int i = 0; i < QuickBar_Capacity; i++)
        {
            if (localBag.bagItems[i] != null)
            {

                OmniAnimation anim = localBag.bagItems[i].type.animList[0];
                if (anim == null)
                    continue;
                GUI.DrawTextureWithTexCoords(slot_bounds[i], OmniAtlas.texture.mainTexture, anim.thumbnailRect);
                
            }
        }
    }
}
