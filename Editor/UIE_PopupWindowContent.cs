// Copyright (c) 2019 Nementic Games GmbH.
// This file is subject to the MIT License. 
// See the LICENSE file in the package root folder for more information.
// Author: Chris Yarbrough

#if UNITY_2019_3_OR_NEWER

namespace Nementic.SelectionUtility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    ///     The UIElements version of the popup which displays all selectable GameObjects.
    /// </summary>
    [Serializable]
    internal sealed class UIE_PopupWindowContent
    {
        private ListView list;
        private List<GameObject> options;
        private int rowHeight => 21;

        /// Because view data persistence is not implemented for the ToolbarSearchField
        /// this member is serialized with the class instance.
        private string searchString;

        private float labelWidth;
        private float rowWidth;

        /// These types are not displayed as component icons in the popup window as they would clutter the view.
        private static readonly HashSet<Type> ignoredIconTypes = new HashSet<Type>()
        {
            typeof(Transform),
            typeof(MeshFilter)
        };

        private Dictionary<GameObject, HashSet<Texture2D>> iconCache;

        public UIE_PopupWindowContent(List<GameObject> options)
        {
            this.options = options;
        }

        public void Build(VisualElement root)
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Packages/com.nementic.selection-utility/Editor/UIE_SelectionPopup.uss");
            root.styleSheets.Add(styleSheet);

            var toolbar = new Toolbar();
            var searchField = new ToolbarSearchField();
            searchField.viewDataKey = "ToolbarSearchFieldData";
            searchField.value = searchString;
            searchField.RegisterCallback<ChangeEvent<string>>(OnSearchChanged);
            toolbar.Add(searchField);
            root.Add(toolbar);

            list = new ListView
            {
                itemHeight = rowHeight,
                makeItem = MakeItem,
                bindItem = BindItem
            };
            list.onSelectionChanged += OnListSelectionChanged;
            list.selectionType = SelectionType.Multiple;
            list.viewDataKey = "ListViewDataKey";
            root.Add(list);

            BuildIconCache();

            RefreshListWithFilter(searchField.value);
        }

        public Vector2 GetWindowSize()
        {
            CalculateLabelAndRowWidths();

            int rows = 2 + options.Count;
            float height = rowHeight * options.Count;
            height += EditorGUIUtility.standardVerticalSpacing;
            height += 21; // Toolbar height.

            float preIconWidth = 22f;
            var size = new Vector2(preIconWidth + rowWidth, height - 2);

            int maxHeight = Mathf.Min(Screen.currentResolution.height, 800);
            if (height > maxHeight)
            {
                size.y = maxHeight;
                size.x += 14; // Extra size to fit vertical scroll bar without clipping icons.
            }

            return size;
        }

        private void CalculateLabelAndRowWidths()
        {
            labelWidth = 0;

            for (int i = 0; i < options.Count; i++)
            {
                // TODO: This may no longer be correct for uielements.
                float width = options[i] != null ? GUI.skin.label.CalcSize(new GUIContent(options[i].name)).x : 0f;

                // If a GameObject name is excessively long, clip it.
                int maxWidth = 300;
                if (width > maxWidth)
                    width = maxWidth;

                if (width > labelWidth)
                    labelWidth = width;
            }

            // After button, add small space.
            labelWidth += EditorGUIUtility.standardVerticalSpacing;

            BuildIconCache();
            float iconWidth = 0;

            for (int i = 0; i < options.Count; i++)
            {
                if (options[i] == null)
                    continue;

                float iconWidthTmp = (18 * iconCache[options[i]].Count);

                if (iconWidthTmp > iconWidth)
                    iconWidth = iconWidthTmp;
            }

            this.rowWidth = labelWidth + iconWidth + EditorGUIUtility.standardVerticalSpacing;
        }

        private void BuildIconCache()
        {
            if (iconCache != null)
                return;

            iconCache = new Dictionary<GameObject, HashSet<Texture2D>>(32);

            for (int i = 0; i < options.Count; i++)
            {
                if (options[i] == null)
                    continue;

                var components = options[i].GetComponents<Component>();

                var displayedIcons = new HashSet<Texture2D>();

                for (int j = 0; j < components.Length; j++)
                {
                    var type = components[j].GetType();

                    if (ignoredIconTypes.Contains(type))
                        continue;

                    // This returns an icon for many builtin components such as Transform and Collider.
                    Texture2D componentIcon = AssetPreview.GetMiniThumbnail(components[j]);

                    if (displayedIcons.Contains(componentIcon))
                        continue;

                    displayedIcons.Add(componentIcon);
                }

                iconCache.Add(options[i], displayedIcons);
            }
        }

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            this.searchString = evt.newValue ?? string.Empty;

            // To polish the search experience:
            // - Ignore multiple spaces in a row by collapsing them down to a single one.
            // - Remove white space at the start and end of the search string.
            // - Ignore letter case.
            string value = Regex.Replace(searchString.Trim(), @"[ ]+", " ");
            RefreshListWithFilter(value);
        }

        private void RefreshListWithFilter(string searchString)
        {
            if (searchString == null)
                searchString = string.Empty;

            var filteredOptions = options.Where(
                x => x.name.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0).ToList();

            list.itemsSource = filteredOptions;
            list.Refresh();
        }

        private VisualElement MakeItem()
        {
            var row = new VisualElement() { name = "RowTemplate" };
            var icon = new VisualElement() { name = "Icon" };
            var label = new Label();
            var container = new VisualElement() { name = "ComponentIconContainer" };
            var separator = new VisualElement();
            separator.AddToClassList("separator");

            row.Add(icon);
            row.Add(icon);
            row.Add(label);
            row.Add(container);
            row.Add(separator);

            return row;
        }

        private void BindItem(VisualElement element, int index)
        {
            GameObject target = (GameObject)list.itemsSource[index];

            var label = element.Q<Label>();
            label.text = target != null ? target.name : "Null";
            label.style.width = labelWidth;

            // Indicate prefabs with a blue label.
            if (PrefabUtility.IsPartOfAnyPrefab(target))
                label.AddToClassList("prefab-label");
            else
                label.RemoveFromClassList("prefab-label");

            Texture2D iconTexture = AssetPreview.GetMiniThumbnail(target);
            var iconElement = element.Q("Icon");
            iconElement.style.backgroundImage = iconTexture;
            iconElement.style.height = iconElement.style.width = 16;

            var container = element.Q("ComponentIconContainer");
            container.Clear();

            if (iconCache.ContainsKey(target))
            {
                foreach (var icon in iconCache[target])
                {
                    var componentIcon = new VisualElement();
                    componentIcon.style.backgroundImage = icon;
                    componentIcon.style.width = componentIcon.style.height = 16;
                    container.Add(componentIcon);
                }
            }
        }

        private void OnListSelectionChanged(List<object> listItems)
        {
            Selection.objects = listItems.ConvertAll(x => (UnityEngine.Object)x).ToArray();
        }
    }
}

#endif