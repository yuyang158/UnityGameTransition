using System;
using UnityEngine;

namespace GameTransition {
	public class GameTransitionBase : MonoBehaviour {
		[Serializable]
		public class Binding {
			public UnityEngine.Object GO;
			public int ActionIndex;
		}

		public GTSequenceState State;
		protected GTSequenceState runtimeState;

		public GameTransitionBase Next;

		[SerializeField, HideInInspector]
		public Binding[] bindings;

		protected virtual void Start() {
			runtimeState = Instantiate( State );
			runtimeState.Initialize();

			foreach( var binding in bindings ) {
				runtimeState.Binding( binding.GO, binding.ActionIndex );
			}
			enabled = false;
		}

		public void StartState( bool forceReplay = false ) {
			if( !forceReplay && runtimeState.IsPlaying ) {
				return;
			}
			runtimeState.Reset();
			enabled = true;
		}

		public void StopState() {
			runtimeState.Stop();
			enabled = false;
		}

		void Update() {
			runtimeState.Update();
			if( runtimeState.IsPlaying == false ) {
				if( Next )
					Next.StartState();

				enabled = false;
			}
		}

		public void OnMessage( string message ) {
			runtimeState.OnMessage( message );
		}
	}
}
