using System;
using UnityEngine;


public class InventoryEvent : NetworkEvent
{

    

    public InventoryEvent(int tick)
        : base(tick)
    {
    }

    public char updateType;
    public int ownerId = -1;
    public int[] args;
    public InventoryEvent(int tick,int id, char updateType, params int[] args)
    :base(tick)
    {
        this.updateType = updateType;
        this.ownerId = id;
        this.args = args;
        
        switch (updateType)
        {
            case 'm': //move selection
            case 's': // Set Quickbar selection
                type = NetworkSendType.ClientUnreliable;
                break;
            case 'a': // Add Item
                type = NetworkSendType.ServerCommandSingle;
                break;


            

        }

    }

    public override void handle(int tick)
    {
        base.handle(tick);
        if (handled)
        {
            if (argsCorrect(updateType))
            {
                switch (updateType)
                {
                    case 's':
                        int sel = (int)args[0];
                        if (OmniWorld.instance.SpawnedObjectsNew[ownerId] is ContainerObject)
                            ((ContainerObject)OmniWorld.instance.SpawnedObjectsNew[ownerId]).selected = sel;
                        break;
                    case 'a':
                        AddItems();
                        break;
                    case 'm':
                        int itemid = (int)args[0];
                        int s = (int)args[1];
                        int amt = (int)args[2];
                        if (OmniWorld.instance.SpawnedObjectsNew[ownerId] is ContainerObject)
                            ((ContainerObject)OmniWorld.instance.SpawnedObjectsNew[ownerId]).TryMoveHeld(itemid, s, amt);
                        break;
                }
            }
            else
            {
                Debug.Log("INV ARGUMENTS WRONG");
            }
        }
    }

    void AddItems()
    {
        for (int i = 0; i < args.Length-1; i+=2)
        {
            int item = args[i];
            int amt = args[i+1];
            if (OmniWorld.instance.SpawnedObjectsNew[ownerId] is ContainerObject)
                ((ContainerObject)OmniWorld.instance.SpawnedObjectsNew[ownerId]).AddItemToBag(OmniItems.itemTypes[item].name, amt);
        }
    }

    bool argsCorrect(char type)
    {
        switch (type)
        {
            case 'm':
            case 's':
                if (args.Length > 0)
                        return true;
                break;
            case 'a':
                if (args.Length > 0)
                    if (args.Length % 2 == 0)
                        return true;
                break;

        }

        return false;
    }

}
