using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class CMSPfbPreBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        var resources = Resources.LoadAll<CMSEntityPfb>("CMS");

        foreach (var component in resources)
        {
            component.AssignBuildId();

            var prefabPath = AssetDatabase.GetAssetPath(component);
            if (!string.IsNullOrEmpty(prefabPath))
            {
                PrefabUtility.SavePrefabAsset(component.gameObject);
            }
        }

        AssetDatabase.SaveAssets();
    }
}
#endif