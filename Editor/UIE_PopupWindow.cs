// Copyright (c) 2019 Nementic Games GmbH.
// This file is subject to the MIT License. 
// See the LICENSE file in the package root folder for more information.
// Author: Chris Yarbrough

namespace Nementic.SelectionUtility
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// A popup-styled editor window which can be shown by providing
    /// an activator rect. This replicates the core functionality of
    /// <see cref="UnityEditor.PopupWindow"/>. Not to be confused with
    /// the unluckily named <see cref="UnityEngine.UIElements.PopupWindow"/>, which 
    /// only describes an element with similar styling.
    /// </summary>
    internal class UIE_PopupWindow : EditorWindow
    {
        public void Show(Rect activatorRect, UIE_PopupWindowContent content)
        {
            base.hideFlags = HideFlags.DontSave;
            base.wantsMouseMove = true;

            content.BuildContent(rootVisualElement);

            activatorRect = GUIUtility.GUIToScreenRect(activatorRect);
            base.ShowAsDropDown(activatorRect, content.GetWindowSize());
        }

        private void OnLostFocus()
        {
            base.Close();
        }
    }

    internal abstract class UIE_PopupWindowContent
    {
        /// <summary>
        /// Called before the popup window will be shown.
        /// </summary>
        public virtual void BuildContent(VisualElement root)
        {
        }

        /// <summary>
        /// Queried after <see cref="BuildContent(VisualElement)"/>
        /// and just before the window will be shown.
        /// </summary>
        public virtual Vector2 GetWindowSize()
        {
            return new Vector2(200, 400);
        }
    }
}
