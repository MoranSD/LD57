using System;

namespace Game
{
    [Serializable]
    public class AbilityState
    {
        public bool HasModel => Model != null;
        public CMSEntity Model;

        public bool IsActive => ActiveCycles > 0;
        public int ActiveCycles;

        public void SetModel(CMSEntity model)
        {
            Model = model;
        }
    }
}