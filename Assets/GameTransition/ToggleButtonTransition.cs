using UnityEngine;
using UnityEngine.UI;

namespace GameTransition {
    public class ToggleButtonTransition : MonoBehaviour {
        public GameTransitionBase OnChecked;
        public GameTransitionBase OnUnchecked;

        public Toggle ToggleButton;

        void Start() {
            ToggleButton.onValueChanged.AddListener( ( isChecked ) => {
                if(isChecked) {
                    OnChecked.StartState();
                }
                else {
                    OnUnchecked.StartState();
                }
            } );    
        }
    }
}