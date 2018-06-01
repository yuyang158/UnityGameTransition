using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GameTransition.Utility {
	public static class TypeHelper {
		private static bool ValidType( Type type ) {
			return type == typeof( int ) || type == typeof( double ) || type == typeof( string ) ||
				type == typeof( TextAsset ) || type == typeof( bool );
		}

		public static IEnumerable<MethodInfo> CollectValidMethod( Type type ) {
			List<MethodInfo> results = new List<MethodInfo>();
			var methods = type.GetMethods( BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance );
			foreach( var m in methods ) {
				var p = m.GetParameters();
				if( ( m.ReturnType == typeof( void ) || m.ReturnType == null ) && p.Length <= 1 && m.IsAbstract == false && m.IsPublic &&
					m.IsStatic == false && !m.Name.Contains( "_" ) ) {
					if( p.Length == 0 ) {
						results.Add( m );
					}
					else if( p.Length == 1 && ValidType( p[0].ParameterType ) ) {
						results.Add( m );
					}
				}
			}

			return results;
		}

		public static IEnumerable<PropertyInfo> CollectValidProperties( Type type ) {
			var properties = type.GetProperties( BindingFlags.Public | BindingFlags.Instance );
			var filtered = from p in properties
						   where p.CanWrite
						   && ValidType( p.PropertyType )
						   select p;

			return filtered;
		}

		static readonly object[] singleParamContainer = new object[1];
		public static void InvokeMethodOrProperty( InvokeDescriptor invokeDescriptor, UnityEngine.Object instance ) {
			if( !instance ) {
				return;
			}

			if( string.IsNullOrEmpty( invokeDescriptor.MethodName ) && string.IsNullOrEmpty( invokeDescriptor.PropertyName ) ) {
				return;
			}

			var type = instance.GetType();
			if( string.IsNullOrEmpty( invokeDescriptor.MethodName ) ) {
				var propertyInfo = type.GetProperty( invokeDescriptor.PropertyName );
				propertyInfo.SetValue( instance, invokeDescriptor.Param.Get(), null );
			}
			else {
				Type[] types;
				if( invokeDescriptor.Param.ParamType == InvokeParam.Type.None ) {
					types = new Type[0];
				}
				else {
					types = new[] { invokeDescriptor.Param.Get().GetType() };

				}

				var methodInfo = type.GetMethod( invokeDescriptor.MethodName, types );
				if( types.Length == 0 ) {
					methodInfo.Invoke( instance, null );
				}
				else {
					singleParamContainer[0] = invokeDescriptor.Param.Get();
					methodInfo.Invoke( instance, singleParamContainer );
				}
			}
		}

		public static List<InvokeDescriptor> CollectValidMeshodAndField( GameObject go ) {
			if( !go ) {
				return null;
			}

			List<InvokeDescriptor> descriptors = new List<InvokeDescriptor>();

			var methods = CollectValidMethod( typeof( GameObject ) );
			foreach( var method in methods ) {
				var param = method.GetParameters();
				descriptors.Add( param.Length > 0 ? new InvokeDescriptor( param[0].ParameterType ) {
					MethodName = method.Name
				} : new InvokeDescriptor() {
					MethodName = method.Name
				} );
			}

			var fields = CollectValidProperties( typeof( GameObject ) );
			foreach( var field in fields ) {
				descriptors.Add( new InvokeDescriptor( field.PropertyType ) {
					PropertyName = field.Name
				} );
			}

			var components = go.GetComponents<Component>();
			foreach( var component in components ) {
				methods = CollectValidMethod( component.GetType() );
				foreach( var method in methods ) {
					var p = method.GetParameters();
					if( p.Length == 1 ) {
						descriptors.Add( new InvokeDescriptor( p[0].ParameterType ) {
							MethodName = method.Name,
							ComponentTypeInput = component.GetType()
						} );
					}
					else {
						descriptors.Add( new InvokeDescriptor() {
							MethodName = method.Name,
							ComponentTypeInput = component.GetType()
						} );
					}
				}

				fields = CollectValidProperties( component.GetType() );
				foreach( var field in fields ) {
					descriptors.Add( new InvokeDescriptor( field.PropertyType ) {
						PropertyName = field.Name,
						ComponentTypeInput = component.GetType()
					} );
				}
			}
			return descriptors;
		}
	}
}
