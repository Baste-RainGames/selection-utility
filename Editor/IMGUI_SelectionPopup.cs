// Copyright (c) 2019 Nementic Games GmbH.
// This file is subject to the MIT License. 
// See the LICENSE file in the package root folder for more information.
// Author: Chris Yarbrough

namespace Nementic.SelectionUtility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    ///     The IMGUI version of the popup which displays all selectable GameObjects.
    /// </summary>
    internal sealed class IMGUI_SelectionPopup : UnityEditor.PopupWindowContent
    {
        public IMGUI_SelectionPopup(List<GameObject> options)
        {
            this.options = options;
        }

        private List<GameObject> options;

        private float buttonAndIconsWidth;
        private float buttonWidth;
        private float iconWidth;

        private Styles styles;
        private List<Component> components = new List<Component>(8);
        private Vector2 scroll;
        private Rect contentRect;

        private Dictionary<GameObject, Texture2D[]> iconLookup = new Dictionary<GameObject, Texture2D[]>(64);

        /// <summary>
        ///     A lookup to avoid showing the same icon multiple times.
        /// </summary>
        private HashSet<Texture2D> displayedIcons = new HashSet<Texture2D>();

        /// <summary>
        ///     These types are not displayed as component icons in the popup window.
        /// </summary>
        private static HashSet<Type> ignoredIconTypes = new HashSet<Type>()
        {
            typeof(Transform),
            typeof(MeshFilter)
        };

        private class Styles
        {
            public Styles()
            {
                prefabLabel = new GUIStyle("PR PrefabLabel");
                prefabLabel.alignment = TextAnchor.MiddleLeft;

                label = new GUIStyle(EditorStyles.label);
                label.alignment = TextAnchor.MiddleLeft;
                var p = label.padding;
                p.top -= 1;
                p.left -= 1;
                label.padding = p;
            }

            public GUIStyle LabelStyle(GameObject target)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(target))
                    return prefabLabel;
                else
                    return label;
            }

            public GUIStyle label;

            private static GUIStyle prefabLabel;

            public Vector2 iconSize = new Vector2(16, 16);

            private static readonly Color splitterDark = new Color(0.12f, 0.12f, 0.12f, 1.333f);
            private static readonly Color splitterLight = new Color(0.6f, 0.6f, 0.6f, 1.333f);

            public Color splitterColor { get { return EditorGUIUtility.isProSkin ? splitterDark : splitterLight; } }

            private static readonly Color hoverDark = new Color(0.1f, 0.1f, 0.1f, 0.4f);
            private static readonly Color hoverLight = new Color(0.5f, 0.5f, 0.5f, 0.4f);

            public Color rowHoverColor { get { return EditorGUIUtility.isProSkin ? hoverDark : hoverLight; } }

            private GUIContent tempContent = new GUIContent();

            public GUIContent TempContent(string text, Texture2D image)
            {
                tempContent.text = text;
                tempContent.image = image;
                return tempContent;
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();
            styles = new Styles();
            editorWindow.wantsMouseMove = true;
            PrecalculateRequiredSizes();
        }

        private void PrecalculateRequiredSizes()
        {
            buttonWidth = 0;

            for (int i = 0; i < options.Count; i++)
            {
                float width = options[i] != null ? styles.label.CalcSize(new GUIContent(options[i].name)).x : 0f;

                // If a GameObject name is excessively long, clip it.
                int maxWidth = 300;
                if (width > maxWidth)
                    width = maxWidth;

                if (width > this.buttonWidth)
                    this.buttonWidth = width;
            }

            // After button, add small space.
            this.buttonWidth += EditorGUIUtility.standardVerticalSpacing;

            iconWidth = 0;

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

                float iconWidth = (18 * displayedIcons.Count);

                if (iconWidth > this.iconWidth)
                    this.iconWidth = iconWidth;
            }

            this.buttonAndIconsWidth = this.buttonWidth + this.iconWidth + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect rect)
        {
            for (int i = options.Count - 1; i >= 0; i--)
                if (options[i] == null)
                    options.RemoveAt(i);

            if (options.Count == 0)
            {
                ClosePopup();
                return;
            }

            Event current = Event.current;

            scroll = GUI.BeginScrollView(rect, scroll, contentRect, GUIStyle.none, GUI.skin.verticalScrollbar);

            rect.height = EditorGUIUtility.singleLineHeight + 2;
            rect.y += EditorGUIUtility.standardVerticalSpacing;
            rect.y -= 1;
            rect.xMin += 2;
            rect.xMax -= 2;

            using (new EditorGUIUtility.IconSizeScope(styles.iconSize))
            {
                for (int i = 0; i < options.Count; i++)
                {
                    DrawRow(rect, current, options[i]);
                    rect.y += RowHeight();

                    if (i < options.Count - 1)
                        DrawSplitter(rect);
                }
            }

            GUI.EndScrollView();

            if (current.type == EventType.MouseMove)
                editorWindow.Repaint();
        }

        private void ClosePopup()
        {
            if (editorWindow)
                editorWindow.Close();
            GUIUtility.ExitGUI();
        }

        private void DrawSplitter(Rect rect)
        {
            rect.height = 1;
            rect.y -= 1;
            rect.xMin = 0f;
            rect.width += 4f;
            EditorGUI.DrawRect(rect, styles.splitterColor);
        }

        private void DrawRow(Rect rect, Event current, GameObject target)
        {
            if (rect.Contains(current.mousePosition) && current.type != EventType.MouseDrag)
            {
                Rect background = rect;
                background.xMin -= 1;
                background.xMax += 1;
                background.yMax += 1;
                EditorGUI.DrawRect(background, styles.rowHoverColor);
            }

            Rect originalRect = rect;
            var icon = AssetPreview.GetMiniThumbnail(target);
            Rect iconRect = rect;
            iconRect.width = 20;

            EditorGUI.LabelField(iconRect, styles.TempContent(null, icon));

            rect.x = iconRect.xMax;
            rect.width = buttonWidth;

            var nameContent = styles.TempContent(target != null ? target.name : "Null", null);
            EditorGUI.LabelField(rect, nameContent, styles.LabelStyle(target));

            if (current.type == EventType.MouseDown &&
                originalRect.Contains(current.mousePosition))
            {
                if (current.shift || current.control)
                {
                    ToggleSelectedObjectAdditive(target);
                }
                else
                {
                    ToggleSelectedObject(target);
                }

                ClosePopup();
            }

            if (target == null)
                return;

            Rect componentIconRect = rect;
            componentIconRect.x = rect.xMax;
            componentIconRect.width = rect.height;

            var icons = iconLookup[target];
            for (int i = 0; i < icons.Length; i++)
            {
                EditorGUI.LabelField(componentIconRect, styles.TempContent(null, icons[i]));
                componentIconRect.x = componentIconRect.xMax;
            }
        }

        private void ToggleSelectedObject(UnityEngine.Object selectedObject)
        {
            if (Selection.activeObject == selectedObject)
                Selection.activeObject = null;
            else
                Selection.activeObject = selectedObject;
        }

        private void ToggleSelectedObjectAdditive(UnityEngine.Object selectedObject)
        {
            var selectedObjects = Selection.objects;

            if (selectedObjects.Contains(selectedObject))
                ArrayUtility.Remove(ref selectedObjects, selectedObject);
            else
                ArrayUtility.Add(ref selectedObjects, selectedObject);

            Selection.objects = selectedObjects;
        }

        private float RowHeight()
        {
            return EditorGUIUtility.singleLineHeight + 2f + EditorGUIUtility.standardVerticalSpacing;
        }

        public override Vector2 GetWindowSize()
        {
            float height = RowHeight() * options.Count;
            height += EditorGUIUtility.standardVerticalSpacing;

            float preIconWidth = 22f;
            var size = new Vector2(preIconWidth + buttonAndIconsWidth, height - 1);

            contentRect = new Rect(Vector2.zero, size);
            int maxHeight = Mathf.Min(Screen.currentResolution.height, 800);
            if (height > maxHeight)
            {
                size.y = maxHeight;
                size.x += 14; // Extra size to fit vertical scroll without clipping icons.
            }

            return size;
        }
    }
}
