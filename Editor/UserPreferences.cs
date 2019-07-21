// Copyright (c) 2019 Nementic Games GmbH.
// This file is subject to the MIT License. 
// See the LICENSE file in the package root folder for more information.
// Author: Chris Yarbrough

namespace Nementic.SelectionUtility
{
    using System.Collections.Generic;
    using UnityEditor;

    /// <summary>
    ///     Provides access to tool settings stored on the local machine.
    /// </summary>
    internal static class UserPreferences
    {
        public static bool Enabled
        {
            get
            {
                if (cacheSet == false)
                {
                    cachedValue = EditorPrefs.GetBool(enabledKey, defaultValue: true);
                    cacheSet = true;
                }
                return cachedValue;
            }
            set
            {
                EditorPrefs.SetBool(enabledKey, value);
                cachedValue = value;
                cacheSet = true;
            }
        }

        private static readonly string enabledKey = "Nementic/SelectionUtility/Enabled";
        private static bool cacheSet;
        private static bool cachedValue;

        [SettingsProvider]
        public static SettingsProvider CreateSettings()
        {
            return new SettingsProvider("Nementic/Selection Utility", SettingsScope.User)
            {
                guiHandler = (searchContext) =>
                {
                    EditorGUI.BeginChangeCheck();
                    bool value = EditorGUILayout.Toggle("Enabled", UserPreferences.Enabled);
                    if (EditorGUI.EndChangeCheck())
                    {
                        UserPreferences.Enabled = value;
                        SceneViewGuiHandler.SetEnabled(value);
                    }
                },
                keywords = new HashSet<string>(new[] { "Nementic", "Selection", "Utility" })
            };
        }
    }
}