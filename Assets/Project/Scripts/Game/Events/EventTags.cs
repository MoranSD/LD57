using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class TagChangeDamageEvent : EntityComponentDefinition
    {
        public float Damage;
        public bool Increase;
        public bool ToPlayer;//если true то эффект наложится на игрока
    }
    public class ChangeDamageEventUse : BaseInteraction, IOnApplyDamage
    {
        public IEnumerator OnApplyDamage(EntityState attacker, EntityState target, PropertyLink<float> damage)
        {
            foreach (var eventState in G.State.ActiveEvents)
            {
                if (eventState.Model.Is<TagChangeDamageEvent>(out var cd))
                {
                    if (attacker.Model.Is<TagPlayer>() != cd.ToPlayer) yield break;

                    if (cd.Increase)
                    {
                        damage.Value += cd.Damage;
                    }
                    else
                    {
                        damage.Value = Mathf.Max(0, damage.Value - cd.Damage);
                    }
                }
            }
        }
    }
    [Serializable]
    public class TagMultiplyDamageEvent : EntityComponentDefinition
    {
        [Range(0, 100f)] public float Multiplier;
        public bool Increase;
        public bool ToPlayer;
    } 
    public class MultiplyDamageEventUse : BaseInteraction, IOnApplyDamage
    {
        public IEnumerator OnApplyDamage(EntityState attacker, EntityState target, PropertyLink<float> damage)
        {
            foreach (var eventState in G.State.ActiveEvents)
            {
                if (eventState.Model.Is<TagMultiplyDamageEvent>(out var md))
                {
                    if (attacker.Model.Is<TagPlayer>() != md.ToPlayer) yield break;

                    var changeValue = (damage.Value / 100f) * md.Multiplier;
                    if (md.Increase)
                    {
                        damage.Value += changeValue;
                    }
                    else
                    {
                        damage.Value = Mathf.Max(0, damage.Value - changeValue);
                    }
                }
            }
        }
    }
    [Serializable]
    public class TagIgnoreDamageEvent : EntityComponentDefinition
    {
        public int Count;
        public bool ToPlayer;
    }
    public class IgnoreDamageEventUse : BaseInteraction, IOnApplyHoleEvent, IOnApplyDamage
    {
        public IEnumerator OnApplyDamage(EntityState attacker, EntityState target, PropertyLink<float> damage)
        {
            var eventsToRemove = new List<EventState>();

            foreach (var eventState in G.State.ActiveEvents)
            {
                if (eventState.Model.Is<TagIgnoreDamageEvent>(out var id))
                {
                    if (attacker.Model.Is<TagPlayer>() == id.ToPlayer) yield break;

                    damage.Value = 0;
                    eventState.ActiveCount--;

                    if(eventState.ActiveCount <= 0)
                        eventsToRemove.Add(eventState);
                }
            }

            yield return MainInteractorsUtil.RemoveAllEvents(eventsToRemove);
        }

        public IEnumerator OnApplyHoleEvent(HoleState holeState, EventState eventState)
        {
            if(eventState.Model.Is<TagIgnoreDamageEvent>(out var id))
            {
                eventState.ActiveCount = id.Count;
                yield break;
            }
        }
    }
    [Serializable]
    public class TagFullHealthRecoveryEvent : EntityComponentDefinition
    {

    }
    public class FullHealthRecoveryEventUse : BaseInteraction, IOnApplyHoleEvent
    {
        public IEnumerator OnApplyHoleEvent(HoleState holeState, EventState eventState)
        {
            if (eventState.Model.Is<TagFullHealthRecoveryEvent>())
            {
                var player = G.State.PlayerState;
                player.Health = player.MaxHealth;
                player.View.UpdateHealth();

                yield return MainInteractorsUtil.RemoveEvent(eventState);
            }
        }
    }
    [Serializable]
    public class TagFullArmorChangeEvent : EntityComponentDefinition
    {
        public bool Fill;
    }
    public class FullArmorChangeEventUse : BaseInteraction, IOnApplyHoleEvent
    {
        public IEnumerator OnApplyHoleEvent(HoleState holeState, EventState eventState)
        {
            if (eventState.Model.Is<TagFullArmorChangeEvent>(out var fac))
            {
                var player = G.State.PlayerState;
                player.Armor = fac.Fill ? player.MaxArmor : 0;
                player.View.UpdateArmor();
                yield return MainInteractorsUtil.RemoveEvent(eventState);
            }
        }
    }
    [Serializable]
    public class TagHealChangeEvent : EntityComponentDefinition
    {
        public float Health;
        [Min(1)] public float MinHealth = 1;
        public bool Increase;
    }
    public class HealChangeEventUse : BaseInteraction, IOnApplyHoleEvent
    {
        public IEnumerator OnApplyHoleEvent(HoleState holeState, EventState eventState)
        {
            if (eventState.Model.Is<TagHealChangeEvent>(out var hc))
            {
                var player = G.State.PlayerState;

                player.Health = hc.Increase ?
                    Mathf.Min(player.MaxHealth, player.Health + hc.Health) :
                    Mathf.Max(Mathf.Max(0, Mathf.Min(hc.MinHealth, player.MaxHealth)), player.Health - hc.Health);

                player.View.UpdateHealth();

                yield return MainInteractorsUtil.RemoveEvent(eventState);
            }
        }
    }
    [Serializable]
    public class TagArmorChangeEvent : EntityComponentDefinition
    {
        public float Armor;
        [Min(1)] public float MinArmor = 1;
        public bool Increase;
    }
    public class ArmorChangeEventUse : BaseInteraction, IOnApplyHoleEvent
    {
        public IEnumerator OnApplyHoleEvent(HoleState holeState, EventState eventState)
        {
            if (eventState.Model.Is<TagArmorChangeEvent>(out var ac))
            {
                var player = G.State.PlayerState;

                player.Armor = ac.Increase ?
                    Mathf.Min(player.MaxArmor, player.Armor + ac.Armor) :
                    Mathf.Max(Mathf.Max(0, Mathf.Min(ac.MinArmor, player.MaxArmor)), player.Armor - ac.Armor);

                player.View.UpdateArmor();

                yield return MainInteractorsUtil.RemoveEvent(eventState);
            }
        }
    }
    [Serializable]
    public class TagHalfHealthEvent : EntityComponentDefinition
    {

    }
    public class HalfHealthEventUse : BaseInteraction, IOnApplyHoleEvent
    {
        public IEnumerator OnApplyHoleEvent(HoleState holeState, EventState eventState)
        {
            if (eventState.Model.Is<TagHalfHealthEvent>())
            {
                var player = G.State.PlayerState;
                player.Health = player.MaxHealth / 2f;
                player.View.UpdateHealth();
                yield return MainInteractorsUtil.RemoveEvent(eventState);
            }
        }
    }
    [Serializable]
    public class TagHalfArmorEvent : EntityComponentDefinition
    {

    }
    public class HalfArmorEventUse : BaseInteraction, IOnApplyHoleEvent
    {
        public IEnumerator OnApplyHoleEvent(HoleState holeState, EventState eventState)
        {
            if (eventState.Model.Is<TagHalfArmorEvent>())
            {
                var player = G.State.PlayerState;
                player.Armor = player.MaxArmor / 2f;
                player.View.UpdateArmor();
                yield return MainInteractorsUtil.RemoveEvent(eventState);
            }
        }
    }
    [Serializable]
    public class TagSecondChanceEvent : EntityComponentDefinition
    {
        public float RestoreHealth;
    }
    public class SecondChanceEventUse : BaseInteraction, IOnBeforeEntityDie
    {
        public IEnumerator OnBeforeEntityDie(EntityState entity)
        {
            if (entity.Model.Is<TagPlayer>() == false) yield break;

            var eventsToRemove = new List<EventState>();

            foreach (var eventState in G.State.ActiveEvents)
            {
                if (eventState.Model.Is<TagSecondChanceEvent>(out var sc))
                {
                    G.State.PlayerState.Health = sc.RestoreHealth;
                    G.State.PlayerState.View.UpdateHealth();

                    eventsToRemove.Add(eventState);
                }
            }

            yield return MainInteractorsUtil.RemoveAllEvents(eventsToRemove);
        }
    }
}