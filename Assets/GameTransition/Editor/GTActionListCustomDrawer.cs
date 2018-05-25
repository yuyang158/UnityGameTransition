using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameTransition.Edit {
	[CustomEditor( typeof( GTSequenceState ), true )]
	public class GTActionListCustomDrawer : Editor {
		private GTSequenceState state;

		void OnEnable() {
			state = target as GTSequenceState;
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
		}
	}
}
