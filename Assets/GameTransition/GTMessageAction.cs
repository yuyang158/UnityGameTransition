using System;
using UnityEngine;

namespace GameTransition {
	[Serializable, CreateAssetMenu( fileName = "Message Action", menuName = "GT/Message Action", order = 1 )]
	public class GTMessageAction : IGTAction {
		public string Message;
		private bool triggered;

		public override void OnMessage( string message ) {
			if( Message == message ) {
				triggered = true;
			}
			else {
				Debug.LogWarningFormat( "Mismatch message expect {0} but {1}", Message, message );
			}
		}

		public override void OnEnable() {
			triggered = false;
		}

		public override bool Finished {
			get {
				return triggered;
			}
		}

		public override string ToString() {
			return "Waiting For : " + Message;
		}
	}
}
