using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;
using UnityEngine.Playables;
using GameTransition.Utility;

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

            public Type ComponentTypeInput {
                set { 
                    ComponentType = value.FullName;
                    AssemblyName = value.Assembly.FullName;
                }
            }

            Type componentOwner;
            public Type ComponentOwner {
                get {
                    if(componentOwner == null) {
                        if(string.IsNullOrEmpty(ComponentType)) {
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
                else if( paramType == typeof( double ) ) {
                    Param.ParamType = InvokeParam.Type.Double;
                }
                else {
                    Debug.LogError( "UNSUPPORT TYPE : " + paramType.Name );
                }
            }

            public override bool Equals( object obj ) {
                InvokeDescriptor other = obj as InvokeDescriptor;
                if(other == null) {
                    return false;
                }
                return (other.MethodName == MethodName && other.PropertyName == 
                        PropertyName && other.ComponentType == ComponentType);
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

        [SerializeField]
        public InvokeDescriptor SelectedDescriptor;

        public GameObject InvokeGO { set; get; }

        public Component InvokeComponent { get; set; }

        readonly object[] singleParamContainer = new object[1];
        public void Invoke() {
            if( !InvokeGO && !InvokeComponent ) {
                return;
            }

            var invokeDescriptor = SelectedDescriptor;
			if( InvokeGO ) {
				InvokeMethodOrProperty( invokeDescriptor, InvokeGO );
			}
			else {
				InvokeMethodOrProperty( invokeDescriptor, InvokeComponent );
			}
		}

        void InvokeMethodOrProperty( InvokeDescriptor invokeDescriptor, UnityEngine.Object instance ) {
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

        bool ValidType( Type type ) {
            return type == typeof( int ) || type == typeof( double ) || type == typeof( string ) ||
                type == typeof( TextAsset ) || type == typeof( bool );
        }

        IEnumerable<MethodInfo> CollectValidMethod( Type type ) {
            List<MethodInfo> results = new List<MethodInfo>();
            var methods = type.GetMethods( BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance );
            foreach( var m in methods ) {
                var p = m.GetParameters();
                if( (m.ReturnType == typeof( void ) || m.ReturnType == null) && p.Length <= 1 && m.IsAbstract == false && m.IsPublic &&
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

        IEnumerable<PropertyInfo> CollectValidProperties( Type type ) {
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

            var fields = CollectValidProperties( InvokeGO.GetType() );
            foreach( var field in fields ) {
                descriptors.Add( new InvokeDescriptor( field.PropertyType ) {
                    PropertyName = field.Name
                } );
            }

            var components = InvokeGO.GetComponents<Component>();
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
