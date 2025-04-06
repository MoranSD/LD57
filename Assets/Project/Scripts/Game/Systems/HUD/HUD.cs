using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class HUD : MonoBehaviour
    {
        public Action<HoleView> OnPointerEnterHole;
        public Action<HoleView> OnPointerExitHole;
        public Action<EntityView> OnPointerEnterEntity;
        public Action<EntityView> OnPointerExitEntity;
        public Action<AbilitySlot> OnPointerEnterAbilitySlot;
        public Action<AbilitySlot> OnPointerExitAbilitySlot;

        [Header("Hole selection")]
        public NewAbilitySelectionPanel AbilitySelectionPanel;
        [Header("Fight")]
        public FightAbilitiesPanel AbilitiesPanel;
        public Button EndTurnButton;

        private void Awake()
        {
            G.HUD = this;
        }

        public void DisableHud()
        {
            DisableFightHud();
        }

        public void EnableFightHud()
        {
            EndTurnButton.gameObject.SetActive(true);
            AbilitiesPanel.Show();
        }

        public void DisableFightHud()
        {
            EndTurnButton.gameObject.SetActive(false);
            AbilitiesPanel.Hide();
        }
    }
}