using System;
using System.Collections.Generic;
using UnityEngine;

public class ContainerObject : DamageableObject
{

    public OmniItemObject[] bagItems;
    public OmniItemObject[] equiptItems;
    public int selected = -1;
    public int Width = 6;
    public int Height = 4;

    public ContainerObject(int id, int itemId)
        : base(id, itemId)
    {
        bagItems = new OmniItemObject[Width * Height];
        equiptItems = new OmniItemObject[Width * Height];
    }


    public void TryEquiptItem(int id)
    {
        if (bagItems[id] == null)
            return;
        if (bagItems[id].stack <= 0)
            return;
        if (!(bagItems[id].type is OmniArmor))
            return;

        OmniArmor armor = (OmniArmor)bagItems[id].type;
        switch (armor.location)
        {
            case ArmorLocation.Head:
                equiptItems[0] = bagItems[id];
                break;
            case ArmorLocation.Body:
                equiptItems[1] = bagItems[id];
                break;
            case ArmorLocation.Legs:
                equiptItems[2] = bagItems[id];
                break;
            case ArmorLocation.Feet:
                equiptItems[3] = bagItems[id];
                break;
        }
    }

    public void AddItemToBag(string name, int amount)
    {

        int i = 0;
        OmniItemType item = OmniItems.items[OmniItems.getItem(name)].GetComponent<OmniItemType>();



        while (amount > 0)
        {

            OmniItemObject obj = bagItems[i];

            if (obj == null)
            {

                if (amount > item.max_stack)
                    bagItems[i] = new OmniItemObject(item, item.max_stack);
                else
                    bagItems[i] = new OmniItemObject(item, amount);
                amount -= item.max_stack;

            }

            else if (obj.name == name)
            {

                int stack_limit = item.max_stack;

                if (obj.stack < stack_limit)
                {

                    if (obj.stack + amount > stack_limit)
                    {
                        amount -= stack_limit - obj.stack;
                        obj.stack = item.max_stack;
                    }
                    else
                    {
                        obj.stack += amount;
                        amount = 0;
                    }

                }
            }

            else if (obj.stack == 0)
            {
                if (amount > item.max_stack)
                    bagItems[i] = new OmniItemObject(item, item.max_stack);
                else
                    bagItems[i] = new OmniItemObject(item, amount);

                amount -= item.max_stack;
            }


            i++;
            if (i > OmniInventory.Bag_MaxItems)
                i = 0;

        }

    }

    public void TryMoveHeld(int from, int to, int amt)
    {

        OmniItemObject item = bagItems[to];
        if (from == to)
        {
            return;
        }


        if (item == null)
        {

            if (bagItems[from].stack - amt > 0)
            {
                bagItems[from].stack -= amt;
                bagItems[to] = new OmniItemObject(item.type, amt);
            }
            else
            {
                bagItems[to] = bagItems[from];
                bagItems[from] = null;

            }
            return;
        }

        if (item.name == bagItems[from].name)
        {

            if (item.stack + amt > item.type.max_stack)
            {



                bagItems[from].stack -= item.type.max_stack - item.stack;

                item.stack = item.type.max_stack;
                return;
            }
            else
            {
                item.stack += amt;
                bagItems[from] = null;
                return;
            }

        }
        else
        {

            if (bagItems[from].stack - amt <= 0)
            {

                bagItems[to] = bagItems[from];
                bagItems[from] = item;
                return;

            }

        }


    }
}
