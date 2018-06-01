using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using GameTransition.Utility;

namespace GameTransition.Edit {
	[CustomEditor( typeof( GTInvokeMethodAction ) )]
	public class GameObjectInvokeHolderEditor : Editor {
		GTInvokeMethodAction action;
		List<InvokeDescriptor> descriptors;

		void OnEnable() {
			action = target as GTInvokeMethodAction;
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			EditorGUILayout.PrefixLabel( "Invoke Context" );

			EditorGUILayout.BeginVertical( "HelpBox" );
			FunctionInvokeEdit( action.Holder );
			EditorGUILayout.EndVertical();
		}

		void OnDescriptorSelected( object selected ) {
			var descriptor = selected as InvokeDescriptor;
			action.Holder.SelectedDescriptor = descriptor;

			EditorUtility.SetDirty( action );
		}

		private GameObject templateGO;

		void FunctionInvokeEdit( GameObjectInvokeHolder holder ) {
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space( 120 );

			descriptors = TypeHelper.CollectValidMeshodAndField( templateGO );
			if( descriptors == null ) {
				GUI.enabled = false;
				string title;
				if( holder.SelectedDescriptor != null ) {
					title = holder.SelectedDescriptor.ToString();
				}
				else {
					title = "No Function";
				}
				EditorGUILayout.DropdownButton( new GUIContent( title ), FocusType.Passive );
				GUI.enabled = true;
			}
			else {
				string title;
				if( holder.SelectedDescriptor != null ) {
					title = holder.SelectedDescriptor.ToString();
				}
				else {
					title = "No Function";
				}

				if( EditorGUILayout.DropdownButton( new GUIContent( title ), FocusType.Passive ) ) {
					GenericMenu menu = new GenericMenu();
					menu.AddItem( new GUIContent( "No Function" ), holder.SelectedDescriptor == null, OnDescriptorSelected, null );
					menu.AddSeparator( "" );

					foreach( var descriptor in descriptors ) {
						menu.AddItem( new GUIContent( descriptor.MenuPath ), holder.SelectedDescriptor == descriptor, OnDescriptorSelected, descriptor );
					}

					var rect = GUILayoutUtility.GetLastRect();
					menu.DropDown( rect );
				}
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			templateGO = (GameObject)EditorGUILayout.ObjectField( templateGO, typeof( GameObject ), true, GUILayout.Width( 120 ) );
			var selectedDescriptor = holder.SelectedDescriptor;
			if( selectedDescriptor == null ) {
				GUILayout.FlexibleSpace();
			}
			else {
				switch( selectedDescriptor.Param.ParamType ) {
				case InvokeParam.Type.Integer:
					selectedDescriptor.Param.Integer = EditorGUILayout.IntField( selectedDescriptor.Param.Integer );
					break;
				case InvokeParam.Type.Float:
				case InvokeParam.Type.Double:
					selectedDescriptor.Param.Float = EditorGUILayout.DoubleField( selectedDescriptor.Param.Float );
					break;
				case InvokeParam.Type.Boolean:
					selectedDescriptor.Param.Boolean = EditorGUILayout.Toggle( selectedDescriptor.Param.Boolean );
					break;
				case InvokeParam.Type.String:
					selectedDescriptor.Param.String = EditorGUILayout.TextField( selectedDescriptor.Param.String );
					break;
				case InvokeParam.Type.Object:
					selectedDescriptor.Param.Object = EditorGUILayout.ObjectField( selectedDescriptor.Param.Object, typeof( Object ), false );
					break;
				case InvokeParam.Type.None:
					GUILayout.FlexibleSpace();
					break;
				}
			}
			EditorGUILayout.EndHorizontal();
		}
	}
}
