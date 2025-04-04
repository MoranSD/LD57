using UnityEngine;

namespace Common
{
    public class TagSFX : EntityComponentDefinition
    {
        public string sfx_id;
        public float Cooldown = 0.1f;
        public bool VaryPitch;
    }

    public class TagMusic : EntityComponentDefinition
    {
        public AudioClip clip;
    }

    public class TagAmbient : EntityComponentDefinition
    {
        public AudioClip clip;
    }
}