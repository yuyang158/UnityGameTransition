using System;
using UnityEngine;

namespace GameTransition {
	[Serializable]
	public class IGTAction : ScriptableObject {
		public virtual void OnEnable() { }

		public virtual void OnDisable() { }

		public virtual bool Finished {
			get { return false; }
		}

		public virtual void Update() { }

		public virtual void OnMessage( string message ) { }
	}

	public interface IGameObjectProvide {
		GameObject GO { get; set; }

		Type ProvideType { get; }

		string ProvideTitle { get; }
	}

	public class GTActionListDrawerAttribute : PropertyAttribute {

	}
}
