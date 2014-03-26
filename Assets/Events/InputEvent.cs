using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public static class InputType
{
    public const int LeftPress = 0;
    public const int LeftRelease = 1;
    public const int RightPress = 2;
    public const int RightRelease = 3;
    public const int UpPress = 4;
    public const int UpRelease = 5;
    public const int MouseButton0Press = 6;
    public const int MouseButton0Release = 7;
}

public class InputEvent : NetworkEvent {


	public int mask;
    public ClientControllable owner;
    public Vector2 clickPos;
    float angle;
    float length;

    public InputEvent(int tick)
        : base(tick)
    {
        type = NetworkSendType.ClientUnreliable;
    }

    public InputEvent(int tick, ClientControllable owner, int mask, params Vector2[] args)
        : base(tick)
    {
		this.mask = mask;
        type = NetworkSendType.ClientUnreliable;
		this.owner = owner;

        if (args.Length > 0)
        {
            clickPos = args[0];
        }

//        if(Network.isClient)
//         OmniNetwork.getView().RPC("RemoteInput", RPCMode.Server, Network.player, mask, tick,clickPos);
	}

    public override void init()
    {
        if (Network.isServer && owner.id != OmniLocal.LocalID)
            tick += owner.delay;

        base.init();
    }

    public override void handle(int tick)
    {
        base.handle(tick);
        if (handled)
        {
            owner.inputMask = mask;
            if (clickPos != null)
                owner.clickPos = clickPos;

            if (owner.mountedTo != null)
                owner.mountedTo.inputMask = mask;
        }
    }

    /*
	public override void handle(int tick) {
		base.handle (tick);

		if (handled) {
			switch(type) {
			case InputType.LeftPress:
				target.Left = true;
				break;
                case InputType.LeftRelease:
				target.Left = false;
				break;

			case InputType.RightPress:
				target.Right = true;
				break;
            case InputType.RightRelease:
				target.Right = false;
				break;

            case InputType.UpPress:
				target.Up = true;
				break;
            case InputType.UpRelease:
				target.Up = false;
				break;
            case InputType.MouseButton0Press:
                target.Clicked = true;
                target.ClickPos = clickPos;
                break;
            case InputType.MouseButton0Release:
                target.Clicked = false;
                break;
                 


			}
		}
    

	}
*/
}
