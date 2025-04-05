using System;

namespace Game
{
    [Serializable]
    public class AbilityState
    {
        public bool HasModel => Model != null;
        public CMSEntity Model;

        public void SetModel(CMSEntity model)
        {
            Model = model;
        }
    }
}