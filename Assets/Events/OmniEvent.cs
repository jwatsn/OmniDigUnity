using UnityEngine;

public class OmniEvent {

	public int tick;
	public bool handled;
    public bool forceDelete; // deletes an event without handle being true
    public delegate void OnHandled(OmniEvent e);
    public OnHandled onHandle;

	public OmniEvent(int tick) {
				this.tick = tick;
	}

    public virtual void init() // called when event is added to the event buffer
    {
    }

	virtual public void handle(int tick) {
		if (this.tick <= tick)
				handled = true;
	}

    virtual public void validate(OmniEvent e)
    {
    }
}
