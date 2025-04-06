using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game
{
    public class AbilitySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public event Action<int> OnClicked;
        [HideInInspector] public int SlotId;

        [SerializeField] private GameObject slotObj;
        [SerializeField] private GameObject abilityObj;
        [SerializeField] private Image abilityLightObj;

        public bool AllowLightHighlightWhenPointer = false;
        public bool HasState => State != null;
        public AbilityState State;
        
        public void SetEmptySlot()
        {
            State = null;
            slotObj.SetActive(true);
            abilityObj.SetActive(false);
            abilityLightObj.gameObject.SetActive(false);
        }
        public void DestroySlot()
        {
            SetEmptySlot();
            //todo: партикл разрушения
        }
        public void SetAbility(AbilityState state, bool withSlot)
        {
            State = state;
            abilityObj.SetActive(true);
            abilityLightObj.gameObject.SetActive(false);
            slotObj.SetActive(withSlot);
        }
        public IEnumerator LightShake()
        {
            abilityLightObj.gameObject.SetActive(true);
            yield return abilityLightObj.DOFade(1, 0.25f).From(0).WaitForCompletion();
            yield return abilityLightObj.DOFade(0.5f, 0.25f).WaitForCompletion();
            yield return abilityLightObj.DOFade(1, 0.25f).WaitForCompletion();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClicked?.Invoke(SlotId);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            G.HUD.OnPointerEnterAbilitySlot?.Invoke(this);

            if (AllowLightHighlightWhenPointer)
            {
                abilityLightObj.gameObject.SetActive(HasState);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            G.HUD.OnPointerExitAbilitySlot?.Invoke(this);

            if (AllowLightHighlightWhenPointer)
            {
                abilityLightObj.gameObject.SetActive(false);
            }
        }
    }
}