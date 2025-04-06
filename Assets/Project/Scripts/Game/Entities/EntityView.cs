using Common;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

namespace Game
{
    public class EntityView : MonoBehaviour
    {
        public EntityState State;
        public Transform healthBarPivot;
        public Transform armorBarPivot;
        public Transform addHealthBarPivot;
        public Transform addArmorBarPivot;
        public GameObject armorBarParent;
        public GameObject bleedingEffectIcon;
        public GameObject stunEffectIcon;
        public SpriteRenderer ViewSprite;
        public Sprite goodHealth;
        public Sprite okHealth;
        public Sprite badHealth;
        public ParticleSystem healEffect;
        public ParticleSystem getDamageEffect;
        public ParticleSystem deathEffect;

        public void SetState(EntityState state)
        { 
            State = state;
            State.View = this;

            ViewSprite.enabled = true;
            if (state.Model.Is<TagSprite>(out var s))
                ViewSprite.sprite = s.Sprite;

            UpdateHealth(true);
            UpdateArmor(true);
        }

        private void OnDestroy()
        {
            DOTween.Kill(addHealthBarPivot);
            DOTween.Kill(addArmorBarPivot);
        }

        private void OnMouseEnter()
        {
            G.HUD.OnPointerEnterEntity(this);
        }

        private void OnMouseExit()
        {
            G.HUD.OnPointerExitEntity(this);
        }

        public void SetBleeding(bool active) => bleedingEffectIcon.SetActive(active);
        public void SetStun(bool active) => stunEffectIcon.SetActive(active);

        public void UpdateHealth(bool instant = false)
        {
            var cur = Mathf.Max(State.Health, 0);
            var max = State.MaxHealth;
            DOTween.Kill(addHealthBarPivot);
            addHealthBarPivot.DOScaleX(cur / max, instant ? 0 : 0.5f);
            healthBarPivot.localScale = new(cur / max, 1, 1);
            UpdateHealthView();
        }
        public void UpdateArmor(bool instant = false)
        {
            UpdateHealthView();
            var cur = State.Armor;
            var max = State.MaxArmor;

            bool noArmor = cur == 0 && max == 0;
            armorBarParent.SetActive(!noArmor);

            if (noArmor) return;

            DOTween.Kill(addArmorBarPivot);
            addArmorBarPivot.DOScaleX(cur / max, instant ? 0 : 0.5f);
            armorBarPivot.localScale = new(cur / max, 1, 1);
        }

        public void DrawHeal()
        {
            healEffect.Emit(5);
        }

        [ContextMenu("test")]
        private void test()
        {
            StartCoroutine(DrawDie());
        }

        public IEnumerator DrawApplyDamage()
        {
            getDamageEffect.Emit(10);
            yield return transform.DOShakePosition(1, Vector3.right * 0.25f, 25, 90).WaitForCompletion();
        }

        public IEnumerator DrawDie()
        {
            yield return transform.DOShakePosition(1, Vector3.right * 0.25f, 25, 90).WaitForCompletion();
            yield return new WaitForSeconds(0.5f);
            ViewSprite.enabled = false;
            deathEffect.Emit(10);
            yield return new WaitForSeconds(1f);
        }

        private void UpdateHealthView()
        {
            if (State.Model.Is<TagPlayer>() == false) return;

            var totalHealth = State.Health + State.Armor;
            var maxTotal = State.MaxHealth + State.MaxArmor;
            var healthState = totalHealth / maxTotal;

            if (healthState >= 0.66f)
            {
                ViewSprite.sprite = goodHealth;
            }
            else if (healthState >= 0.33f)
            {
                ViewSprite.sprite = okHealth;
            }
            else
            {
                ViewSprite.sprite = badHealth;
            }
        }
    }
}