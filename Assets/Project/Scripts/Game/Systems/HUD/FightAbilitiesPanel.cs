using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class FightAbilitiesPanel : MonoBehaviour
    {
        public event Action<int> OnSelectAbility;

        [SerializeField] private GameObject panel;
        [SerializeField] private List<AbilitySlot> slots;

        private void Awake()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].SlotId = i;
                slots[i].OnClicked += OnClickOnSlot;
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < slots.Count; i++)
                slots[i].OnClicked -= OnClickOnSlot;
        }

        public void Show()
        {
            panel.SetActive(true);

            var playerState = G.State.PlayerState;

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
        }
        public void Hide()
        {
            panel.SetActive(false);
        }

        private void OnClickOnSlot(int abilityId)
        {
            OnSelectAbility?.Invoke(abilityId);
        }
    }
}