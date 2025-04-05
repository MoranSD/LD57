using System;

namespace Game
{
    [Serializable]
    public class TagApplyDamageAbility : EntityComponentDefinition
    {
        public float Damage;
    }
    [Serializable]
    public class TagIncreaseArmorAbility : EntityComponentDefinition
    {
        public float Armor;
    }
}