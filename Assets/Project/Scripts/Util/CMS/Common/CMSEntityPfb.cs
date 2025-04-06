using System;
using System.Collections.Generic;
using UnityEngine;
public class CMSEntityPfb : MonoBehaviour
{
    public Sprite Icon;
    public string builtId;

    [SerializeReference, SubclassSelector]
    public List<EntityComponentDefinition> Components;

    public bool Is<T>(out T unknown) where T : EntityComponentDefinition, new()
    {
        unknown = Get<T>();
        return unknown != null;
    }

    public bool Is<T>() where T : EntityComponentDefinition, new()
    {
        return Get<T>() != null;
    }

    public bool Is(Type type)
    {
        return Components.Find(m => m.GetType() == type) != null;
    }

    public T Get<T>() where T : EntityComponentDefinition, new()
    {
        return Components.Find(m => m is T) as T;
    }

    public string GetId()
    {
#if UNITY_EDITOR
        // Get the path of the prefab in the Resources folder
        string path = UnityEditor.AssetDatabase.GetAssetPath(gameObject);

        // Remove the "Assets/Resources/" part and ".prefab" to match the Resources.Load format
        if (path.StartsWith("Assets/Resources/CMS") && path.EndsWith(".prefab"))
        {
            path = path.Substring("Assets/Resources/CMS".Length);
            path = path.Substring(0, path.Length - ".prefab".Length);
        }

        return path;
#else
        return builtId;
#endif
    }

    public void AssignBuildId()
    {
        builtId = GetId();
    }

    public override string ToString()
    {
        return GetId();
    }

    public CMSEntity AsEntity()
    {
        return CMS.Get<CMSEntity>(GetId());
    }
}