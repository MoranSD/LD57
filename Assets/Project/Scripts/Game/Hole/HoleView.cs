using System;
using UnityEngine;

namespace Game
{
    public class HoleView : MonoBehaviour
    {
        public event Action<HoleState> OnPressed;

        public HoleState State;

        public void SetState(HoleState state)
        {
            State = state;
            State.View = this;
        }

        private void OnMouseDown()
        {
            OnPressed?.Invoke(State);
        }

        private void OnMouseEnter()
        {
            G.HUD.OnPointerEnterHole(this);
        }

        private void OnMouseExit()
        {
            G.HUD.OnPointerExitHole(this);
        }
    }
}