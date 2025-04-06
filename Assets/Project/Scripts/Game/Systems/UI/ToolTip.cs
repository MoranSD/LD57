using Common;
using System;
using System.Linq;
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

        private bool isShowingHoleInfo;
        private bool isShowingEntityInfo;

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
            G.HUD.AbilitySelectionPanel.Opened += OnOpenAbilitySelection;
            G.Main.OnChangeScene += OnChangeScene;
            G.HUD.OnPointerEnterEntity += OnPointerEnterEntity;
            G.HUD.OnPointerExitEntity += OnPointerExitEntity;
        }

        private void OnDestroy()
        {
            G.HUD.OnPointerEnterHole -= OnPointerEnterHole;
            G.HUD.OnPointerExitHole -= OnPointerExitHole;
            G.HUD.OnPointerEnterAbilitySlot -= OnPointerEnterAbilitySlot;
            G.HUD.OnPointerExitAbilitySlot -= OnPointerExitAbilitySlot;
            G.HUD.AbilitySelectionPanel.Opened -= OnOpenAbilitySelection;
            G.Main.OnChangeScene -= OnChangeScene;
            G.HUD.OnPointerEnterEntity -= OnPointerEnterEntity;
            G.HUD.OnPointerExitEntity -= OnPointerExitEntity;
        }

        private void OnPointerExitEntity(EntityView view)
        {
            if (G.HUD.AbilitySelectionPanel.IsVisible) return;
            if (isShowingEntityInfo == false) return;

            isShowingEntityInfo = false;
            infoText.text = "";
        }

        private void OnPointerEnterEntity(EntityView view)
        {
            if (G.HUD.AbilitySelectionPanel.IsVisible) return;

            var info = "";

            info += $"Health: {view.State.Health}\n";
            info += $"Armor: {view.State.Armor}\n";

            if (view.State.IsStunned)
                info += $"Stunned - {view.State.StunCycles}\n";
            if (view.State.IsBleeding)
                info += $"Stunned - {view.State.BleedingCycles}\n";

            if (view.State.Model.Is<TagPlayer>() == false)
            {
                info += "Abilities:\n";

                foreach (var ability in view.State.Abilities.Select(x => x.Model))
                {
                    if (ability.Is<TagDescription>(out var d))
                        info += "   " + d.Description + "\n";
                }
            }

            infoText.text = info;
            isShowingEntityInfo = true;
        }

        private void OnChangeScene()
        {
            isShowingEntityInfo = false;
            isShowingHoleInfo = false;
            infoText.text = "";
        }

        private void OnOpenAbilitySelection()
        {
            if (isShowingHoleInfo || isShowingEntityInfo)
            {
                isShowingEntityInfo = false;
                isShowingHoleInfo = false;
                infoText.text = "";
            }
        }

        private void OnPointerExitAbilitySlot(AbilitySlot slot)
        {
            infoText.text = "";
        }

        private void OnPointerEnterAbilitySlot(AbilitySlot slot)
        {
            if (slot.HasState == false) return;

            if (slot.State.Model == null)
            {
                Debug.LogError("Такого быть не должно!");
                return;
            }

            if (slot.State.Model.Is<TagDescription>(out var ds)) 
                infoText.text = ds.Description;
        }

        private void OnPointerExitHole(HoleView view)
        {
            if (G.HUD.AbilitySelectionPanel.IsVisible) return;
            if (isShowingHoleInfo == false) return;

            isShowingHoleInfo = false;
            infoText.text = "";
        }

        private void OnPointerEnterHole(HoleView view)
        {
            if (G.HUD.AbilitySelectionPanel.IsVisible) return;
            if (view.State == null) return;

            isShowingHoleInfo = true;
            var info = "";

            if (view.State.HasEvent) 
            {
                if (view.State.Event.Is<TagDescription>(out var ds))
                    info += $"Event - {ds.Description}";
            }

            info += "\n\n";

            if (view.State.HasAbility)
            {
                if (view.State.Ability.Is<TagDescription>(out var ds))
                    info += $"Ability - {ds.Description}";
            }

            infoText.text = info;
        }
    }
}