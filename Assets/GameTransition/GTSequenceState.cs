using Malee;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameTransition {

	[Serializable]
	public class GTActionList : ReorderableArray<IGTAction> { }

	[Serializable, CreateAssetMenu( fileName = "Sequence State", menuName = "GT/Sequence State", order = 1 )]
	public class GTSequenceState : ScriptableObject {
		[SerializeField, Reorderable( singleLine = false )]
		private GTActionList actions = new GTActionList();
		public GTActionList Actions {
			get { return actions; }
		}

		private List<IGTAction> runtimeActions = new List<IGTAction>();
		public void Initialize() {
			foreach( var action in actions ) {
				runtimeActions.Add( Instantiate( action ) );
			}
		}

		private IGTAction currentAction;

		private int currentActionIndex;
		public int CurrentActionIndex {
			private set {
				currentActionIndex = value;
                if( currentActionIndex >= runtimeActions.Count || currentActionIndex < 0 ) {
					currentAction = null;
					return;
				}
				if( currentAction != null ) {
					currentAction.OnDisable();
				}

				currentAction = runtimeActions[currentActionIndex];
				currentAction.OnEnable();
			}

			get {
				return currentActionIndex;
			}
		}

		public void Reset() {
			CurrentActionIndex = 0;
		}

		public void Binding( GameObject go, int index ) {
			var runtime = runtimeActions[index] as IGameObjectProvide;
			runtime.GO = go;
		}

		public void Update() {
			while( currentAction != null && currentAction.Finished ) {
				CurrentActionIndex += 1;
				if( currentAction == null ) {
					break;
				}
			}

			if( currentAction != null ) {
				currentAction.Update();
			}
		}

		public void Stop() {
			CurrentActionIndex = 9999;
			currentAction = null;
		}


		public void OnMessage( string message ) {
			if( currentAction != null ) {
				currentAction.OnMessage( message );
			}
		}

        public bool IsPlaying {
            get {
                return currentAction != null;
            }
        }
	}
}
