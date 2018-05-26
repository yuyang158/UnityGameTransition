using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace GameTransition.Edit {
	[CustomEditor( typeof( GTInvokeMethodAction ) )]
	public class GameObjectInvokeHolderEditor : Editor {
		GTInvokeMethodAction action;
		List<GameObjectInvokeHolder.InvokeDescriptor> descriptors;

		void OnEnable() {
			action = target as GTInvokeMethodAction;
		}

		int selectedDescriptorIndex = 0;

		public override void OnInspectorGUI() {
			EditorGUILayout.PrefixLabel( "Invoke Context" );

			EditorGUILayout.BeginVertical( "HelpBox" );
			{
				EditorGUILayout.BeginHorizontal();
                GUILayout.Space(120);

				var holder = action.Holder;
				FunctionInvokeEdit( holder );

				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();
		}

		void FunctionInvokeEdit( GameObjectInvokeHolder holder ) {
			descriptors = holder.CollectValidMeshodAndField();
			if( descriptors == null ) {
				string[] noFunction = new string[1];
				if( holder.SelectedDescriptor != null ) {
					noFunction[0] = holder.SelectedDescriptor.ToString();
				}
				else {
					noFunction[0] = "No Function";
				}
				EditorGUILayout.Popup( 0, noFunction );
			}
			else {
				List<string> functions = new List<string> {
						"No Function"
					};
				foreach( var descriptor in descriptors ) {
					functions.Add( descriptor.ToString() );
				}

				if( holder.SelectedDescriptor != null ) {
					for( int i = 0; i < functions.Count; i++ ) {
						if( functions[i] == holder.SelectedDescriptor.ToString() ) {
							selectedDescriptorIndex = i;
							break;
						}
					}
				}

				var selected = EditorGUILayout.Popup( selectedDescriptorIndex, functions.ToArray() );
				if( selected != selectedDescriptorIndex ) {
					selectedDescriptorIndex = selected;
					if( selectedDescriptorIndex == 0 ) {
						holder.SelectedDescriptor = null;
					}
					else {
						holder.SelectedDescriptor = descriptors[selectedDescriptorIndex - 1];
					}
				}
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
            holder.InvokeGO = (GameObject)EditorGUILayout.ObjectField( holder.InvokeGO, typeof( GameObject ), true, GUILayout.Width(120) );
			var selectedDescriptor = holder.SelectedDescriptor;
			if( selectedDescriptor == null ) {
				GUILayout.FlexibleSpace();
			}
			else {
				switch( selectedDescriptor.Param.ParamType ) {
				case GameObjectInvokeHolder.InvokeParam.Type.Integer:
					selectedDescriptor.Param.Integer = EditorGUILayout.IntField( selectedDescriptor.Param.Integer );
					break;
				case GameObjectInvokeHolder.InvokeParam.Type.Float:
					selectedDescriptor.Param.Float = EditorGUILayout.FloatField( selectedDescriptor.Param.Float );
					break;
				case GameObjectInvokeHolder.InvokeParam.Type.Boolean:
					selectedDescriptor.Param.Boolean = EditorGUILayout.Toggle( selectedDescriptor.Param.Boolean );
					break;
				case GameObjectInvokeHolder.InvokeParam.Type.String:
					selectedDescriptor.Param.String = EditorGUILayout.TextField( selectedDescriptor.Param.String );
					break;
				case GameObjectInvokeHolder.InvokeParam.Type.Object:
					selectedDescriptor.Param.Object = EditorGUILayout.ObjectField( selectedDescriptor.Param.Object, typeof( Object ), false );
					break;
				case GameObjectInvokeHolder.InvokeParam.Type.None:
					GUILayout.FlexibleSpace();
					break;
				}
			}
		}
	}
}
