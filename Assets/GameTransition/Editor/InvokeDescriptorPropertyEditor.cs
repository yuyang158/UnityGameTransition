using GameTransition.Utility;
using Malee.Editor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GameTransition.Edit {
	[CustomPropertyDrawer( typeof( InvokeDescriptor ) )]
	public class InvokeDescriptorDrawer : PropertyDrawer {
		private class Context {
			public InvokeDescriptor Current;
			public InvokeDescriptor Selected;
		}

		List<InvokeDescriptor> descriptors;
		InvokeDescriptor descriptor;
		void OnDescriptorSelected( object selected ) {
			var context = selected as Context;

			var desc = context.Selected;
			var current = context.Current;
			if( desc == null ) {
				current.AssemblyName = "";
				current.ComponentType = "";
				current.MethodName = "";
				current.PropertyName = "";
			}
			else {
				current.AssemblyName = desc.AssemblyName;
				current.ComponentType = desc.ComponentType;
				current.MethodName = desc.MethodName;
				current.PropertyName = desc.PropertyName;
				current.Param = desc.Param;

				current.ClearOwnerType();
			}
		}

		private GameObject templateGO;
		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
			descriptors = TypeHelper.CollectValidMeshodAndField( templateGO );
			descriptor = GetTargetObjectWithProperty( property ) as InvokeDescriptor;
			Rect functionSelect = position;
			functionSelect.x += 120;
			functionSelect.width -= 120;
			functionSelect.height = EditorGUIUtility.singleLineHeight;

			if( descriptors == null ) {
				GUI.enabled = false;
				EditorGUI.DropdownButton( functionSelect, new GUIContent( descriptor.ToString() ), FocusType.Passive );
				GUI.enabled = true;
			}
			else {
				if( EditorGUI.DropdownButton( functionSelect, new GUIContent( descriptor.ToString() ), FocusType.Passive ) ) {
					GenericMenu menu = new GenericMenu();
					menu.AddItem( new GUIContent( "No Function" ), descriptor == null, OnDescriptorSelected, new Context() {
						Current = descriptor,
						Selected = null
					} );
					menu.AddSeparator( "" );

					foreach( var desc in descriptors ) {
						menu.AddItem( new GUIContent( desc.MenuPath ), desc == descriptor, OnDescriptorSelected, new Context() {
							Current = descriptor,
							Selected = desc
						} );
					}

					functionSelect.y -= EditorGUIUtility.singleLineHeight;
					menu.DropDown( functionSelect );
				}
			}

			// next line
			position.y += EditorGUIUtility.singleLineHeight;
			templateGO = (GameObject)EditorGUI.ObjectField( new Rect( position.x, position.y, 120, EditorGUIUtility.singleLineHeight ), templateGO, typeof( GameObject ), true );

			var valueRect = new Rect( position );
			valueRect.x += 120;
			valueRect.width -= 120;
			valueRect.height = EditorGUIUtility.singleLineHeight;
			if( descriptor.IsActive ) {
				switch( descriptor.Param.ParamType ) {
				case InvokeParam.Type.Integer:
					descriptor.Param.Integer = EditorGUI.IntField( valueRect, descriptor.Param.Integer );
					break;
				case InvokeParam.Type.Float:
				case InvokeParam.Type.Double:
					descriptor.Param.Float = EditorGUI.DoubleField( valueRect, descriptor.Param.Float );
					break;
				case InvokeParam.Type.Boolean:
					descriptor.Param.Boolean = EditorGUI.Toggle( valueRect, descriptor.Param.Boolean );
					break;
				case InvokeParam.Type.String:
					descriptor.Param.String = EditorGUI.TextField( valueRect, descriptor.Param.String );
					break;
				case InvokeParam.Type.Object:
					descriptor.Param.Object = EditorGUI.ObjectField( valueRect, descriptor.Param.Object, typeof( Object ), false );
					break;
				case InvokeParam.Type.None:
					break;
				}
			}
		}

		public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) {
			return EditorGUIUtility.singleLineHeight * 2 + 5;
		}

		public static object GetTargetObjectWithProperty( SerializedProperty prop ) {
			var path = prop.propertyPath.Replace( ".Array.data[", "[" );
			object obj = prop.serializedObject.targetObject;
			var elements = path.Split( '.' );
			foreach( var element in elements ) {
				if( element.Contains( "[" ) ) {
					var elementName = element.Substring( 0, element.IndexOf( "[" ) );
					var index = System.Convert.ToInt32( element.Substring( element.IndexOf( "[" ) ).Replace( "[", "" ).Replace( "]", "" ) );
					obj = GetValue_Imp( obj, elementName, index );
				}
				else {
					obj = GetValue_Imp( obj, element );
				}
			}
			return obj;
		}

		private static object GetValue_Imp( object source, string name ) {
			if( source == null )
				return null;
			var type = source.GetType();

			while( type != null ) {
				var f = type.GetField( name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance );
				if( f != null )
					return f.GetValue( source );

				var p = type.GetProperty( name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase );
				if( p != null )
					return p.GetValue( source, null );

				type = type.BaseType;
			}
			return null;
		}

		private static object GetValue_Imp( object source, string name, int index ) {
			var enumerable = GetValue_Imp( source, name ) as System.Collections.IEnumerable;
			if( enumerable == null ) return null;
			var enm = enumerable.GetEnumerator();
			//while (index-- >= 0)
			//    enm.MoveNext();
			//return enm.Current;

			for( int i = 0; i <= index; i++ ) {
				if( !enm.MoveNext() ) return null;
			}
			return enm.Current;
		}
	}
}
