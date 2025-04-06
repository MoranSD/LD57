using System;
using System.Collections;
using System.Collections.Generic;

namespace Game
{
    public static class MainInteractorsUtil
    {
        public static IEnumerator RemoveAllEvents(List<EventState> eventStates)
        {
            foreach (var eventState in eventStates)
                G.State.ActiveEvents.Remove(eventState);

            yield break;
        }

        public static IEnumerator RemoveEvent(EventState eventState)
        {
            G.State.ActiveEvents.Remove(eventState);
            yield break;
        }
    }
    public enum TurnTeam
    {
        Player,
        Enemy,
        NoOne,
    }
    public static class EnumExtensions
    {
        public static TurnTeam Opposite(this TurnTeam team)
        {
            return team switch
            {
                TurnTeam.Player => TurnTeam.Enemy,
                TurnTeam.Enemy => TurnTeam.Player,
                TurnTeam.NoOne => TurnTeam.NoOne,
                _ => TurnTeam.NoOne,
            };
        }
    }
    public interface IOnTurnBegin
    {
        IEnumerator OnTurnBegin(TurnTeam team);
    }
    public interface IOnTurnEnd
    {
        IEnumerator OnTurnEnd(TurnTeam team);
    }
    public class FightTurnInteraction : BaseInteraction, IOnTurnEnd, IOnFightEnd
    {
        public override int Priority()
        {
            return PriorityLayers.FIRST;
        }
        public IEnumerator OnTurnEnd(TurnTeam team)
        {
            if (team == TurnTeam.Player)
            {
                G.State.PlayerState.UsedAbilitiesCount = 0;
                G.State.PlayerState.UsedItemsCount = 0;

                if (G.State.PlayerState.IsStunned)
                {
                    G.State.PlayerState.StunCycles--;
                }
                else
                {
                    G.State.PlayerState.CyclesAfterStun++;
                }
            }
            else if (team == TurnTeam.Enemy)
            {
                G.State.EnemyState.UsedAbilitiesCount = 0;
                G.State.EnemyState.UsedItemsCount = 0;

                if (G.State.EnemyState.IsStunned)
                {
                    G.State.EnemyState.StunCycles--;
                }
                else
                {
                    G.State.EnemyState.CyclesAfterStun++;
                }
            }

            yield break;
        }

        public IEnumerator OnFightEnd()
        {
            G.State.PlayerState.StunCycles = 0;
            G.State.PlayerState.CyclesAfterStun = 0;
            G.State.PlayerState.UsedAbilitiesCount = 0;
            G.State.PlayerState.UsedItemsCount = 0;
            G.State.ActiveEvents.Clear();
            yield break;
        }
    }
    public interface ICanUseAbilityFilter
    {
        public bool CanUse(EntityState owner, AbilityState ability);
    }
    public interface IOnUseAbility
    {
        public IEnumerator OnUseAbility(EntityState owner, AbilityState ability);
    }
    public interface IOnApplyHoleEvent
    {
        public IEnumerator OnApplyHoleEvent(HoleState holeState, EventState eventState);
    }
    public interface IOnApplyDamage
    {
        public IEnumerator OnApplyDamage(EntityState attacker, EntityState target, PropertyLink<float> damage);
    }
    public interface IOnFightBegin
    {
        public IEnumerator OnFightBegin();
    }
    public interface IOnFightEnd
    {
        public IEnumerator OnFightEnd();
    }
    public interface IOnBeforeEntityDie
    {
        public IEnumerator OnBeforeEntityDie(EntityState entity);
    }
}