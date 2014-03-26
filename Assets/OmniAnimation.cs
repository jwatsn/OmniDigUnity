using UnityEngine;
using System.Collections;

[System.Serializable]
public class AnimSkeleton
{
    

    public Rect bounds;
    public string name;
    public Vector3 rotationPoint;
    public bool noDraw;
    public Vector3[] mountPoint = {new Vector3()};
    public int id;

    [HideInInspector]
    public Matrix4x4[] mountMatrix;
    [HideInInspector]
    public Matrix4x4 worldTr;
    [HideInInspector]
    public Matrix4x4 tr;
    [HideInInspector]
    public Rect texBounds;
    [HideInInspector]
    public float rotation;
    float lastRot = -1;

    //public float rotateTo;
    //float rotateFrom;
    float looklerp;




    public AnimSkeleton()
    {
    }

    public AnimSkeleton(AnimSkeleton s)
    {
        this.bounds = new Rect(s.bounds.x, s.bounds.y, s.bounds.width, s.bounds.height);
        this.rotationPoint = new Vector3(s.rotationPoint.x, s.rotationPoint.y, s.rotationPoint.z);
        this.name = s.name;
        this.mountMatrix = s.mountMatrix;
        this.tr = s.tr;
        this.worldTr = s.tr;
        this.noDraw = s.noDraw;
        this.texBounds = new Rect(s.texBounds.x, s.texBounds.y, s.texBounds.width, s.texBounds.height);
        this.mountPoint[0] = new Vector3(s.mountPoint[0].x, s.mountPoint[0].y, s.mountPoint[0].z);
    }
    /*
    public void rotate(float angle)
    {
        rotateFrom = rotateTo;
        if (rotateFrom < 0 || Mathf.Abs(rotateFrom - angle) > 6)
            rotateFrom = angle;


        rotateTo = angle;
        looklerp = 0;
    }
     * */
    public bool hasChanged()
    {
        if (lastRot != rotation)
        {
            lastRot = rotation;
            return true;
        }
        else return false;
    }
    /*
    public void update(OmniObject owner)
    {
        if (rotateTo >= 0)
        {
            float a = looklerp / owner.lerpSpeed;
            if (a > 1)
                a = 1;

            rotation = Mathf.LerpAngle(rotateFrom, rotateTo, a);
            
        }
        looklerp += Time.deltaTime;
        
    }
     * */
}
[System.Serializable]
public class SkelAnim
{
    [System.Serializable]
    public class sk
    {
        public float time;
        public Vector3 pos;
        public float angle;
    }

    public string name;
    public sk[] frames;
}


public class OmniAnimation : MonoBehaviour {

	public enum Type {
		Frame,
		EngineRotation,
        Skeleton
	}

    public static int Max_Skeletons = 10;

    public Vector3[] mountPoint;


    public Vector2 boundsPosOffset;
    public Vector2 boundsSizeOffset;
    public float RotationOffset = 0;
	public int width = 8;
	public int height = 8;
	public int frames = 1;
	public float speed = 0.3f;
    public Rect thumbnailRect;
	[HideInInspector]public Mesh[] meshs;
	[HideInInspector]public Mesh[] meshsFlipped;
	[HideInInspector]public Mesh mesh;
	[HideInInspector]public Mesh meshFlipped;
	public string animName = "idle";
	public Type animationType = Type.Frame;

    public AnimSkeleton[] skeletonRects;


	float stateTime;


	public string texture;

	[HideInInspector]public TPAtlasTexture aTexture;
    [HideInInspector]public Texture nTexture;
    [HideInInspector]public TPAtlasTexture boundsTexture;
	[HideInInspector]public int index = 0;
	[HideInInspector]public int currentFrame;
    [HideInInspector]public GameObject itemObject;

    public Rect[] boundsList;
    [HideInInspector]public Vector2[] boundsPos;

	TPAtlas atlas;
	
	void OnEnable() {
		atlas = TPackManager.getAtlas ("World");
		aTexture = atlas.getTexture(atlas.frameNames [index]);
		//aTexture.calculateVars();
        
        boundsTexture = atlas.getTexture("MAT_Dirt");
        
	}
    public void DrawThumbnail(Rect pos)
    {

    }

	void Start () {



        //aTexture.calculateVars();
        for (int i = 0; i < mountPoint.Length; i++)
        {
            mountPoint[i].x /= width;
            mountPoint[i].y /= height;
        }


        boundsPosOffset.x /= width;
        boundsPosOffset.y /= height;
        boundsSizeOffset.x /= width;
        boundsSizeOffset.y /= height;

        boundsPos = new Vector2[boundsList.Length];
        for (int i = 0; i < boundsList.Length; i++)
        {
            boundsList[i].x /= width;
            boundsList[i].y /= height;
            boundsList[i].width /= width;
            boundsList[i].height /= height;

            boundsPos[i].x = boundsList[i].x;// +boundsList[i].width / 2;
            boundsPos[i].y = boundsList[i].y;// +boundsList[i].height / 2;
        }

        if (animationType == Type.Skeleton)
        {

            meshs = new Mesh[skeletonRects.Length];
            meshsFlipped = new Mesh[skeletonRects.Length];
            for (int i = 0; i < skeletonRects.Length; i++)
            {
                skeletonRects[i].bounds.x /= width;
                skeletonRects[i].bounds.y /= height;

                skeletonRects[i].bounds.width /= width;
                skeletonRects[i].bounds.height /= height;

                skeletonRects[i].rotationPoint.x /= width;
                skeletonRects[i].rotationPoint.y /= height;

                if (skeletonRects[i].mountPoint.Length <= 0)
                    skeletonRects[i].mountPoint = new Vector3[] { new Vector3() };

                skeletonRects[i].mountPoint[0].x /= width;
                skeletonRects[i].mountPoint[0].y /= height;


                skeletonRects[i].mountMatrix = new Matrix4x4[] { Matrix4x4.TRS(new Vector3(skeletonRects[i].mountPoint[0].x, skeletonRects[i].mountPoint[0].y, 0), Quaternion.identity, Vector3.one) };

                skeletonRects[i].tr = Matrix4x4.TRS(new Vector3(skeletonRects[i].bounds.x, skeletonRects[i].bounds.y, 0), Quaternion.identity, Vector3.one);

                float x = aTexture.coords.x + skeletonRects[i].bounds.x * aTexture.coords.width;
                float y = aTexture.coords.y + skeletonRects[i].bounds.y * aTexture.coords.height;

                float width2 = aTexture.coords.width * skeletonRects[i].bounds.width;
                float height2 = aTexture.coords.height * skeletonRects[i].bounds.height;
                skeletonRects[i].id = i;
                skeletonRects[i].texBounds = new Rect(x, y, width2, height2);
            }

        }
        else
        {
            meshs = new Mesh[frames];

            meshsFlipped = new Mesh[frames];
        }
		genMesh ();
		genMeshFlipped ();
        thumbnailRect = new Rect(aTexture.coords.x, aTexture.coords.y, aTexture.coords.width/frames, aTexture.coords.height);
	}
	
