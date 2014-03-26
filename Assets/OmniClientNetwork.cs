using System;
using System.Collections.Generic;
using UnityEngine;

public class OmniClientNetwork : MonoBehaviour
{
    OmniNetwork network;

    void Start()
    {
        network = GetComponent<OmniNetwork>();
        
    }

    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {

        int tick = 0;
        float clientLookRot = 0;
        float clientFireRot = 0;
        bool flipped = false;

        if (Network.isServer && stream.isReading || Network.isClient && stream.isWriting)
        {
            if (Network.isClient)
            {
                if (OmniLocal.LocalID >= 0 && OmniLocal.LocalID < OmniWorld.instance.SpawnedObjectsNew.Count)
                {
                    OmniLocal.instance.NetworkUpdate();
                    ControllableObject o = OmniLocal.getLocalPlayer();
                    if (o != null)
                    {
                        clientLookRot = o.lookTo;
                        clientFireRot = o.fireRot;
                        flipped = o.flipped;
                        tick = OmniWorld.tick;
                    }
                }
            }


            stream.Serialize(ref clientLookRot);
            stream.Serialize(ref clientFireRot);
            stream.Serialize(ref tick);
            stream.Serialize(ref flipped);
            if (Network.isServer)
            {
                if (network.connectedClients.ContainsKey(info.sender))
                {
                    int delay = network.connectedClients[info.sender].delay;
                    network.connectedClients[info.sender].updates.Add(new clientUpdate(tick+delay, clientLookRot, clientFireRot, flipped));
                }
            }
        }

    }
}
