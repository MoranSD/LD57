using Common;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class ToolTip : MonoBehaviour
    {
        public bool ShowInfo { get; private set; } = true;

        public float leftXPivot = -0.1f;
        public float rightXPivot = 1.1f;
        public float screenBorderOffset = 20f;

        [SerializeField] private RectTransform infoRect;
        [SerializeField] private TextMeshProUGUI infoText;

        private void Update()
        {
            if (!ShowInfo) return;

            var mousePosition = G.UI.GetMousePosition();
            LayoutRebuilder.ForceRebuildLayoutImmediate(infoRect);
            Vector2 menuSize = infoRect.rect.size;
            if (mousePosition.x + menuSize.x > Screen.width - screenBorderOffset)
            {
                var pivot = infoRect.pivot;
                pivot.x = rightXPivot;
                infoRect.pivot = pivot;
            }
            else
            {
                var pivot = infoRect.pivot;
                pivot.x = leftXPivot;
                infoRect.pivot = pivot;
            }

            infoRect.position = mousePosition;
        }

        public void EnableInfoShow()
        {
            ShowInfo = true;
        }
        public void DisableInfoShow()
        {
            ShowInfo = false;
            infoText.text = "";
        }

        private void Start()
        {
            G.HUD.OnPointerEnterHole += OnPointerEnterHole;
            G.HUD.OnPointerExitHole += OnPointerExitHole;
            G.HUD.OnPointerEnterAbilitySlot += OnPointerEnterAbilitySlot;
            G.HUD.OnPointerExitAbilitySlot += OnPointerExitAbilitySlot;
        }

        private void OnDestroy()
        {
            G.HUD.OnPointerEnterHole -= OnPointerEnterHole;
            G.HUD.OnPointerExitHole -= OnPointerExitHole;
            G.HUD.OnPointerEnterAbilitySlot -= OnPointerEnterAbilitySlot;
            G.HUD.OnPointerExitAbilitySlot -= OnPointerExitAbilitySlot;
        }

        private void OnPointerExitAbilitySlot(AbilitySlot slot)
        {
            infoText.text = "";
        }

        private void OnPointerEnterAbilitySlot(AbilitySlot slot)
        {
            if (slot.HasState == false) return;

            if(slot.State.Model == null)
            {
                Debug.LogError("Такого быть не должно!");
                return;
            }

            if (slot.State.Model.Is<TagDescription>(out var ds)) 
                infoText.text = ds.Description;
        }

        private void OnPointerExitHole(HoleView view)
        {
            infoText.text = "";
        }

        private void OnPointerEnterHole(HoleView view)
        {
            var info = "";

            if (view.State.HasAbility)
            {
                if (view.State.Ability.Is<TagDescription>(out var ds))
                    info += ds.Description;
            }

            infoText.text = info;
        }
    }
}