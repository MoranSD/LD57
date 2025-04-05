using UnityEngine;

namespace Game
{
    public class EntityView : MonoBehaviour
    {
        public EntityState State;

        public void SetState(EntityState state)
        {
            State = state;
            State.View = this;
        }
    }
}