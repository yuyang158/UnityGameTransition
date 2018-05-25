using UnityEngine.UI;

namespace GameTransition {
	public class ButtonTransition : GameTransitionBase {
		public Button Button;

		protected override void Start() {
			base.Start();
			Button.onClick.AddListener( () => {
				StartState();
			} );
		}
	}
}
