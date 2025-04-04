using System.Collections.Generic;
using UnityEngine;

public class TagSFXArray : EntityComponentDefinition
{
    public List<AudioClip> files = new List<AudioClip>();
    public float volume = 1f;
}
