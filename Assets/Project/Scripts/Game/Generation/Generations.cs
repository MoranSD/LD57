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
    public class TagHoleGenerationInfo : EntityComponentDefinition
    {
        [Range(0, 100)] public float HoleAbilityChance;
        [Range(0, 100)] public float HoleEventChance;
        public bool CanHaveBothThings;
        [Range(0, 100)] public float EnemyExistentChance;

        public List<EntityLink> AbilityLinks;
        public List<EntityLink> EventLinks;
    }
    [Serializable]
    public class TagEnemyGenerationInfo : EntityComponentDefinition
    {
        public List<EntityLink> EntityLinks;
    }
}