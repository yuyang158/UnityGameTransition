using System;
using UnityEditor;
using UnityEngine;

namespace GameTransition.Edit {
	[CustomEditor( typeof( GameTransitionBase ) )]
	public class GameTransitionEditor : Editor {
		GameTransitionBase transition;

		void OnEnable() {
			transition = target as GameTransitionBase;
		}

		static bool foldout;
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();

			if( transition.State == null ) {
				transition.bindings = new GameTransitionBase.Binding[0];
				return;
			}

			foldout = EditorGUILayout.Foldout( foldout, "Bindings" );
			if( foldout ) {
				int totalGOProviderCount = 0;
				foreach( var action in transition.State.Actions ) {
					if( action is IGameObjectProvide ) {
						totalGOProviderCount += 1;
					}
				}

				int bindingCount = transition.bindings == null ? 0 : transition.bindings.Length;
				if( totalGOProviderCount != bindingCount ) {
					var oriBindins = transition.bindings;
					transition.bindings = new GameTransitionBase.Binding[totalGOProviderCount];

					if( oriBindins != null ) {
						Array.Copy( oriBindins, transition.bindings, Math.Min( oriBindins.Length, totalGOProviderCount ) );
					}

					for( int i = 0; i < transition.bindings.Length; i++ ) {
						if( transition.bindings[i] == null ) {
							transition.bindings[i] = new GameTransitionBase.Binding();
						}
					}
				}

				EditorGUI.indentLevel += 1;
				int bindingIndex = 0;
				for( int actionIndex = 0; actionIndex < transition.State.Actions.Length; actionIndex++ ) {
					var action = transition.State.Actions[actionIndex];
					if( action is IGameObjectProvide ) {
						var provide = action as IGameObjectProvide;
						var binding = transition.bindings[bindingIndex];
						binding.ActionIndex = actionIndex;
                        binding.GO = (GameObject)EditorGUILayout.ObjectField( provide.ProvideTitle, binding.GO, provide.ProvideType, true );

						bindingIndex++;
					}
				}
			}
		}
	}

	[CustomEditor( typeof( ButtonTransition ) )]
	public class ButtonTransitionEditor : GameTransitionEditor {
	}

	[CustomEditor( typeof( ManualTransition ) )]
	public class ManualTransitionEditor : GameTransitionEditor {
	}
}
