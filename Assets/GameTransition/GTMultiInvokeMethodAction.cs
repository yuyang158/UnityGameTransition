using System;
using UnityEngine;

namespace GameTransition {
	[Serializable, CreateAssetMenu( fileName = "InvokeMultiAction", menuName = "GT/Invoke Multi Action", order = 1 )]
	public class GTMultiInvokeMethodAction : IGTAction, IGameObjectProvide {
		public override bool Finished {
			get { return true; }
		}

		public GameObject GO { get; set; }
        public Component Component { get; set; }

		[SerializeField]
		GameObjectMultiInvokeHolder holder = new GameObjectMultiInvokeHolder();
		public GameObjectMultiInvokeHolder Holder {
			get { return holder; }
		}

		public Type ProvideType {
			get {
				return typeof( GameObject );
			}
		}

		public string ProvideTitle {
			get {
				string title = "";
				foreach( var descriptor in holder.Descriptors ) {
					title += descriptor + "\n";
				}

				return title;
			}
		}

		public override void OnEnable() {
			holder.InvokeGO = GO;
			if( Application.isPlaying ) {
				holder.Invoke();
			}
		}

		public override string ToString() {
			return string.IsNullOrEmpty(Name) ? ProvideTitle : Name;
		}
	}
}

