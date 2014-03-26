using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OmniEvents : MonoBehaviour {

	static OmniEvents instance;
    public static List<OmniEvent> toValidate;
	List<OmniEvent> pendingEvents;
	List<OmniEvent> pendingDelete;
	// Use this for initialization
	void OnEnable() {
		pendingEvents = new List<OmniEvent> ();
		pendingDelete = new List<OmniEvent> ();
        toValidate = new List<OmniEvent>();
		instance = this;
	}

	public static void AddEvent(OmniEvent e) {
		instance.pendingEvents.Add(e);
        e.init();
	}

	// Update is called once per frame
	public void OmniUpdate (float delta) {
		int len = pendingEvents.Count;
		for (int i=0; i<len; i++) {
            pendingEvents[i].handle(OmniWorld.tick);
            if (pendingEvents[i].handled)
            {
                if(pendingEvents[i].onHandle != null)
                    pendingEvents[i].onHandle(pendingEvents[i]);

                pendingDelete.Add(pendingEvents[i]);
            }
            if(pendingEvents[i].forceDelete)
                if(!pendingDelete.Contains(pendingEvents[i]))
                    pendingDelete.Add(pendingEvents[i]);
		}
        CheckRemove();
	}


	void CheckRemove() {
		int len = pendingDelete.Count;
		for (int i=0; i<len; i++) {
			pendingEvents.Remove(pendingDelete[i]);
		}
		pendingDelete.Clear ();
	}
}