	public void draw(Rect pos) {
        aTexture.draw(pos);
	}

    [ContextMenu("Set All Mount Points to this")]
    void setMountPoints()
    {
        OmniAnimation[] anim = GetComponents<OmniAnimation>();
        for(int i=0; i<anim.Length;i++)
            anim[i].mountPoint[0] = new Vector3(mountPoint[0].x,mountPoint[0].y,mountPoint[0].z);
    }

    public void drawFrame(OmniObject owner,float time, Vector3 scale,OmniAnimation.Type type,params Vector3[] mount)
    {

        

        if(owner is PhysicsObject)
            if (((PhysicsObject)owner).isProjectile)
            {
                drawSkeleton(owner, time, scale);
            }
            else
            {

                if (type == null)
                {

                    switch (animationType)
                    {
                        case Type.Frame:
                            draw(owner, time, scale, mount);
                            break;
                        case Type.EngineRotation:
                            drawRot(owner, time, scale, mount);
                            break;
                        case Type.Skeleton:
                            drawSkeleton(owner, time, scale);
                            break;
                    }
                }
                else
                {
                    switch (type)
                    {
                        case Type.Frame:
                            draw(owner, time, scale, mount);
                            break;
                        case Type.EngineRotation:
                            drawRot(owner, time, scale, mount);
                            break;
                        case Type.Skeleton:
                            drawSkeleton(owner, time, scale);
                            break;
                    }
                }
            }
    }

