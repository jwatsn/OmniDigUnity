using System;
using System.Collections.Generic;
using UnityEngine;

public class OmniSuperPickaxe : OmniPickaxe
{

    public override void AltFire(ControllableObject player)
    {
        if (player.grounded)
            return;
        player.stun = 2;
        player.rotSpeed = 1080;

        if (!player.flipped)
            player.rotSpeed *= -1;


        player.mount0 = id;
    }

}