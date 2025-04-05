using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class EntityState
    {
        public CMSEntity Model;
        public EntityView View;

        public float MaxHealth;
        public float Health;
        public float MaxArmor;
        public float Armor;

        public int MaxAbilitiesCount = 1;
        public int AbilitiesCount => Abilities == null ? 0 : Abilities.Where(x => x != null).Count();
        public AbilityState[] Abilities;

        public void SetModel(CMSEntity model)
        {
            Model = model;

            if (model.Is<TagMaxAbilities>(out var maxAbilities))
                MaxAbilitiesCount = maxAbilities.count;
            else
                MaxAbilitiesCount = 1;

            Abilities = new AbilityState[MaxAbilitiesCount];

            if (model.Is<TagStartAbilities>(out var startAbilities))
            {
                for (int i = 0; i < startAbilities.All.Count && i < MaxAbilitiesCount; i++)
                {
                    EntityLink abilityId = startAbilities.All[i];
                    var abilityState = new AbilityState();
                    abilityState.SetModel(CMS.Get<CMSEntity>(abilityId.Id));
                    Abilities[i] = abilityState;
                }
            }

            if (model.Is<TagStartHealth>(out var sh))
                Health = sh.Val;
            else
                Debug.Log($"Not start health tag", View);

            if (model.Is<TagMaxHealth>(out var mh))
                MaxHealth = mh.Val;
            else
                MaxHealth = Health;

            if (model.Is<TagStartArmor>(out var sa))
                Armor = sa.Val;
            else
                Debug.Log($"Not start armor tag", View);

            if (model.Is<TagMaxArmor>(out var ma))
                MaxArmor = ma.Val;
            else
                MaxArmor = Armor;
        }
    }
}