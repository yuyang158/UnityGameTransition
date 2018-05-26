using System;
using System.Reflection;
using UnityEngine;

namespace GameTransition {
	[Serializable, CreateAssetMenu( fileName = "InvokeAction", menuName = "GT/Invoke Action", order = 1 )]
	public class GTInvokeMethodAction : IGTAction, IGameObjectProvide {
		public override bool Finished {
			get { return true; }
		}

		public GameObject GO {
			get;
			set;
		}

		[SerializeField]
		GameObjectInvokeHolder holder = new GameObjectInvokeHolder();
		public GameObjectInvokeHolder Holder {
			get { return holder; }
		}

		public Type ProvideType {
			get {
				if( holder.SelectedDescriptor != null ) {
                    return holder.SelectedDescriptor.ComponentOwner;
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

