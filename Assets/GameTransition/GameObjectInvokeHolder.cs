using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;
using UnityEngine.Playables;

namespace GameTransition {
	public class GameObjectInvokeDrawerAttribute : PropertyAttribute {

	}

	[Serializable]
	public class GameObjectInvokeHolder {
		[Serializable]
		public struct InvokeParam {
			public enum Type {
				Integer,
				Float,
				Boolean,
				String,
				Object,
				None
			}
			public Type ParamType;

			public int Integer;
			public float Float;
			public bool Boolean;
			public string String;
			public UnityEngine.Object Object;

			public object Get() {
				switch( ParamType ) {
				case Type.Integer:
					return Integer;
				case Type.Float:
					return Float;
				case Type.Boolean:
					return Boolean;
				case Type.String:
					return String;
				case Type.Object:
					return Object;
				case Type.None:
					throw new Exception( "SHOULD NOT REACH THIS" );
				}
				throw new Exception( "SHOULD NOT REACH THIS" );
			}
		}

		[Serializable]
		public class InvokeDescriptor {
			public string MethodName;
			public string PropertyName;
			public string ComponentType;

			public InvokeParam Param;

			public InvokeDescriptor() {
				Param.ParamType = InvokeParam.Type.None;
			}

			public InvokeDescriptor( Type paramType ) {
				if( paramType == typeof( bool ) ) {
					Param.ParamType = InvokeParam.Type.Boolean;
				}
				else if( paramType == typeof( int ) ) {
					Param.ParamType = InvokeParam.Type.Integer;
				}
				else if( paramType == typeof( float ) ) {
					Param.ParamType = InvokeParam.Type.Float;
				}
				else if( paramType == typeof( string ) ) {
					Param.ParamType = InvokeParam.Type.String;
				}
				else if( paramType == typeof( UnityEngine.Object ) ) {
					Param.ParamType = InvokeParam.Type.Object;
				}
				else {
					Debug.LogError( "UNSUPPORT TYPE : " + paramType.Name );
				}
			}

			public override string ToString() {
				string path;
				if( string.IsNullOrEmpty( ComponentType ) ) {
					path = "GameObject.";
				}
				else {
					var last = ComponentType.LastIndexOf( '.' );
					if( last == -1 ) {
						last = 0;
					}
					else {
						last += 1;
					}
					path = ComponentType.Substring( last ) + ".";
				}

				if( string.IsNullOrEmpty( MethodName ) ) {
					path += PropertyName;
				}
				else {
					if( Param.ParamType == InvokeParam.Type.None ) {
						path += MethodName + "()";
					}
					else {
						path += string.Format( "{0}({1})", MethodName, Param.ParamType );
					}
				}
				return path;
			}
		}

		[SerializeField]
		public InvokeDescriptor SelectedDescriptor;

		public GameObject InvokeGO {
			set;
			get;
		}

		public GameObjectInvokeHolder() {

		}

		private readonly object[] singleParamContainer = new object[1];
		public void Invoke() {
			if( !InvokeGO ) {
				return;
			}

			var invokeDescriptor = SelectedDescriptor;
			if( string.IsNullOrEmpty(invokeDescriptor.ComponentType) ) {
				var instance = InvokeGO;
				InvokeMethodOrProperty( invokeDescriptor, instance );
			}
			else {
				var pd = InvokeGO.GetComponent<PlayableDirector>();
				pd.Play();
				var instance = InvokeGO.GetComponent( "PlayableDirector" );
				if( instance ) {
					InvokeMethodOrProperty( invokeDescriptor, instance );
				}
			}
		}

		private void InvokeMethodOrProperty( InvokeDescriptor invokeDescriptor, object instance ) {
			if( string.IsNullOrEmpty( invokeDescriptor.MethodName ) && string.IsNullOrEmpty( invokeDescriptor.PropertyName ) ) {
				return;
			}

			var type = instance.GetType();
			if( string.IsNullOrEmpty( invokeDescriptor.MethodName ) ) {
				var propertyInfo = type.GetProperty( invokeDescriptor.PropertyName );
				propertyInfo.SetValue( instance, invokeDescriptor.Param.Get(), null );
			}
			else {
				var methodInfo = type.GetMethod( invokeDescriptor.MethodName );
				if( methodInfo.GetParameters().Length == 0 ) {
					methodInfo.Invoke( instance, null );
				}
				else {
					singleParamContainer[0] = invokeDescriptor.Param.Get();
					methodInfo.Invoke( instance, singleParamContainer );
				}
			}
		}

		private bool ValidType( Type type ) {
			return type == typeof( int ) || type == typeof( double ) || type == typeof( string ) ||
				type == typeof( TextAsset ) || type == typeof( bool );
		}

		private IEnumerable<MethodInfo> CollectValidMethod( Type type ) {
			List<MethodInfo> results = new List<MethodInfo>();
			var methods = type.GetMethods( BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance );
			foreach( var m in methods ) {
				var p = m.GetParameters();
				if( m.ReturnType == typeof( void ) && p.Length <= 1 && m.IsAbstract == false && m.IsPublic &&
					m.IsStatic == false && !m.Name.Contains( "_" ) ) {
					if( p.Length == 1 && ValidType( p[0].ParameterType ) ) {
						results.Add( m );
					}
				}
			}

			return results;
		}

		private IEnumerable<PropertyInfo> CollectValidFields( Type type ) {
			var properties = type.GetProperties( BindingFlags.Public | BindingFlags.Instance );
			var filtered = from p in properties
						   where p.CanWrite
						   && ValidType( p.PropertyType )
						   select p;

			return filtered;
		}

		public List<InvokeDescriptor> CollectValidMeshodAndField() {
			if( !InvokeGO ) {
				return null;
			}

			List<InvokeDescriptor> descriptors = new List<InvokeDescriptor>();

			var methods = CollectValidMethod( InvokeGO.GetType() );
			foreach( var method in methods ) {
				var param = method.GetParameters();
				descriptors.Add( param.Length > 0 ? new InvokeDescriptor( param[0].ParameterType ) {
					MethodName = method.Name
				} : new InvokeDescriptor() {
					MethodName = method.Name
				} );
			}

			var fields = CollectValidFields( InvokeGO.GetType() );
			foreach( var field in fields ) {
				descriptors.Add( new InvokeDescriptor( field.PropertyType ) {
					PropertyName = field.Name
				} );
			}

			var components = InvokeGO.GetComponents<Component>();
			foreach( var component in components ) {
				methods = CollectValidMethod( component.GetType() );
				foreach( var method in methods ) {
					descriptors.Add( new InvokeDescriptor() {
						MethodName = method.Name,
						ComponentType = component.GetType().FullName
					} );
				}

				fields = CollectValidFields( component.GetType() );
				foreach( var field in fields ) {
					descriptors.Add( new InvokeDescriptor( field.PropertyType ) {
						PropertyName = field.Name,
						ComponentType = component.GetType().FullName
					} );
				}
			}
			return descriptors;
		}
	}
}
