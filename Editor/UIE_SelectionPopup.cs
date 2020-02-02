// Copyright (c) 2019 Nementic Games GmbH.
// This file is subject to the MIT License. 
// See the LICENSE file in the package root folder for more information.
// Author: Chris Yarbrough

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
    internal sealed class UIE_SelectionPopup : UIE_PopupWindowContent
    {
        private readonly List<GameObject> options;
        private float buttonWidth;
        private float buttonAndIconsWidth;
        private HashSet<Texture2D> displayedIcons = new HashSet<Texture2D>();
        private List<Component> components = new List<Component>(8);

        /// <summary>
        ///     These types are not displayed as component icons in the popup window.
        /// </summary>
        private static HashSet<Type> ignoredIconTypes = new HashSet<Type>()
        {
            typeof(Transform),
            typeof(MeshFilter)
        };

        private Dictionary<GameObject, Texture2D[]> iconLookup = new Dictionary<GameObject, Texture2D[]>(64);

        private float rowHeight => 21;

        private List<GameObject> filteredOptions;
        private ListView list;

        public UIE_SelectionPopup(List<GameObject> options)
        {
            this.options = options;
        }

        private void PrecalculateRequiredSizes()
        {
            buttonWidth = 0;

            for (int i = 0; i < options.Count; i++)
            {
                // TODO: This may no longer be correct for uielements.
                float width = options[i] != null ? GUI.skin.label.CalcSize(new GUIContent(options[i].name)).x : 0f;

                // If a GameObject name is excessively long, clip it.
                int maxWidth = 300;
                if (width > maxWidth)
                    width = maxWidth;

                if (width > buttonWidth)
                    buttonWidth = width;
            }

            // After button, add small space.
            buttonWidth += EditorGUIUtility.standardVerticalSpacing;

            float iconWidth = 0;

            for (int i = 0; i < options.Count; i++)
            {
                if (options[i] == null)
                    continue;

                options[i].GetComponents<Component>(components);

                displayedIcons.Clear();

                for (int j = 0; j < components.Count; j++)
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

                iconLookup.Add(options[i], displayedIcons.ToArray());

                float iconWidth2 = (18 * displayedIcons.Count);

                if (iconWidth2 > iconWidth)
                    iconWidth = iconWidth2;
            }

            this.buttonAndIconsWidth = buttonWidth + iconWidth + EditorGUIUtility.standardVerticalSpacing;
        }

        public override Vector2 GetWindowSize()
        {
            int rows = 2 + options.Count;
            float height = rowHeight * options.Count;
            height += EditorGUIUtility.standardVerticalSpacing;
            height += 21; // Toolbar height.

            float preIconWidth = 22f;
            var size = new Vector2(preIconWidth + buttonAndIconsWidth, height - 2);

            int maxHeight = Mathf.Min(Screen.currentResolution.height, 800);
            if (height > maxHeight)
            {
                size.y = maxHeight;
                size.x += 14; // Extra size to fit vertical scroll without clipping icons.
            }

            return size;
        }

        public override void BuildContent(VisualElement root)
        {
            PrecalculateRequiredSizes();

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.nementic.selection-utility/Editor/UIE_SelectionPopup.uss");
            root.styleSheets.Add(styleSheet);

            var toolbar = new Toolbar();
            var searchField = new ToolbarSearchField();
            toolbar.Add(searchField);
            root.Add(toolbar);

            searchField.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                // To polish the search experience:
                // - Ignore multiple spaces in a row by collapsing them down to a single one.
                // - Remove white space at the start and end of the search string.
                // - Ignore letter case.
                string value = Regex.Replace(evt.newValue.Trim(), @"[ ]+", " ");
                filteredOptions = options.Where(x => x.name.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0).ToList();
                list.itemsSource = filteredOptions;
                list.Refresh();

                //var field = typeof(SceneView).GetField("m_SearchFilter", BindingFlags.NonPublic | BindingFlags.Instance);
                //field.SetValue(SceneView.lastActiveSceneView, value);
            });

            filteredOptions = options.ToList();

            list = new ListView(filteredOptions, 21, MakeItem, BindItem);

            VisualElement MakeItem()
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

            void BindItem(VisualElement ve, int index)
            {
                GameObject target = filteredOptions[index];

                var label = ve.Q<Label>();
                label.text = target != null ? target.name : "Null";
                label.style.width = buttonWidth;

                if (PrefabUtility.IsPartOfAnyPrefab(target))
                    label.AddToClassList("prefab-label");
                else
                    label.RemoveFromClassList("prefab-label");

                var iconImage = AssetPreview.GetMiniThumbnail(target);
                var iconElement = ve.Q("Icon");
                iconElement.style.backgroundImage = iconImage;
                iconElement.style.height = iconElement.style.width = 16;

                var container = ve.Q("ComponentIconContainer");
                container.Clear();

                if (iconLookup.ContainsKey(target))
                {
                    Texture2D[] icons = iconLookup[target];
                    for (int i = 0; i < icons.Length; i++)
                    {
                        var componentIcon = new VisualElement();
                        componentIcon.style.backgroundImage = icons[i];
                        componentIcon.style.width = componentIcon.style.height = 16;
                        container.Add(componentIcon);
                    }
                }
            }

            list.onSelectionChanged += OnItemChosen;
            list.selectionType = SelectionType.Multiple;

            list.Refresh();
            root.Add(list);
        }

        private void OnItemChosen(List<object> obj)
        {
            var unityObjects = new List<UnityEngine.Object>();
            foreach (var o in obj)
                unityObjects.Add((UnityEngine.Object)o);

            Selection.objects = unityObjects.ToArray();
        }
    }
}
