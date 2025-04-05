using System;
using System.Collections.Generic;

namespace Game
{
    [Serializable]
    public class TagStartAbilities : EntityComponentDefinition
    {
        public List<EntityLink> All;
    }
    [Serializable]
    public class TagMaxAbilities : EntityComponentDefinition
    {
        public int count;
    }
}