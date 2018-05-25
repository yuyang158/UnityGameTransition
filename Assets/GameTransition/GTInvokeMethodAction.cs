using System;
using System.Reflection;
using UnityEngine;

namespace GameTransition {
	[Serializable, CreateAssetMenu( fileName = "InvokeMethod", menuName = "GT/Invoke Method", order = 1 )]
	public class GTInvokeMethodAction : IGTAction, IGameObjectProvide {
		public override bool Finished {
			get { return true; }
		}

		public GameObject GO {
			get;
			set;
		}

		[SerializeField]
		private GameObjectInvokeHolder holder = new GameObjectInvokeHolder();
		public GameObjectInvokeHolder Holder {
			get { return holder; }
		}

		public Type ProvideType {
			get {
				if( holder.SelectedDescriptor != null ) {
					if( string.IsNullOrEmpty( holder.SelectedDescriptor.ComponentType ) ) {
						return typeof( GameObject );
					}
					Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
					foreach( var assembly in assemblies ) {
						var type = assembly.GetType( holder.SelectedDescriptor.ComponentType );
						if( type != null ) {
							return type;
						}
					}
				}

				return typeof( GameObject );
			}
		}

		public string ProvideTitle {
			get {
				return holder.SelectedDescriptor.ToString();
			}
		}

		public GTInvokeMethodAction() {

		}

		public override void OnEnable() {
			holder.InvokeGO = GO;
			if( Application.isPlaying ) {
				holder.Invoke();
			}
		}
	}
}

