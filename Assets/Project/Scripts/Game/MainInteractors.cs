using System.Collections;

namespace Game
{
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
    public class FightTurnInteraction : BaseInteraction, IOnTurnEnd
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
    }
    public interface ICanUseAbilityFilter
    {
        public bool CanUse(EntityState owner, AbilityState ability);
    }
    public interface IOnUseAbility
    {
        public IEnumerator OnUseAbility(EntityState owner, AbilityState ability);
    }
}