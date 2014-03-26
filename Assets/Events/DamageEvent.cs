using UnityEngine;
using System.Collections.Generic;



public class DamageEvent : NetworkEvent
{

    public OmniObject damaged;
    public OmniObject attacker;
    public OmniItemType itemType;
    public Vector2 vel;
    public Vector2 hp;

    public DamageEvent(int tick)
        : base(tick)
    {
        type = NetworkSendType.ServerCommandAll;
        
    }

    public DamageEvent(int tick, OmniObject damaged, OmniObject attacker, OmniItemType itemType,Vector2 vel)
        : base(tick)
    {
        this.damaged = damaged;
        this.attacker = attacker;
        this.itemType = itemType;
        this.vel = vel;
        type = NetworkSendType.ServerCommandAll;
        if (Network.isServer)
            hp = new Vector2(damaged.HP, attacker.HP);

      
    }

    public override void handle(int tick)
    {
        base.handle(tick);

        if (handled)
        {
            Vector2 v = Vector2.zero;
            if(fromServer || Network.isServer)
                if (hp != null)
                {
                    damaged.HP = hp.x;
                    attacker.HP = hp.y;
                    v = vel;
                }

            damaged.item.OnDamage(damaged, attacker, itemType, v);


        }
    }

}

