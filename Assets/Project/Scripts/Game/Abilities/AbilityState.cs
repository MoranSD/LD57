using System;

namespace Game
{
    [Serializable]
    public class AbilityState
    {
        public CMSEntity Model;

        public void SetModel(CMSEntity model)
        {
            Model = model;
        }
    }
}