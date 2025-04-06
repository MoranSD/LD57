using System;
using UnityEngine;

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
    public class TagSprite : EntityComponentDefinition
    {
        public Sprite Sprite;
    }

    [Serializable]
    public class TagDescription : EntityComponentDefinition
    {
        public string Description;
    }

    [Serializable]
    public class TagStartHealth : EntityComponentDefinition
    {
        public float Val;
    }

    [Serializable]
    public class TagMaxHealth : EntityComponentDefinition
    {
        public float Val;
    }

    [Serializable]
    public class TagStartArmor : EntityComponentDefinition
    {
        public float Val;
    }

    [Serializable]
    public class TagMaxArmor : EntityComponentDefinition
    {
        public float Val;
    }
}