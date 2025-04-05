using System;
using System.Collections;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class TagApplyDamageAbility : EntityComponentDefinition
    {
        public float Damage;
    }
    public class ApplyDamageAbilityUse : BaseInteraction, IOnUseAbility
    {
        public IEnumerator OnUseAbility(EntityState owner, AbilityState ability)
        {
            if(ability.Model.Is<TagApplyDamageAbility>(out var ad))
            {
                var opponent = G.Main.GetOpponent(owner);
                yield return G.Main.ApplyDamage(owner, opponent, ad.Damage);
            }
        }
    }
    [Serializable]
    public class TagIncreaseArmorAbility : EntityComponentDefinition
    {
        public float Armor;
    }
    public class IncreaseArmorAbilityCheck : BaseInteraction, ICanUseAbilityFilter
    {
        public bool CanUse(EntityState owner, AbilityState ability)
        {
            if (ability.Model.Is<TagIncreaseArmorAbility>(out var ia))
            {
                return owner.Armor < owner.MaxArmor;
            }

            return true;
        }
    }
    public class IncreaseArmorAbilityUse : BaseInteraction, IOnUseAbility
    {
        public IEnumerator OnUseAbility(EntityState owner, AbilityState ability)
        {
            if (ability.Model.Is<TagIncreaseArmorAbility>(out var ia))
            {
                owner.Armor = Mathf.Min(owner.MaxArmor, owner.Armor + ia.Armor);
                yield break;
            }
        }
    }
}