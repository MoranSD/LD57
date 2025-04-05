using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class GenerationInfo
    {
        public bool HasEnemyInfo => EnemyInfo != null;
        public bool HasHoleInfo => HoleInfo != null;

        public int Grade;
        public CMSEntity EnemyInfo;
        public CMSEntity HoleInfo;
    }
    [Serializable]
    public class TagHoleGenerationChances : EntityComponentDefinition
    {
        [Range(0, 100)] public float HoleAbilityChance;
    }
    [Serializable]
    public class TagAbilitiesHolder : EntityComponentDefinition
    {
        public List<EntityLink> All;
    }
    [Serializable]
    public class TagViewsHolder : EntityComponentDefinition
    {
        public List<Sprite> All;
    }
}