using System.Collections;

namespace Game
{
    public enum TurnTeam
    {
        Player,
        Enemy,
        NoOne,
    }
    public interface IOnTurnBegin
    {
        IEnumerator OnTurnBegin(TurnTeam team);
    }
    public interface IOnTurnEnd
    {
        IEnumerator OnTurnEnd(TurnTeam team);
    }
    public class TurnEndInteraction : BaseInteraction, IOnTurnEnd
    {
        public IEnumerator OnTurnEnd(TurnTeam team)
        {
            G.State.EnemyState.UsedAbilitiesCount = 0;
            G.State.EnemyState.UsedItemsCount = 0;

            G.State.PlayerState.UsedAbilitiesCount = 0;
            G.State.PlayerState.UsedItemsCount = 0;

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