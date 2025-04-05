using System;
using UnityEngine;

namespace Game
{
    public class EntityView : MonoBehaviour
    {
        public EntityState State;
        public Transform healthBarPivot;
        public Transform armorBarPivot;
        public GameObject armorBarParent;

        public void SetState(EntityState state)
        {
            State = state;
            State.View = this;

            UpdateHealth();
            UpdateArmor();
        }

        public void UpdateHealth()
        {
            var cur = State.Health;
            var max = State.MaxHealth;
            healthBarPivot.localScale = new(cur / max, 1, 1);
        }

        public void UpdateArmor()
        {
            var cur = State.Armor;
            var max = State.MaxArmor;

            bool noArmor = cur == 0 && max == 0;
            armorBarParent.SetActive(!noArmor);

            if (noArmor) return;

            armorBarPivot.localScale = new(cur / max, 1, 1);
        }

        public void DrawDie()
        {
            Debug.Log("todo: entity die");
        }
    }
}