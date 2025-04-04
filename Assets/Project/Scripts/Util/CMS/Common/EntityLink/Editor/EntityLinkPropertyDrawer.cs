using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Game
{
    [CustomPropertyDrawer(typeof(EntityLink))]
    public class EntityLinkPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Получаем свойство Id
            SerializedProperty idProp = property.FindPropertyRelative("Id");

            // Отрисовываем лейбл
            Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, label.text, idProp.stringValue);

            // Кнопка для выбора типа
            Rect buttonRect = new Rect(
                position.x,
                position.y + EditorGUIUtility.singleLineHeight + 2,
                position.width,
                EditorGUIUtility.singleLineHeight
            );

            if (GUI.Button(buttonRect, "Select Type"))
            {
                // Открываем окно выбора типа
                TypeSelectorWindow.Open(selectedType =>
                {
                    idProp.stringValue = selectedType.FullName;
                    idProp.serializedObject.ApplyModifiedProperties();
                });
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Высота свойства: лейбл + кнопка + отступы
            return EditorGUIUtility.singleLineHeight * 2 + 2;
        }
    }

    public class TypeSelectorWindow : EditorWindow
    {
        private string _searchQuery = "";
        private List<Type> _filteredTypes;
        private Action<Type> _onTypeSelected;
        private Vector2 _scrollPosition;

        public static void Open(Action<Type> onTypeSelected)
        {
            var window = GetWindow<TypeSelectorWindow>(true, "Select CMSEntity Type");
            window._searchQuery = "";
            window._onTypeSelected = onTypeSelected;
            window._filteredTypes = GetCMSDerivedTypes();
            window.ShowUtility();
        }

        private void OnGUI()
        {
            // Поле поиска
            EditorGUILayout.Space();
            string newSearch = EditorGUILayout.TextField("Search", _searchQuery);
            if (newSearch != _searchQuery)
            {
                _searchQuery = newSearch;
                UpdateFilteredTypes();
            }

            // Список типов с прокруткой
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            foreach (Type type in _filteredTypes)
            {
                if (GUILayout.Button(type.FullName))
                {
                    _onTypeSelected?.Invoke(type);
                    Close();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void UpdateFilteredTypes()
        {
            var allTypes = GetCMSDerivedTypes();
            _filteredTypes = allTypes
                .Where(t => t.FullName.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        private static List<Type> GetCMSDerivedTypes()
        {
            return ReflectionUtil.FindAllSubslasses<CMSEntity>().ToList();
        }
    }
}
