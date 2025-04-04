using System;

namespace Common
{
    public class TagAnything : EntityComponentDefinition
    {
    }

    [Serializable]
    public class TagPrefab : EntityComponentDefinition
    {
        public UnityEngine.Object Prefab;
    }

    [Serializable]
    public class TagDescription : EntityComponentDefinition
    {
        public string Description;
    }

    [Serializable]
    public class TagStartHealth : EntityComponentDefinition
    {
        public int Val;
    }
}