using UnityEngine.UI;

namespace GameTransition {
	public class ButtonTransition : GameTransitionBase {
		public Button Button;

		protected override void Awake() {
			base.Awake();
			Button.onClick.AddListener( () => {
				StartState();
			} );
		}
	}
}
