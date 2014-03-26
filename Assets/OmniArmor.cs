using UnityEngine;
using System;
using System.Collections;

[Serializable]
public enum ArmorLocation
{
    Head,
    Body,
    Legs,
    Feet
}

public class OmniArmor : OmniItemType {

    public ArmorLocation location;
}
