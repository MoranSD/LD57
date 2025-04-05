using System;
using UnityEngine;

namespace Game
{
    public class HUD : MonoBehaviour
    {
        public Action<HoleView> OnPointerEnterHole;
        public Action<HoleView> OnPointerExitHole;
        public Action<AbilitySlot> OnPointerEnterAbilitySlot;
        public Action<AbilitySlot> OnPointerExitAbilitySlot;

        public NewAbilitySelectionPanel AbilitySelectionPanel;

        private void Awake()
        {
            G.HUD = this;
        }

        public void DisableHud()
        {

        }
    }
}