using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class NewAbilitySelectionPanel : MonoBehaviour
    {
        public event Action Opened;
        public bool IsVisible { get; private set; }
        public int SelectedNewAbilitySlotId { get; private set; } = -1;

        [SerializeField] private GameObject panel;
        [SerializeField] private List<AbilitySlot> slots;
        [SerializeField] private AbilitySlot newAbilitySlot;

        private Vector2 newAbilitySlotDefaultPosition;
        private int selectedSlotId = -1;
        private bool selectingSlot;

        private void Awake()
        {
            newAbilitySlot.SlotId = -1;
            newAbilitySlotDefaultPosition = newAbilitySlot.transform.position;
            newAbilitySlot.OnClicked += OnClickOnSlot;

            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].SlotId = i;
                slots[i].OnClicked += OnClickOnSlot;
            }
        }
        private void OnDestroy()
        {
            newAbilitySlot.OnClicked -= OnClickOnSlot;

            for (int i = 0; i < slots.Count; i++)
                slots[i].OnClicked -= OnClickOnSlot;
        }

        public IEnumerator ShowAbilityMoveToSlot(AbilityState abilityState, EntityState playerState, int slotId)
        {
            IsVisible = true;
            panel.SetActive(true);
            PrepareEnvironment(abilityState, playerState);
            Opened?.Invoke();

            selectingSlot = false;
            selectedSlotId = 0;
            yield return new WaitUntil(() => selectedSlotId == -1);

            yield return ShowAbilityMoveToSlot(slotId);

            yield return new WaitForSeconds(1f);
        }
        public IEnumerator SelectAbilitySlot(AbilityState abilityState, EntityState playerState)
        {
            IsVisible = true;
            panel.SetActive(true);
            PrepareEnvironment(abilityState, playerState);
            Opened?.Invoke();

            selectingSlot = true;
            selectedSlotId = -1;
            yield return new WaitWhile(() => selectedSlotId == -1);
            SelectedNewAbilitySlotId = selectedSlotId;

            slots[selectedSlotId].DestroySlot();

            yield return ShowAbilityMoveToSlot(selectedSlotId);

            yield return new WaitForSeconds(1f);
        }
        public void HideMenu()
        {
            IsVisible = false;
            panel.SetActive(false);
        }

        private IEnumerator ShowAbilityMoveToSlot(int slotId)
        {
            var targetPosition = slots[slotId].transform.position;

            yield return newAbilitySlot.transform.DOMove(targetPosition, 2).WaitForCompletion();

            slots[slotId].SetAbility(newAbilitySlot.State, true);
            newAbilitySlot.gameObject.SetActive(false);

            yield return slots[slotId].LightShake();
        }
        private void PrepareEnvironment(AbilityState abilityState, EntityState playerState)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (i >= playerState.MaxAbilitiesCount)
                {
                    slots[i].gameObject.SetActive(false);
                    continue;
                }

                slots[i].gameObject.SetActive(true);

                if (playerState.Abilities[i] != null)
                {
                    slots[i].SetAbility(playerState.Abilities[i], true);
                }
                else
                {
                    slots[i].SetEmptySlot();
                }
            }

            newAbilitySlot.transform.position = newAbilitySlotDefaultPosition;
            newAbilitySlot.gameObject.SetActive(true);
            newAbilitySlot.SetAbility(abilityState, false);
        }
        private void OnClickOnSlot(int slotId)
        {
            if (selectingSlot)
            {
                if (slotId == -1) return;
            }
            else
            {
                if (slotId != -1) return;
            }

            selectedSlotId = slotId;
        }
    }
}