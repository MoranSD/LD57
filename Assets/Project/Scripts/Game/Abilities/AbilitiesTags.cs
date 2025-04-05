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
            if (ability.Model.Is<TagIncreaseArmorAbility>())
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
                owner.View.UpdateArmor();
                yield break;
            }
        }
    }
    [Serializable]
    public class TagApplyDamageThroughArmorAbility : EntityComponentDefinition
    {
        public float Damage;
    }
    public class ApplyDamageThroughArmorAbilityUse : BaseInteraction, IOnUseAbility
    {
        public IEnumerator OnUseAbility(EntityState owner, AbilityState ability)
        {
            if (ability.Model.Is<TagApplyDamageThroughArmorAbility>(out var adta))
            {
                var opponent = G.Main.GetOpponent(owner);
                yield return G.Main.ApplyDamage(owner, opponent, adta.Damage, true);
            }
        }
    }
    [Serializable]
    public class TagApplyStunAbility : EntityComponentDefinition
    {
        public int StunCycles;
    }
    public class ApplyStunAbilityCheck : BaseInteraction, ICanUseAbilityFilter
    {
        public bool CanUse(EntityState owner, AbilityState ability)
        {
            if (ability.Model.Is<TagApplyStunAbility>())
            {
                var opponent = G.Main.GetOpponent(owner);
                return !opponent.IsStunned && opponent.CyclesAfterStun >= 1;
            }

            return true;
        }
    }
    public class ApplyStunAbilityUse : BaseInteraction, IOnUseAbility
    {
        public IEnumerator OnUseAbility(EntityState owner, AbilityState ability)
        {
            if (ability.Model.Is<TagApplyStunAbility>(out var ap))
            {
                var opponent = G.Main.GetOpponent(owner);
                yield return G.Main.ApplyStun(owner, opponent, ap.StunCycles);
            }
        }
    }
    [Serializable]
    public class TagRemovePerscentArmorAbility : EntityComponentDefinition
    {
        [Range(0f, 100f)] public float RemovePerscent;
        public bool FromMax;
    }
    public class RemovePerscentArmorAbilityCheck : BaseInteraction, ICanUseAbilityFilter
    {
        public bool CanUse(EntityState owner, AbilityState ability)
        {
            if (ability.Model.Is<TagRemovePerscentArmorAbility>())
            {
                var opponent = G.Main.GetOpponent(owner);

                return opponent.Armor > 0;
            }

            return true;
        }
    }
    public class RemovePerscentArmorAbilityUse : BaseInteraction, IOnUseAbility
    {
        public IEnumerator OnUseAbility(EntityState owner, AbilityState ability)
        {
            if(ability.Model.Is<TagRemovePerscentArmorAbility>(out var rpa))
            {
                var opponent = G.Main.GetOpponent(owner);
                var removeArmorPoints = ((rpa.FromMax ? opponent.MaxArmor : opponent.Armor) / 100f) * rpa.RemovePerscent;
                opponent.Armor = Mathf.Max(0, opponent.Armor - removeArmorPoints);
                opponent.View.UpdateArmor();
                yield break;
            }
        }
    }
    [Serializable]
    public class TagApplyBleedingAbility : EntityComponentDefinition
    {
        public int Cycles;
        public float Damage;
    }
    public class ApplyBleedingCheck : BaseInteraction, ICanUseAbilityFilter
    {
        public bool CanUse(EntityState owner, AbilityState ability)
        {
            if (ability.Model.Is<TagApplyBleedingAbility>())
            {
                return !ability.IsActive;
            }

            return true;
        }
    }
    public class ApplyBleedingUse : BaseInteraction, IOnUseAbility, IOnTurnEnd
    {
        public IEnumerator OnTurnEnd(TurnTeam team)
        {
            var owner = G.Main.GetEntity(team.Opposite());
            foreach (var ability in owner.Abilities) 
            {
                if (ability == null) continue;

                if (ability.Model.Is<TagApplyBleedingAbility>(out var ab)) 
                {
                    if (!ability.IsActive) continue;

                    ability.ActiveCycles--;
                    var opponent = G.Main.GetOpponent(owner);
                    yield return G.Main.ApplyDamage(owner, opponent, ab.Damage, true);
                }
            }
        }

        public IEnumerator OnUseAbility(EntityState owner, AbilityState ability)
        {
            if (ability.Model.Is<TagApplyBleedingAbility>(out var ab))
            {
                ability.ActiveCycles = ab.Cycles;
                var opponent = G.Main.GetOpponent(owner);
                yield return G.Main.ApplyDamage(owner, opponent, ab.Damage, true);
            }
        }
    }
    [Serializable]
    public class TagApplyExtraHealAbility : EntityComponentDefinition
    {
        public float Health;
        public int StunCycles;
    }
    public class ApplyExtraHealAbilityCheck : BaseInteraction, ICanUseAbilityFilter
    {
        public bool CanUse(EntityState owner, AbilityState ability)
        {
            if (ability.Model.Is<TagApplyExtraHealAbility>())
            {
                var remainingPerscent = (owner.Health / owner.MaxHealth) * 100f;
                return remainingPerscent <= 25;
            }

            return true;
        }
    }
    public class ApplyExtraHealAbilityUse : BaseInteraction, IOnUseAbility
    {
        public IEnumerator OnUseAbility(EntityState owner, AbilityState ability)
        {
            if(ability.Model.Is<TagApplyExtraHealAbility>(out var aeh))
            {
                yield return G.Main.ApplySelfHeal(owner, aeh.Health);
                yield return G.Main.ApplySelfStun(owner, aeh.StunCycles);   
            }
        }
    }
}