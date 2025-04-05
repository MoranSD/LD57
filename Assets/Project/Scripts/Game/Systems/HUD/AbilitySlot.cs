using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game
{
    public class AbilitySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public event Action<int> OnClicked;
        [HideInInspector] public int SlotId;

        [SerializeField] private GameObject slotObj;
        [SerializeField] private GameObject abilityObj;

        public bool HasState => State != null;
        public AbilityState State;
        
        public void SetEmptySlot()
        {
            State = null;
            slotObj.SetActive(true);
            abilityObj.SetActive(false);
        }
        public void SetAbility(AbilityState state, bool withSlot)
        {
            State = state;
            abilityObj.SetActive(true);
            slotObj.SetActive(withSlot);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClicked?.Invoke(SlotId);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            G.HUD.OnPointerEnterAbilitySlot?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            G.HUD.OnPointerExitAbilitySlot?.Invoke(this);
        }
    }
}