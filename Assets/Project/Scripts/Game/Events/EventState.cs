using System;

namespace Game
{
    public class EventState
    {
        public CMSEntity Model;

        public int ActiveCount;

        public void SetModel(CMSEntity model)
        {
            Model = model;
        }
    }
}