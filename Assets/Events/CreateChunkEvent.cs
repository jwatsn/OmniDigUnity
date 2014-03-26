using UnityEngine;

public class CreateChunkEvent : OmniEvent{

	public CreateChunkEvent(int tick) : base(tick) {

	}


	public override void handle(int tick) {
		base.handle (tick);

		if (handled) {

		}
	}

}
