using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

public class CMSEntityExplorer : EditorWindow
{
    private const string SEARCH_PATH = "Assets/Resources";
    
    private string searchQuery = "";
    private TreeViewState treeViewState;
    private EntityTreeView treeView;
    private Vector2 scrollPosition;
    
    private GUIStyle clearButtonStyle;
    
    [MenuItem("Tools/Explore/CMS Entity Explorer")]
    public static void ShowWindow()
    {
        var window = GetWindow<CMSEntityExplorer>();
        window.titleContent = new GUIContent("CMS Entity Explorer");
        window.Show();
    }

    private void OnEnable()
    {
        CMS.Init();
        
        if (treeViewState == null)
            treeViewState = new TreeViewState();

        treeView = new EntityTreeView(treeViewState);
        PerformSearch();
    }

    private void OnGUI()
    {
        if (treeView == null)
        {
            OnEnable();
            return;
        }

        if (clearButtonStyle == null)
        {
            clearButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, 0, 0),
                fixedWidth = 16,
                fixedHeight = 16
            };
        }
        
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("CMS Entity Explorer", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        string newSearch = EditorGUILayout.TextField(searchQuery, EditorStyles.toolbarSearchField);
        
        if (!string.IsNullOrEmpty(searchQuery))
        {
            if (GUILayout.Button("×", clearButtonStyle, GUILayout.Width(16)))
            {
                newSearch = "";
                GUI.FocusControl(null);
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (newSearch != searchQuery)
        {
            searchQuery = newSearch;
            PerformSearch();
        }
        EditorGUILayout.EndHorizontal();

        var rect = EditorGUILayout.GetControlRect(false, GUILayout.ExpandHeight(true));
        if (treeView != null)
        {
            treeView.OnGUI(rect);
        }
    }

    private void PerformSearch()
    {
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { SEARCH_PATH });
        var results = new List<SearchResult>();

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null)
            {
                var cmsEntity = prefab.GetComponent<CMSEntityPfb>();
                
                if (cmsEntity != null)
                {
                    if (string.IsNullOrEmpty(searchQuery) || 
                        (cmsEntity.name != null && cmsEntity.name.ToLower().Contains(searchQuery.ToLower())))
                    {
                        results.Add(new SearchResult 
                        { 
                            Prefab = prefab,
                            Entity = cmsEntity,
                            DisplayName = $"{prefab.name} ({cmsEntity.GetType().Name})",
                            Sprite = cmsEntity.Icon
                        });
                    }
                }
            }
        }

        treeView.SetSearchResults(results);
    }
}

public class EntityTreeView : TreeView
{
    private List<SearchResult> searchResults = new List<SearchResult>();
    private const float ROW_HEIGHT = 32f;  // Increased height to accommodate sprite

    public EntityTreeView(TreeViewState state) : base(state)
    {
        rowHeight = ROW_HEIGHT;
        Reload();
    }

    public void SetSearchResults(List<SearchResult> results)
    {
        searchResults = results;
        Reload();
    }

    protected override TreeViewItem BuildRoot()
    {
        var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
        var items = new List<TreeViewItem>();

        for (int i = 0; i < searchResults.Count; i++)
        {
            var result = searchResults[i];
            items.Add(new EntityTreeViewItem 
            { 
                id = i + 1,
                depth = 0,
                displayName = result.DisplayName,
                prefab = result.Prefab,
                entity = result.Entity,
                sprite = result.Sprite
            });
        }

        root.children = items;
        return root;
    }

    protected override void RowGUI(RowGUIArgs args)
    {
        if (args.item is EntityTreeViewItem item)
        {
            if (args.row % 2 == 0)
            {
                EditorGUI.DrawRect(args.rowRect, new Color(0.5f, 0.5f, 0.5f, 0.1f));
            }

            // Draw sprite
            if (item.sprite != null)
            {
                float spriteSize = ROW_HEIGHT - 4f;
                Rect spriteRect = new Rect(args.rowRect.x, args.rowRect.y + 2f, spriteSize, spriteSize);
                
                // Draw the sprite with proper UV coordinates
                GUI.DrawTextureWithTexCoords(
                    spriteRect, 
                    item.sprite.texture, 
                    new Rect(
                        item.sprite.textureRect.x / item.sprite.texture.width,
                        item.sprite.textureRect.y / item.sprite.texture.height,
                        item.sprite.textureRect.width / item.sprite.texture.width,
                        item.sprite.textureRect.height / item.sprite.texture.height
                    ),
                    true
                );
                
                // Adjust label position
                args.rowRect.x += spriteSize + 4f;
                args.rowRect.width -= spriteSize + 4f;
            }
        }
        base.RowGUI(args);
    }

    protected override void SingleClickedItem(int id)
    {
        if (searchResults.Count >= id && id > 0)
        {
            var item = searchResults[id - 1];
            EditorGUIUtility.PingObject(item.Prefab);
        }
    }

    protected override void DoubleClickedItem(int id)
    {
        if (searchResults.Count >= id && id > 0)
        {
            var item = searchResults[id - 1];
            Selection.activeObject = item.Prefab;
            EditorUtility.OpenPropertyEditor(item.Entity);
        }
    }
}

public class EntityTreeViewItem : TreeViewItem
{
    public GameObject prefab;
    public CMSEntityPfb entity;
    public Sprite sprite;
}

public class SearchResult
{
    public GameObject Prefab;
    public CMSEntityPfb Entity;
    public string DisplayName;
    public Sprite Sprite;
}