#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ResourceClassGenerator : AssetPostprocessor
{
    public const string PATH_TO_RESOURCES_FOLDER = "Resources";
    public const string PATH_TO_OUTPUT = "Generated";
    public const string OUTPUT_CLASS_NAME = "GameResources";
    public const string IGNORED_FOLDER_PREFIX = "IgnoreTag"; // Папки с этим префиксом игнорируются

    private static bool _pendingGenerate = false;

    [MenuItem("Tools/Generate Resource Class")]
    public static void GenerateResourceClass()
    {
        string resourcesPath = Path.Combine(Application.dataPath, PATH_TO_RESOURCES_FOLDER);
        if (!Directory.Exists(resourcesPath))
        {
            Debug.LogError("Resources folder not found at path: " + resourcesPath);
            return;
        }

        string outputDirectory = Path.Combine(Application.dataPath, PATH_TO_OUTPUT);
        string outputPath = Path.Combine(outputDirectory, $"{OUTPUT_CLASS_NAME}.cs");

        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        HashSet<string> namespaces = new HashSet<string> { "UnityEngine", "UnityEngine.UIElements" };

        StringBuilder classBuilder = new StringBuilder();
        classBuilder.AppendLine("// This file is auto-generated. Do not modify manually.");
        classBuilder.AppendLine();

        classBuilder.AppendLine($"public static class {OUTPUT_CLASS_NAME}");
        classBuilder.AppendLine("{");

        GenerateClassForFolder(resourcesPath, classBuilder, "    ", namespaces);

        classBuilder.AppendLine("}");

        StringBuilder finalBuilder = new StringBuilder();
        foreach (string ns in namespaces.OrderBy(n => n))
        {
            finalBuilder.AppendLine($"using {ns};");
        }

        finalBuilder.AppendLine();
        finalBuilder.Append(classBuilder.ToString());

        var isNewFile = !File.Exists(outputPath);
        File.WriteAllText(outputPath, finalBuilder.ToString());

        if (isNewFile)
        {
            AssetDatabase.Refresh();
            Debug.Log($"Generated {OUTPUT_CLASS_NAME} class at: {outputPath}");
        }
    }

    private static void GenerateClassForFolder(string folderPath, StringBuilder classBuilder, string indent, HashSet<string> namespaces)
    {
        string[] subFolders = Directory.GetDirectories(folderPath);
        string[] files = Directory.GetFiles(folderPath);

        foreach (string subFolder in subFolders)
        {
            string folderName = Path.GetFileName(subFolder);

            // Пропускаем папки с префиксом игнорирования
            if (folderName.StartsWith(IGNORED_FOLDER_PREFIX))
            {
                continue;
            }

            string validFolderName = EscapeToValidIdentifier(folderName);
            classBuilder.AppendLine($"{indent}public static class {validFolderName}");
            classBuilder.AppendLine($"{indent}{{");
            GenerateClassForFolder(subFolder, classBuilder, indent + "    ", namespaces);
            classBuilder.AppendLine($"{indent}}}");
        }

        foreach (string file in files)
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
            if (string.IsNullOrEmpty(fileNameWithoutExtension)) continue;

            string fileName = EscapeToValidIdentifier(fileNameWithoutExtension);
            string relativePath = GetRelativePath(file);

            string assetType = GetAssetType(file, relativePath, namespaces);
            if (!string.IsNullOrEmpty(assetType))
            {
                classBuilder.AppendLine($"{indent}public static {assetType} {fileName} => Resources.Load<{assetType}>(\"{relativePath}\");");
            }
        }
    }

    private static string GetAssetType(string filePath, string relativePath, HashSet<string> namespaces)
    {
        string fileExtension = Path.GetExtension(filePath).ToLower();

        switch (fileExtension)
        {
            case ".prefab":
                string componentType = GetFirstMonoBehaviourType(relativePath, namespaces);
                return string.IsNullOrEmpty(componentType) ? "GameObject" : componentType;
            case ".txt":
            case ".json":
            case ".xml":
                return "TextAsset";
            case ".mp3":
            case ".wav":
            case ".ogg":
                return "AudioClip";
            case ".png":
            case ".jpg":
            case ".jpeg":
                return "Sprite";
            case ".mat":
                return "Material";
            case ".shader":
                return "Shader";
            case ".uss":
                return "StyleSheet";
            default:
                return null;
        }
    }

    private static string GetFirstMonoBehaviourType(string relativePath, HashSet<string> namespaces)
    {
        string fullPath = $"Assets/{PATH_TO_RESOURCES_FOLDER}/" + relativePath + ".prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found: <color=red>{fullPath}</color>");
            return null;
        }

        Component[] components = prefab.GetComponents<Component>();
        foreach (Component component in components)
        {
            if (component != null && component is MonoBehaviour)
            {
                Type type = component.GetType();
                if (!string.IsNullOrEmpty(type.Namespace))
                {
                    namespaces.Add(type.Namespace);
                }
                return type.Name;
            }
        }

        return null;
    }

    private static string EscapeToValidIdentifier(string name)
    {
        StringBuilder validName = new StringBuilder();
        foreach (char c in name)
        {
            validName.Append(char.IsLetterOrDigit(c) ? c : '_');
        }

        if (validName.Length == 0) return "_empty_";
        if (char.IsDigit(validName[0])) validName.Insert(0, '_');
        if (IsCSharpKeyword(validName.ToString())) validName.Insert(0, '@');

        return validName.ToString();
    }

    private static bool IsCSharpKeyword(string word)
    {
        string[] keywords = {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
            "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit",
            "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in",
            "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator",
            "out", "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte",
            "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw",
            "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void",
            "volatile", "while"
        };

        return Array.Exists(keywords, k => k == word);
    }

    private static string GetRelativePath(string fullPath)
    {
        int index = fullPath.IndexOf("Resources", StringComparison.Ordinal);
        if (index == -1) return null;

        string relativePath = fullPath.Substring(index + "Resources/".Length);
        return relativePath.Replace("\\", "/").Replace(Path.GetExtension(fullPath), "");
    }

    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (!_pendingGenerate)
        {
            _pendingGenerate = true;
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isCompiling)
                {
                    EditorApplication.delayCall += OnRecompileCompleted;
                }
                else
                {
                    OnRecompileCompleted();
                }
            };
        }
    }

    private static void OnRecompileCompleted()
    {
        if (!EditorApplication.isCompiling)
        {
            _pendingGenerate = false;
            GenerateResourceClass();
        }
    }
}
#endif