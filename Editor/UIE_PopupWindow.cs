// Copyright (c) 2019 Nementic Games GmbH.
// This file is subject to the MIT License. 
// See the LICENSE file in the package root folder for more information.
// Author: Chris Yarbrough

#if UNITY_2019_3_OR_NEWER

namespace Nementic.SelectionUtility
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// A popup-styled editor window which can be shown by providing
    /// an activator rect. This replicates the core functionality of
    /// <see cref="UnityEditor.PopupWindow"/>. Not to be confused with
    /// the unluckily named <see cref="UnityEngine.UIElements.PopupWindow"/>, which
    /// only describes an element with similar styling.
    /// </summary>
    internal class UIE_PopupWindow : EditorWindow
    {
        private UIE_PopupWindowContent content;

        public void Show(Rect activatorRect, UIE_PopupWindowContent content)
        {
            base.hideFlags = HideFlags.DontSave;
            base.wantsMouseMove = true;

            this.content = content;

            Vector2 size = content.GetWindowSize();
            content.Build(rootVisualElement);

            activatorRect = GUIUtility.GUIToScreenRect(activatorRect);
            base.ShowAsDropDown(activatorRect, size);
        }

        private void OnEnable()
        {
            // Rebuild the content after domain reload.
            if (content != null)
                content.Build(rootVisualElement);
        }

        private void OnLostFocus()
        {
            base.Close();
        }
    }
}

#endif