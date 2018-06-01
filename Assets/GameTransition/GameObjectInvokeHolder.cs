using System;
using UnityEngine;
using GameTransition.Utility;

namespace GameTransition {
	[Serializable]
	public struct InvokeParam {
		public enum Type {
			Integer,
			Float,
			Double,
			Boolean,
			String,
			Object,
			None
		}
		public Type ParamType;

		public int Integer;
		public double Float;
		public bool Boolean;
		public string String;
		public UnityEngine.Object Object;

		public object Get() {
			switch( ParamType ) {
			case Type.Integer:
				return Integer;
			case Type.Float:
				return (float)Float;
			case Type.Double:
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
		public string AssemblyName;

		public InvokeParam Param;

		public Type ComponentTypeInput {
			set {
				ComponentType = value.FullName;
				AssemblyName = value.Assembly.FullName;
			}
		}

		Type componentOwner;
		public Type ComponentOwner {
			get {
				if( componentOwner == null ) {
					if( string.IsNullOrEmpty( ComponentType ) ) {
						componentOwner = typeof( GameObject );
					}
					else {
						var assembly = AssemblyHelper.GetAssembly( AssemblyName );
						if( assembly != null ) {
							componentOwner = assembly.GetType( ComponentType );
						}
					}
				}
				return componentOwner;
			}
		}

		public void ClearOwnerType() {
			componentOwner = null;
		}

		public InvokeDescriptor() {
			Param.ParamType = InvokeParam.Type.None;
		}

		public bool IsActive {
			get {
				return !( string.IsNullOrEmpty( MethodName ) && string.IsNullOrEmpty( PropertyName ) );
			}
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
			else if( paramType == typeof( double ) ) {
				Param.ParamType = InvokeParam.Type.Double;
			}
			else {
				Debug.LogError( "UNSUPPORT TYPE : " + paramType.Name );
			}
		}

		public override bool Equals( object obj ) {
			InvokeDescriptor other = obj as InvokeDescriptor;
			if( other == null ) {
				return false;
			}
			return ( other.MethodName == MethodName && other.PropertyName ==
					PropertyName && other.ComponentType == ComponentType );
		}

		public override int GetHashCode() {
			return 1;
		}

		public string MenuPath {
			get {
				var name = ComponentOwner.Name;
				string path = name + "/";

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

		public override string ToString() {
			if( string.IsNullOrEmpty( MethodName ) && string.IsNullOrEmpty( PropertyName ) ) {
				return "No Function";
			}

			var name = ComponentOwner.Name;
			string path = name + ".";

			if( string.IsNullOrEmpty( MethodName ) ) {
				path += PropertyName + " = " + Param.Get();
			}
			else {
				if( Param.ParamType == InvokeParam.Type.None ) {
					path += MethodName + "()";
				}
				else {
					path += string.Format( "{0}({1})", MethodName, Param.Get() );
				}
			}
			return path;
		}
	}

	[Serializable]
	public class GameObjectInvokeHolder {
		[SerializeField]
		public InvokeDescriptor SelectedDescriptor;

		public GameObject InvokeGO { set; get; }
		public Component InvokeComponent { get; set; }

		public void Invoke() {
			if( !InvokeGO && !InvokeComponent ) {
				return;
			}

			if( InvokeGO ) {
				TypeHelper.InvokeMethodOrProperty( SelectedDescriptor, InvokeGO );
			}
			else {
				TypeHelper.InvokeMethodOrProperty( SelectedDescriptor, InvokeComponent );
			}
		}
	}
}