    public void draw(OmniObject player,float time, Vector3 scale,params Vector3[] mount)
    {


        int frame = ((int)(time / speed)) % frames;
        Vector3 pos = player.position;



        if (!player.flipped)
        {
            Quaternion rot = Quaternion.AngleAxis(-RotationOffset, new Vector3(0, 0, 1));
 //           pos.x -= boundsPosOffset.x;
 //           pos.y += boundsPosOffset.y;
            Matrix4x4 tr = Matrix4x4.identity;
            tr.SetTRS(pos, Quaternion.identity, scale);
//            tr *= Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0),rot, Vector3.one);
//            tr *= Matrix4x4.TRS(new Vector3(-0.5f, -0.5f, 0), Quaternion.identity, Vector3.one);
            tr *= Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0), Quaternion.AngleAxis(player.rotation, new Vector3(0, 0, 1)), Vector3.one);
            tr *= Matrix4x4.TRS(new Vector3(-0.5f, -0.5f, 0), Quaternion.identity, Vector3.one);
            Graphics.DrawMesh(meshs[frame], tr, OmniAtlas.texture, 0);
        }
        else
        {
         //   pos.x += OffsetX;
         //   pos.y += OffsetY;
            Matrix4x4 tr = Matrix4x4.identity;
            tr.SetTRS(pos, Quaternion.identity, scale);
            tr *= Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0), Quaternion.AngleAxis(player.rotation, new Vector3(0, 0, 1)), Vector3.one);
            tr *= Matrix4x4.TRS(new Vector3(-0.5f, -0.5f, 0), Quaternion.identity, Vector3.one);

            Graphics.DrawMesh(meshsFlipped[frame], tr, OmniAtlas.texture, 0);
        }


    }


    void OnGUI()
    {
        //GUI.Box(
    }

    public void drawSkelMountedProj(DamageableObject p,int i,Vector2 scale)
    {
        Matrix4x4 tr;
        Quaternion rot;

        Vector2 pos = new Vector2(p.mountedTo.Position.x + p.mountPos.x, p.mountedTo.Position.y + p.mountPos.y);
        tr = Matrix4x4.TRS(p.mountedTo.Position, Quaternion.identity, p.mountedTo.item.scale);

        tr *= Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0), Quaternion.AngleAxis(p.mountedTo.rotation, Vector3.forward), Vector3.one);
        tr *= Matrix4x4.TRS(new Vector3(-0.5f, -0.5f, 0), Quaternion.identity, Vector3.one);
        rot = Quaternion.AngleAxis(p.mountedTo.skeleton[p.mountLoc].rotation, Vector3.forward);
        Quaternion rot2 = Quaternion.AngleAxis(RotationOffset - 90, new Vector3(0, 0, 1));
        if (p.mountedTo.flipped)
            rot = Quaternion.AngleAxis(-p.mountedTo.skeleton[p.mountLoc].rotation, Vector3.forward);
        else
            rot = Quaternion.AngleAxis(p.mountedTo.skeleton[p.mountLoc].rotation, Vector3.forward);

        tr *= p.mountedTo.skeleton[p.mountLoc].tr;
        tr *= Matrix4x4.TRS(p.mountedTo.skeleton[p.mountLoc].rotationPoint, rot, Vector3.one);
        tr *= Matrix4x4.TRS(-p.mountedTo.skeleton[p.mountLoc].rotationPoint, Quaternion.identity, Vector3.one);
        if (!p.flipped)
            tr *= Matrix4x4.TRS(p.mountPos / p.mountedTo.item.Size, Quaternion.identity, Vector3.one);

        else
            tr *= Matrix4x4.TRS(p.mountPosFlipped / p.mountedTo.item.Size, Quaternion.identity, Vector3.one);

        Vector3 sc = new Vector3(scale.x / (p.mountedTo.skeleton[p.mountLoc].bounds.width * p.mountedTo.item.Size), scale.y / (p.mountedTo.skeleton[p.mountLoc].bounds.height * p.mountedTo.item.Size), 0);
        tr *= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale / p.mountedTo.item.Size);

        tr *= Matrix4x4.TRS(new Vector3(-p.item.animList[0].boundsList[0].x, -p.item.animList[0].boundsList[0].y, 0), Quaternion.identity, Vector3.one);
        if (!p.flipped)
            tr *= Matrix4x4.TRS(new Vector3(p.item.animList[0].boundsList[0].x, p.item.animList[0].boundsList[0].y, 0), Quaternion.AngleAxis(p.mountRot, Vector3.forward), Vector3.one);
        else
            tr *= Matrix4x4.TRS(new Vector3(p.item.animList[0].boundsList[0].x, p.item.animList[0].boundsList[0].y, 0), Quaternion.AngleAxis(90 - p.mountRot, Vector3.forward), Vector3.one);
        tr *= Matrix4x4.TRS(new Vector3(-p.item.animList[0].boundsList[0].x, -p.item.animList[0].boundsList[0].y, 0), Quaternion.identity, Vector3.one);



        Graphics.DrawMesh(meshs[i], tr, OmniAtlas.texture, 0);

    }

    public void drawSkelMounted(DamageableObject owner, int i, Vector2 scale)
    {
        Vector2 ownerpos = owner.mountedTo.Position;
        ownerpos.x -= boundsPosOffset.x * scale.x;
        ownerpos.y -= boundsPosOffset.y * scale.y;



        Matrix4x4 tr = owner.mountedTo.item.animList[0].skelMatrix(owner.mountedTo,owner.mountedTo.Position, owner.mountedTo.item.scale, owner.mountLoc);
  
        
        //tr = owner.mountedTo.skeleton[owner.mountLoc].worldTr;
        tr *= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale / owner.mountedTo.item.Size);
        float rotOffset = owner.mountedTo.skeleton[owner.mountLoc].mountPoint[0].z;

        Quaternion rot;

        if (owner.flipped)
            rot = Quaternion.AngleAxis(-owner.skeleton[i].rotation, Vector3.forward);
        else
            rot = Quaternion.AngleAxis(owner.skeleton[i].rotation, Vector3.forward);

        
 



        tr *= owner.mountedTo.skeleton[owner.mountLoc].mountMatrix[0];
       

        
        tr *= Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0), Quaternion.AngleAxis(owner.rotation+rotOffset, Vector3.forward), Vector3.one);
        tr *= Matrix4x4.TRS(new Vector3(-0.5f, -0.5f, 0), Quaternion.identity, Vector3.one);
        tr *= owner.skeleton[i].tr;
        tr *= Matrix4x4.TRS(owner.skeleton[i].rotationPoint, rot, Vector3.one);
        tr *= Matrix4x4.TRS(-owner.skeleton[i].rotationPoint, Quaternion.identity, Vector3.one);
        

        if (owner.flipped)
            Graphics.DrawMesh(meshsFlipped[i], tr, OmniAtlas.texture, 0);
        else
            Graphics.DrawMesh(meshs[i], tr, OmniAtlas.texture, 0);

    }

    public Matrix4x4 drawSkelMountedMatrix(OmniObject owner,Vector3 ownerpos,Vector2 scale, int i)
    {
        ownerpos.x -= boundsPosOffset.x * scale.x;
        ownerpos.y -= boundsPosOffset.y * scale.y;
        Matrix4x4 tr = owner.mountedTo.item.animList[0].skelMatrix(owner.mountedTo,ownerpos, owner.mountedTo.item.scale, owner.mountLoc);
        tr *= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale / owner.mountedTo.item.Size);
        float rotOffset = owner.mountedTo.skeleton[owner.mountLoc].mountPoint[0].z;
        Quaternion rot;

        if (owner.flipped)
            rot = Quaternion.AngleAxis(-owner.skeleton[i].rotation, Vector3.forward);
        else
            rot = Quaternion.AngleAxis(owner.skeleton[i].rotation, Vector3.forward);






        tr *= owner.mountedTo.skeleton[owner.mountLoc].mountMatrix[0];



        tr *= Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0), Quaternion.AngleAxis(owner.rotation + rotOffset, Vector3.forward), Vector3.one);
        tr *= Matrix4x4.TRS(new Vector3(-0.5f, -0.5f, 0), Quaternion.identity, Vector3.one);
        tr *= owner.skeleton[i].tr;
        tr *= Matrix4x4.TRS(owner.skeleton[i].rotationPoint, rot, Vector3.one);
        tr *= Matrix4x4.TRS(-owner.skeleton[i].rotationPoint, Quaternion.identity, Vector3.one);


        return tr;

    }

    public void drawSkeleton(OmniObject owner, float time,Vector3 scale)
    {

        //float maxtime = skeletonAnimation[skeletonAnimation.Length-1]
        //time = time % 
        Vector3 half = new Vector3(0.5f, 0.5f, 0);
        for (int i = 0; i < owner.skeleton.Length; i++)
        {

            if (owner.skeleton[i].noDraw)
            {
                continue;
            }
            Matrix4x4 tr;
            Quaternion rot;
            if (owner is DamageableObject)
            {
                DamageableObject p = owner as DamageableObject;
                if (p.mountLoc >= 0)
                {
                    if(p.isProjectile)
                        drawSkelMountedProj(p,i,scale);
                    else
                        drawSkelMounted(p, i, scale);
                    continue;
                }
            }
            Vector2 ownerpos = owner.Position;
            ownerpos.x -= boundsPosOffset.x * scale.x;
            ownerpos.y -= boundsPosOffset.y * scale.y;
            tr = Matrix4x4.TRS(ownerpos, Quaternion.identity, scale);

                //Quaternion rot;

                if (owner.flipped)
                    rot = Quaternion.AngleAxis(-owner.skeleton[i].rotation, Vector3.forward);
                else
                    rot = Quaternion.AngleAxis(owner.skeleton[i].rotation, Vector3.forward);

               



                //tr *= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
                tr *= Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0), Quaternion.AngleAxis(owner.rotation, Vector3.forward), Vector3.one);
                tr *= Matrix4x4.TRS(new Vector3(-0.5f, -0.5f, 0), Quaternion.identity, Vector3.one);
                tr *= owner.skeleton[i].tr;
                tr *= Matrix4x4.TRS(owner.skeleton[i].rotationPoint, rot, Vector3.one);
                tr *= Matrix4x4.TRS(-owner.skeleton[i].rotationPoint, Quaternion.identity, Vector3.one);

                owner.skeleton[i].worldTr = tr;

            if(owner.flipped)
                Graphics.DrawMesh(meshsFlipped[i], tr, OmniAtlas.texture, 0);
            else
                Graphics.DrawMesh(meshs[i], tr, OmniAtlas.texture, 0);
    }
        
    }

    public Matrix4x4 skelMatrix(OmniObject owner,Vector3 ownerpos, Vector3 scale, int i)
    {
        Matrix4x4 tr = Matrix4x4.identity;



            Vector3 half = new Vector3(0.5f, 0.5f, 0);

            Quaternion rot;
            
            ownerpos.x -= boundsPosOffset.x * scale.x;
            ownerpos.y -= boundsPosOffset.y * scale.y;


            tr = Matrix4x4.TRS(ownerpos, Quaternion.identity, scale);
            if (owner.flipped)
                rot = Quaternion.AngleAxis(-owner.skeleton[i].rotation, Vector3.forward);
            else
                rot = Quaternion.AngleAxis(owner.skeleton[i].rotation, Vector3.forward);
            tr *= Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0), Quaternion.AngleAxis(owner.rotation, Vector3.forward), Vector3.one);
            tr *= Matrix4x4.TRS(new Vector3(-0.5f, -0.5f, 0), Quaternion.identity, Vector3.one);
            tr *= owner.skeleton[i].tr;
            tr *= Matrix4x4.TRS(owner.skeleton[i].rotationPoint, rot, Vector3.one);
            tr *= Matrix4x4.TRS(-owner.skeleton[i].rotationPoint, Quaternion.identity, Vector3.one);



            return tr;

        

    }

    public Matrix4x4 skelMatrix2(OmniObject owner, Vector3 ownerpos, int i)
    {
        Matrix4x4 tr = Matrix4x4.identity;

        Vector3 scale = owner.item.scale;

        Vector3 half = new Vector3(0.5f, 0.5f, 0);

        Quaternion rot;

        ownerpos.x -= boundsPosOffset.x * scale.x;
        ownerpos.y -= boundsPosOffset.y * scale.y;


        tr = Matrix4x4.TRS(ownerpos, Quaternion.identity, scale);
        if (owner.flipped)
            rot = Quaternion.AngleAxis(-owner.skeleton[i].rotation, Vector3.forward);
        else
            rot = Quaternion.AngleAxis(owner.skeleton[i].rotation, Vector3.forward);
        tr *= Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0), Quaternion.AngleAxis(owner.rotation, Vector3.forward), Vector3.one);
        tr *= Matrix4x4.TRS(new Vector3(-0.5f, -0.5f, 0), Quaternion.identity, Vector3.one);
        tr *= owner.skeleton[i].tr;
        tr *= Matrix4x4.TRS(owner.skeleton[i].rotationPoint, rot, Vector3.one);
        tr *= Matrix4x4.TRS(-owner.skeleton[i].rotationPoint, Quaternion.identity, Vector3.one);
        tr *= owner.skeleton[i].mountMatrix[0];
        scale.x = 1f / scale.x;
        scale.y = 1f / scale.y;
        tr *= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);

        return tr;


    }

    public Matrix4x4 mountMatrix(OmniObject owner, float time, Vector3 scale)
    {
        Quaternion rot2 = Quaternion.AngleAxis(RotationOffset - 90, new Vector3(0, 0, 1));
        if (owner.flipped)
            rot2 = Quaternion.AngleAxis(RotationOffset, new Vector3(0, 0, 1));
        Matrix4x4 tr1 = owner.item.animList[0].skelMatrix2(owner, owner.Position, owner.heldId);
        Matrix4x4 tr2 = Matrix4x4.TRS(-mountPoint[0], Quaternion.identity, Vector3.one);
        Matrix4x4 tr3 = Matrix4x4.TRS(Vector3.zero, rot2, scale);
        Matrix4x4 tr = tr1;
        tr *= tr3;
        //tr *= Matrix4x4.TRS(new Vector3(mountPoint[0].x, mountPoint[0].y, 0), Quaternion.identity, Vector3.one);

        tr *= Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(owner.skeleton[owner.heldId].mountPoint[0].z, Vector3.back), Vector3.one);
        //
        if (owner.flipped)
            tr *= Matrix4x4.TRS(new Vector3(-(1 - mountPoint[0].x), -mountPoint[0].y, 0), Quaternion.identity, Vector3.one);
        else
            tr *= Matrix4x4.TRS(new Vector3(-(mountPoint[0].x), -mountPoint[0].y, 0), Quaternion.identity, Vector3.one);
        //tr *= Matrix4x4.TRS(new Vector3(-(mountPoint[0].x), -mountPoint[0].y, 0), Quaternion.identity, Vector3.one);
        return tr;
    }

    public void drawRot(OmniObject owner, float time, Vector3 scale, params Vector3[] mount)
    {
        Quaternion rot2 = Quaternion.AngleAxis(RotationOffset - 90, new Vector3(0, 0, 1));
        if(owner.flipped)
            rot2 = Quaternion.AngleAxis(RotationOffset, new Vector3(0, 0, 1));
        Matrix4x4 tr1 = owner.item.animList[0].skelMatrix2(owner, owner.Position, owner.heldId);
        Matrix4x4 tr2 = Matrix4x4.TRS(-mountPoint[0], Quaternion.identity, Vector3.one);
        Matrix4x4 tr3 = Matrix4x4.TRS(Vector3.zero, rot2, scale);
        Matrix4x4 tr = tr1;
        tr *= tr3;
        tr *= Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(owner.skeleton[owner.heldId].mountPoint[0].z, Vector3.back), Vector3.one);
        //
        if(owner.flipped)
            tr *= Matrix4x4.TRS(new Vector3(-(1-mountPoint[0].x), -mountPoint[0].y, 0), Quaternion.identity, Vector3.one);
        else
        tr *= Matrix4x4.TRS(new Vector3(-(mountPoint[0].x), -mountPoint[0].y, 0), Quaternion.identity, Vector3.one);
        
        if(owner.flipped)
            Graphics.DrawMesh(meshsFlipped[0], tr, OmniAtlas.texture, 0);
        else
        Graphics.DrawMesh(meshs[0], tr, OmniAtlas.texture, 0);
    }

    public void drawRotOld(OmniObject owner, float time, Vector3 scale, params Vector3[] mount)
    {

        float a = 1;
        if (owner.item is OmniHuman)
        {
            OmniHuman h = owner.item as OmniHuman;
            Matrix4x4 tr = Matrix4x4.identity;//Matrix4x4.TRS(owner.position, Quaternion.identity, owner.item.scale);
            if (!owner.flipped)
            {
                /*
                Quaternion rot = Quaternion.AngleAxis(owner.skeleton[h.RightArm].rotation, Vector3.forward);
                




                tr *= Matrix4x4.TRS(new Vector3(0.5f, 0.5f), Quaternion.AngleAxis(owner.rotation, new Vector3(0, 0, 1)), Vector3.one);
                tr *= Matrix4x4.TRS(new Vector3(-0.5f, -0.5f), Quaternion.identity, Vector3.one);


                tr *= owner.skeleton[h.RightArm].tr;
                tr *= Matrix4x4.TRS(owner.skeleton[h.RightArm].rotationPoint, rot, Vector3.one);
                tr *= Matrix4x4.TRS(-owner.skeleton[h.RightArm].rotationPoint, Quaternion.identity, Vector3.one);
                */
                Quaternion rot2 = Quaternion.AngleAxis(RotationOffset - 90, new Vector3(0, 0, 1));
                if (owner.mountedTo == null)
                    tr *= skelMatrix(owner, owner.Position, owner.item.scale, h.RightArm);
                else
                    tr *= drawSkelMountedMatrix(owner, owner.Position, owner.item.scale, h.RightArm);

                Vector3 sc = new Vector3(scale.x / owner.item.scale.x, scale.y / owner.item.scale.y, 0);

                if (mount.Length > 0)
                {
                    tr *= Matrix4x4.TRS(mount[0], Quaternion.AngleAxis(mount[0].z, Vector3.back), sc);
                }
                else
                tr *= Matrix4x4.TRS(owner.skeleton[h.RightArm].mountPoint[0], Quaternion.AngleAxis(owner.skeleton[h.RightArm].mountPoint[0].z, Vector3.back), sc);
                tr *= Matrix4x4.TRS(new Vector3(-(mountPoint[0].x), -mountPoint[0].y, 0), Quaternion.identity, Vector3.one);
                tr *= Matrix4x4.TRS(new Vector3(mountPoint[0].x, mountPoint[0].y, 0), rot2, Vector3.one);
                tr *= Matrix4x4.TRS(new Vector3(-(mountPoint[0].x), -mountPoint[0].y, 0), Quaternion.identity, Vector3.one);
                Graphics.DrawMesh(meshs[0], tr, OmniAtlas.texture, 0);
            }
            else
            {
                Quaternion rot = Quaternion.AngleAxis(-owner.skeleton[h.LeftArm].rotation, Vector3.forward);
                Quaternion rot2 = Quaternion.AngleAxis(-RotationOffset + 90, new Vector3(0, 0, 1));
                if (owner.mountedTo == null)
                    tr *= skelMatrix(owner, owner.Position, owner.item.scale, h.LeftArm);
                else
                    tr *= drawSkelMountedMatrix(owner, owner.Position, owner.item.scale, h.LeftArm);

                Vector3 sc = new Vector3(scale.x / owner.item.scale.x, scale.y / owner.item.scale.y, 0);
                if (mount.Length > 0)
                    tr *= Matrix4x4.TRS(mount[0], Quaternion.AngleAxis(-mount[0].z, Vector3.back), sc);
                else
                    tr *= Matrix4x4.TRS(owner.skeleton[h.LeftArm].mountPoint[0], Quaternion.AngleAxis(owner.skeleton[h.LeftArm].mountPoint[0].z, Vector3.back), sc);
                tr *= Matrix4x4.TRS(new Vector3(-1 + mountPoint[0].x, -mountPoint[0].y, 0), Quaternion.identity, Vector3.one);
                tr *= Matrix4x4.TRS(new Vector3(1 - mountPoint[0].x, mountPoint[0].y, 0), rot2, Vector3.one);
                tr *= Matrix4x4.TRS(new Vector3(-(1 - mountPoint[0].x), -mountPoint[0].y, 0), Quaternion.identity, Vector3.one);
                Graphics.DrawMesh(meshsFlipped[0], tr, OmniAtlas.texture, 0);
            }
            

        }

        
    }


    public Matrix4x4 GetBoundsMatrix(OmniObject owner, Vector3 scale, int k, int skel)
    {



            int skelLoc = skel;

            Vector3 boundsScale = new Vector3(boundsList[k].width, boundsList[k].height);
            Vector3 bOffset = boundsPos[k];
            Vector3 ownerPos = new Vector3(owner.bounds.x, owner.bounds.y, 0);

            if (skel < 0)
            {
                skelLoc = 0;
            }

            Matrix4x4 tr = Matrix4x4.identity;//Matrix4x4.TRS(ownerPos, Quaternion.identity, owner.item.scale);
//            if (owner.item is OmniHuman)
//            {
//                OmniHuman h = owner.item as OmniHuman;

                if (!owner.flipped)
                {
   
                    Quaternion rot2 = Quaternion.AngleAxis(RotationOffset - 90, new Vector3(0, 0, 1));
                    Quaternion rot3 = Quaternion.AngleAxis(-(RotationOffset - 90), new Vector3(0, 0, 1));
                    //Quaternion rot4 = 




                    //tr *= Matrix4x4.TRS(new Vector3(0.5f, 0.5f), Quaternion.AngleAxis(owner.rotation, new Vector3(0, 0, 1)), Vector3.one);
                    //tr *= Matrix4x4.TRS(new Vector3(-0.5f, -0.5f), Quaternion.identity, Vector3.one);


                    if (owner.mountedTo == null)
                        tr *= skelMatrix(owner, owner.lastPos, owner.item.scale, skelLoc);
                    else
                        tr *= drawSkelMountedMatrix(owner, owner.lastPos, owner.item.scale, skelLoc);

                    //tr *= Matrix4x4.TRS(owner.skeleton[skelLoc].rotationPoint, rot, Vector3.one);
                    //tr *= Matrix4x4.TRS(-owner.skeleton[skelLoc].rotationPoint, Quaternion.identity, Vector3.one);
                    Vector3 sc = new Vector3(scale.x / owner.item.scale.x, scale.y / owner.item.scale.y, 0);
                    tr *= Matrix4x4.TRS(owner.skeleton[skelLoc].mountPoint[0], Quaternion.identity, sc);
                    tr *= Matrix4x4.TRS(new Vector3(-(mountPoint[0].x), -mountPoint[0].y, 0), Quaternion.identity, Vector3.one);
                    tr *= Matrix4x4.TRS(new Vector3(mountPoint[0].x, mountPoint[0].y, 0), rot2, Vector3.one);
                    tr *= Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(owner.skeleton[skelLoc].mountPoint[0].z, Vector3.back), Vector3.one);
                    tr *= Matrix4x4.TRS(new Vector3(-(mountPoint[0].x), -mountPoint[0].y, 0), Quaternion.identity, Vector3.one);
                    tr *= Matrix4x4.TRS(bOffset, Quaternion.identity, boundsScale);
                    if (skel >= 0)
                    {
                        tr *= Matrix4x4.TRS(new Vector3(0.5f, 0.5f), Quaternion.AngleAxis(-owner.skeleton[skelLoc].rotation - (RotationOffset - 90), new Vector3(0, 0, 1)), Vector3.one);
                        tr *= Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(-owner.skeleton[skelLoc].mountPoint[0].z, Vector3.back), Vector3.one);
                        tr *= Matrix4x4.TRS(new Vector3(-0.5f, -0.5f), Quaternion.identity, Vector3.one);
                    }

                }
                else
                {
                    Quaternion rot = Quaternion.AngleAxis(-owner.skeleton[skelLoc].rotation, Vector3.forward);
                    Quaternion rot2 = Quaternion.AngleAxis(-RotationOffset + 90, new Vector3(0, 0, 1));
                    Quaternion rot4 = Quaternion.AngleAxis(owner.skeleton[skelLoc].rotation + RotationOffset - 90, new Vector3(0, 0, 1));
                    if (owner.mountedTo == null)
                        tr *= skelMatrix(owner, owner.lastPos, owner.item.scale, skelLoc);
                    else
                        tr *= drawSkelMountedMatrix(owner, owner.lastPos, owner.item.scale, skelLoc);


                    Vector3 sc = new Vector3(scale.x / owner.item.scale.x, scale.y / owner.item.scale.y, 0);
                    tr *= Matrix4x4.TRS(owner.skeleton[skelLoc].mountPoint[0], Quaternion.AngleAxis(owner.skeleton[skelLoc].mountPoint[0].z, Vector3.back), sc);
                    tr *= Matrix4x4.TRS(new Vector3(-1 + mountPoint[0].x, -mountPoint[0].y, 0), Quaternion.identity, Vector3.one);
                    tr *= Matrix4x4.TRS(new Vector3(1 - mountPoint[0].x, mountPoint[0].y, 0), rot2, Vector3.one);
                    tr *= Matrix4x4.TRS(new Vector3(-(1 - mountPoint[0].x), -mountPoint[0].y, 0), Quaternion.identity, Vector3.one);
                    bOffset.x = 1 - bOffset.x;
                    tr *= Matrix4x4.TRS(bOffset, Quaternion.identity, boundsScale);
                    tr *= Matrix4x4.TRS(Vector3.left, Quaternion.identity, Vector3.one);
                    tr *= Matrix4x4.TRS(new Vector3(0.5f, 0.5f), rot4, Vector3.one);
                    tr *= Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(-owner.skeleton[skelLoc].mountPoint[0].z, Vector3.back), Vector3.one);

                    tr *= Matrix4x4.TRS(new Vector3(-0.5f, -0.5f), Quaternion.identity, Vector3.one);

                }

