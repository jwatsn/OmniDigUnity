using System;
using System.Collections.Generic;
using UnityEngine;

public class ChatEvent : NetworkEvent
{



    public int senderId;
    public string message;

    public ChatEvent(int tick)
        :base(tick)
    {
        type = NetworkSendType.Everyone;
    }

    public ChatEvent(int tick, int senderId, string msg)
        : base(tick)
    {
        type = NetworkSendType.Everyone;
        this.senderId = senderId;
        this.message = msg;
    }

    public override void handle(int tick)
    {
        base.handle(tick);
        if (handled)
        {
            OmniChatBox.Add(new chatMessage(senderId, message));
        }
    }
}
