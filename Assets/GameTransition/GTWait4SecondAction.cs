using System;
using UnityEngine;

namespace GameTransition {
	[Serializable, CreateAssetMenu( fileName = "Wait Time Action", menuName = "GT/Wait Time Action", order = 1 )]
	public class GTWait4SecondAction : IGTAction {
		public float duration;
		private float time;

		public override void Update() {
			time += Time.deltaTime;
		}

		public override void OnEnable() {
			time = 0;
		}

		public override bool Finished {
			get {
				return time >= duration;
			}
		}

		public override string ToString() {
			return string.Format( "Wait for {0} s", duration );
		}
	}
}
