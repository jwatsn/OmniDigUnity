using UnityEngine;
using System.IO;

public class BlockEvent : NetworkEvent {

    public tUpdate[] updates;
    bool flag = false;
    int updateMask = 0;
    public BlockEvent(int tick,tUpdate[] updates) : base(tick)
    {
        this.updates = updates;
        this.type = NetworkSendType.ServerCommandAll;
	}

    public BlockEvent(int tick)
        : base(tick)
    {
    }

    public override void handle(int tick)
    {
        base.handle(tick);
        if (handled)
        {
            OmniTerrain.blockUpdate(this);
        }
    }

    public void handleOld(int tick)
    {

        //base.handle(tick);
        if (tick >= this.tick)
        {
            if (Network.isClient)
                handled = false;
            else
                handled = true;

            if (!flag)
            {
                OmniTerrain.blockUpdate(this);              
                flag = true;
            }
            if (fromServer)
            {
                handled = true;
            }
            else
            {
                if (tick - this.tick > 20)
                {
                    for (int i = 0; i < updates.Length; i++)
                    {
                        if ((updateMask & 1 << i) == 0)
                        {
                            OmniTerrain.blockReverse(updates[i]);
                        }
                    }
                    OmniEvents.toValidate.Remove(this);
                    handled = true;
                }
            }

        }
    }

    public override void validate(OmniEvent e)
    {
        base.validate(e);

        if (e is BlockEvent)
        {

            BlockEvent bl = e as BlockEvent;
            for (int i = 0; i < updates.Length; i++)
            {
                for (int j = 0; j < bl.updates.Length; j++)
                {
                    if (bl.updates[j].x == updates[i].x && bl.updates[j].y == updates[i].y)
                    {
                        updateMask |= 1 << i;
                    }
                }
            }

        }
    }
}

