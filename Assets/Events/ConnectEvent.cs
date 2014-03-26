using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

    public class ConnectEvent : OmniEvent
    {
        string ip;
        int port;
        int lastTick;
        public ConnectEvent(int tick, string address) :
            base(tick)
        {
            Network.Disconnect();
            OmniWorld.Clear();
            string[] a = address.Split(':');
            if (a.Length > 1)
            {
                ip = a[0];
                port = int.Parse(a[1]);
            }

        }


        public override void handle(int tick)
        {
            base.handle(tick);
            if (handled)
            {
                NetworkConnectionError e = NetworkConnectionError.EmptyConnectTarget;
                e = Network.Connect(ip, port);
            }

        }
    }

