using System;
using UnityEngine;

namespace GameTransition {
	public class GameTransitionBase : MonoBehaviour {
		[Serializable]
		public class Binding {
			public GameObject GO;
			public int ActionIndex;
		}

		public GTSequenceState State;
		protected GTSequenceState runtimeState;

		[SerializeField, HideInInspector]
		public Binding[] bindings;

		protected virtual void Start() {
			runtimeState = Instantiate( State );
			runtimeState.Initialize();

			foreach( var binding in bindings ) {
				runtimeState.Binding( binding.GO, binding.ActionIndex );
			}
		}

		public void StartState(bool forceReplay = false) {
            if( !forceReplay && runtimeState.IsPlaying ) {
                return;
            }
			runtimeState.Reset();
		}

		public void StopState() {
			runtimeState.Stop();
		}

		private void Update() {
			runtimeState.Update();
		}

		public void OnMessage( string message ) {
            runtimeState.OnMessage( message );
		}
	}
}