/*
            }
            else
            {
                tr *= Matrix4x4.TRS(new Vector3(0.5f, 0.5f), Quaternion.AngleAxis(owner.rotation, new Vector3(0, 0, 1)), Vector3.one);
                tr *= Matrix4x4.TRS(new Vector3(-0.5f, -0.5f), Quaternion.identity, Vector3.one);
                tr *= Matrix4x4.TRS(bOffset, Quaternion.identity, boundsScale);

            }
*/
            return tr;




    }


    public void GetBoundsPos(PhysicsObject owner, Vector3 scale, float time)
    {

        for (int k = 0; k < boundsList.Length; k++)
        {
            /*
            Vector3 boundsScale = new Vector3(boundsList[k].width, boundsList[k].height);
            Vector3 bOffset = boundsPos[k];
            Vector3 ownerPos = new Vector3(owner.bounds.x, owner.bounds.y, 0);
            Matrix4x4 tr = Matrix4x4.identity;//Matrix4x4.TRS(ownerPos, Quaternion.identity, owner.item.scale);
            if (owner.item is OmniHuman)
            {
                OmniHuman h = owner.item as OmniHuman;

                if (!owner.flipped)
                {
                    Quaternion rot = Quaternion.AngleAxis(owner.skeleton[h.RightArm].rotation, Vector3.forward);
                    Quaternion rot2 = Quaternion.AngleAxis(RotationOffset - 90, new Vector3(0, 0, 1));
                    Quaternion rot3 = Quaternion.AngleAxis(-(RotationOffset - 90), new Vector3(0, 0, 1));
                    Quaternion rot4 = Quaternion.AngleAxis(-owner.skeleton[h.RightArm].rotation - (RotationOffset - 90), new Vector3(0, 0, 1));




                    //tr *= Matrix4x4.TRS(new Vector3(0.5f, 0.5f), Quaternion.AngleAxis(owner.rotation, new Vector3(0, 0, 1)), Vector3.one);
                    //tr *= Matrix4x4.TRS(new Vector3(-0.5f, -0.5f), Quaternion.identity, Vector3.one);


                    if (owner.mountedTo == null)
                        tr *= skelMatrix(owner, owner.lastPos, owner.item.scale, h.RightArm);
                    else
                        tr *= drawSkelMountedMatrix(owner, owner.lastPos, owner.item.scale, h.RightArm);

                    //tr *= Matrix4x4.TRS(owner.skeleton[h.RightArm].rotationPoint, rot, Vector3.one);
                    //tr *= Matrix4x4.TRS(-owner.skeleton[h.RightArm].rotationPoint, Quaternion.identity, Vector3.one);
                    Vector3 sc = new Vector3(scale.x / owner.item.scale.x, scale.y / owner.item.scale.y, 0);
                    tr *= Matrix4x4.TRS(owner.skeleton[h.RightArm].mountPoint[0], Quaternion.identity, sc);
                    tr *= Matrix4x4.TRS(new Vector3(-(mountPoint[0].x), -mountPoint[0].y, 0), Quaternion.identity, Vector3.one);
                    tr *= Matrix4x4.TRS(new Vector3(mountPoint[0].x, mountPoint[0].y, 0), rot2, Vector3.one);
                    tr *= Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(owner.skeleton[h.RightArm].mountPoint[0].z, Vector3.back), Vector3.one);
                    tr *= Matrix4x4.TRS(new Vector3(-(mountPoint[0].x), -mountPoint[0].y, 0), Quaternion.identity, Vector3.one);
                    tr *= Matrix4x4.TRS(bOffset, Quaternion.identity, boundsScale);
                    tr *= Matrix4x4.TRS(new Vector3(0.5f, 0.5f), rot4, Vector3.one);
                    tr *= Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(-owner.skeleton[h.RightArm].mountPoint[0].z, Vector3.back), Vector3.one);
                    tr *= Matrix4x4.TRS(new Vector3(-0.5f, -0.5f), Quaternion.identity, Vector3.one);

                }
                else
                {
                    Quaternion rot = Quaternion.AngleAxis(-owner.skeleton[h.LeftArm].rotation, Vector3.forward);
                    Quaternion rot2 = Quaternion.AngleAxis(-RotationOffset + 90, new Vector3(0, 0, 1));
                    Quaternion rot4 = Quaternion.AngleAxis(owner.skeleton[h.LeftArm].rotation + RotationOffset - 90, new Vector3(0, 0, 1));
                    if (owner.mountedTo == null)
                        tr *= skelMatrix(owner, owner.lastPos, owner.item.scale, h.LeftArm);
                    else
                        tr *= drawSkelMountedMatrix(owner, owner.lastPos, owner.item.scale, h.LeftArm);


                    Vector3 sc = new Vector3(scale.x / owner.item.scale.x, scale.y / owner.item.scale.y, 0);
                    tr *= Matrix4x4.TRS(owner.skeleton[h.LeftArm].mountPoint[0], Quaternion.AngleAxis(owner.skeleton[h.LeftArm].mountPoint[0].z, Vector3.back), sc);
                    tr *= Matrix4x4.TRS(new Vector3(-1 + mountPoint[0].x, -mountPoint[0].y, 0), Quaternion.identity, Vector3.one);
                    tr *= Matrix4x4.TRS(new Vector3(1 - mountPoint[0].x, mountPoint[0].y, 0), rot2, Vector3.one);
                    tr *= Matrix4x4.TRS(new Vector3(-(1 - mountPoint[0].x), -mountPoint[0].y, 0), Quaternion.identity, Vector3.one);
                    bOffset.x = 1 - bOffset.x;
                    tr *= Matrix4x4.TRS(bOffset, Quaternion.identity, boundsScale);
                    tr *= Matrix4x4.TRS(Vector3.left, Quaternion.identity, Vector3.one);
                    tr *= Matrix4x4.TRS(new Vector3(0.5f, 0.5f), rot4, Vector3.one);
                    tr *= Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(-owner.skeleton[h.LeftArm].mountPoint[0].z, Vector3.back), Vector3.one);

                    tr *= Matrix4x4.TRS(new Vector3(-0.5f, -0.5f), Quaternion.identity, Vector3.one);

                }


            }
            else
            {
                    tr *= Matrix4x4.TRS(new Vector3(0.5f, 0.5f), Quaternion.AngleAxis(owner.rotation, new Vector3(0, 0, 1)), Vector3.one);
                    tr *= Matrix4x4.TRS(new Vector3(-0.5f, -0.5f), Quaternion.identity, Vector3.one);
                    tr *= Matrix4x4.TRS(bOffset, Quaternion.identity, boundsScale);

            }
            */
            Matrix4x4 tr = GetBoundsMatrix(owner, scale, k, owner.heldId);

            Vector3 ret = new Vector3();


            ret = tr.MultiplyPoint(ret);


            owner.itemBounds[k].width = scale.x * boundsList[k].width;
            owner.itemBounds[k].height = scale.y * boundsList[k].height;


            owner.itemBounds[k].x = ret.x;// -(owner.itemBounds[k].width / 2f);
            owner.itemBounds[k].y = ret.y;// -(owner.itemBounds[k].height / 2f);

            owner.itemBoundsVel[k].x = (ret.x + owner.itemBounds[k].width / 2) - (owner.itemBoundsLastVel[k].x + owner.itemBounds[k].width / 2);
            owner.itemBoundsVel[k].y = (ret.y + owner.itemBounds[k].height / 2) - (owner.itemBoundsLastVel[k].y + owner.itemBounds[k].height / 2);


            owner.itemBoundsLastVel[k].x = ret.x;
            owner.itemBoundsLastVel[k].y = ret.y;
            owner.itemMatrix[k] = tr;
            

           
        }
        
    }

  


    void genMesh()
    {


        if (animationType == Type.Skeleton)
        {
            for (int i = 0; i < skeletonRects.Length; i++)
            {

                meshs[i] = new Mesh();
                Vector3[] verticies = new Vector3[4] {
			new Vector3 (0, 0, 0),
			new Vector3 (0, skeletonRects[i].bounds.height, 0),
			new Vector3 (skeletonRects[i].bounds.width, skeletonRects[i].bounds.height, 0),
			new Vector3 (skeletonRects[i].bounds.width, 0, 0)
			};

                float texX = skeletonRects[i].texBounds.x;
                float texY = skeletonRects[i].texBounds.y;
                Vector2[] uv = new Vector2[4] {
			
			new Vector2 (texX, texY),
			new Vector2 (texX, texY+skeletonRects[i].texBounds.height),
			new Vector2 (texX+skeletonRects[i].texBounds.width, texY+skeletonRects[i].texBounds.height),
			new Vector2 (texX+skeletonRects[i].texBounds.width, texY)
			};

                int[] triangles = new int[6] {
			0,1,2,
			2,3,0
		};
                meshs[i].vertices = verticies;
                meshs[i].uv = uv;
                meshs[i].triangles = triangles;
            }
        }
        else
        {

            for (int i = 0; i < frames; i++)
            {
                meshs[i] = new Mesh();
                Vector3 scale = transform.localScale;
                Vector3 pos = transform.localPosition;

                Vector3[] verticies = new Vector3[4] {
			new Vector3 (0, 0, 0),
			new Vector3 (0, 1, 0),
			new Vector3 (1, 1, 0),
			new Vector3 (1, 0, 0)
			};
                float fwidth = width / atlas.width;
                float texX = aTexture.coords.x + fwidth * i;
                float texY = aTexture.coords.y;
                Vector2[] uv = new Vector2[4] {
			
			new Vector2 (texX, texY),
			new Vector2 (texX, texY+aTexture.coords.height),
			new Vector2 (texX + fwidth, texY+aTexture.coords.height),
			new Vector2 (texX + fwidth, aTexture.coords.y)
			};
                int[] triangles = new int[6] {
			0,1,2,
			2,3,0
		};
                meshs[i].vertices = verticies;
                meshs[i].uv = uv;
                meshs[i].triangles = triangles;
            }

        }

    }
	void genMeshFlipped() {
        if (animationType == Type.Skeleton)
        {
            for (int i = 0; i < skeletonRects.Length; i++)
            {

                meshsFlipped[i] = new Mesh();
                Vector3[] verticies = new Vector3[4] {
			new Vector3 (0, 0, 0),
			new Vector3 (0, skeletonRects[i].bounds.height, 0),
			new Vector3 (skeletonRects[i].bounds.width, skeletonRects[i].bounds.height, 0),
			new Vector3 (skeletonRects[i].bounds.width, 0, 0)
			};

                float texX = skeletonRects[i].texBounds.x;
                float texY = skeletonRects[i].texBounds.y;
                Vector2[] uv = new Vector2[4] {
			
			new Vector2 (texX+skeletonRects[i].texBounds.width, texY),
			new Vector2 (texX+skeletonRects[i].texBounds.width, texY+skeletonRects[i].texBounds.height),
			new Vector2 (texX, texY+skeletonRects[i].texBounds.height),
			new Vector2 (texX, texY)
			};

                int[] triangles = new int[6] {
			0,1,2,
			2,3,0
		};
                meshsFlipped[i].vertices = verticies;
                meshsFlipped[i].uv = uv;
                meshsFlipped[i].triangles = triangles;
            }
        }
        else
        {
            for (int i = 0; i < frames; i++)
            {
                meshsFlipped[i] = new Mesh();
                Vector3 scale = transform.localScale;
                Vector3 pos = transform.localPosition;

                Vector3[] verticies = new Vector3[4] {
			new Vector3 (0, 0, 0),
			new Vector3 (0, 1, 0),
			new Vector3 (1, 1, 0),
			new Vector3 (1, 0, 0)
		};
                float fwidth = width / atlas.width;
                float texX = aTexture.coords.x + fwidth * i;
                float texY = aTexture.coords.y;
                Vector2[] uv = new Vector2[4] {
			new Vector2 (texX+fwidth, texY),
			new Vector2 (texX+fwidth, aTexture.coords.yMax),
			new Vector2 (texX, aTexture.coords.yMax),
			new Vector2 (texX, aTexture.coords.yMin)
		};
                int[] triangles = new int[6] {
			0,1,2,
			2,3,0
		};
                meshsFlipped[i].vertices = verticies;
                meshsFlipped[i].uv = uv;
                meshsFlipped[i].triangles = triangles;
            }
        }
	}
	// Update is called once per frame
	void Update () {


	}
}
