using UnityEngine;
using System;
using System.Collections;
using System.Reflection;

public class SpawnEvent : NetworkEvent {
	
    public System.Type classType;

    //NETWORK VALUES
    public string target;
    public string name;
    public int id = -1;
    public Vector2 pos,vel;
    public bool projectile;
    public OmniObject firedBy;


    public SpawnEvent(int tick)
        : base(tick)
    {
        type = NetworkSendType.ServerCommandAll;

    }

	public SpawnEvent(int tick, string target,string name, Vector2 pos,Vector2 vel, int id,params System.Type[] components) : base(tick) {

        type = NetworkSendType.ServerCommandAll;

		this.target = target;
        this.pos = pos;
        this.vel = vel;
        this.id = id;
        this.name = name;
        if (this.id < 0)
            this.id = OmniWorld.GetSpawnID();

        if(components.Length>0)
            this.classType = components[0];
	}

    public SpawnEvent(int tick, string target, Vector2 pos, Vector2 vel, int id,bool projectile, params System.Type[] components)
        : base(tick)
    {

        type = NetworkSendType.ServerCommandAll;

        this.target = target;
        this.pos = pos;
        this.vel = vel;
        this.id = id;
        if (this.id < 0)
            this.id = OmniWorld.GetSpawnID();

        if (components.Length > 0)
            this.classType = components[0];


        this.projectile = projectile;
    }

    public override void init()
    {
        base.init();


    }

	public override void handle (int tick)
	{
		base.handle (tick);
		
		if(handled) {

            OmniObject obj = null;
            bool ghostflag = false;
            if (Network.isClient)
            {

                    if(id != OmniLocal.LocalID)
                        if(!projectile)
                            ghostflag = true;

                   

            }


            if (typeof(OmniObject).IsAssignableFrom(classType))
            {
                obj = Activator.CreateInstance(classType, id, OmniItems.getItem(target)) as OmniObject;
            
            }
            if (name != null)
                obj.Name = name;
            if (ghostflag)
            {
                obj.isGhost = true;
                obj.ghost = new GhostObject();
                obj.ghost.owner = obj;
            }


            obj.setPos(pos.x, pos.y);


            OmniWorld.AddToSpawnListNew(obj, id);

            if (projectile)
            {
                obj.animType = OmniAnimation.Type.Frame;
                obj.rotation = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;   
            }

            if(firedBy != null)
            obj.firedBy = firedBy.id;

            if (obj is PhysicsObject)
            {
                if (projectile)
                    ((PhysicsObject)obj).isProjectile = true;
                ((PhysicsObject)obj).vel = vel;
            }
            

		}
	}
	
}
