// Copyright (c) 2019 Nementic Games GmbH.
// This file is subject to the MIT License. 
// See the LICENSE file in the package root folder for more information.
// Author: Chris Yarbrough

namespace Nementic.SelectionUtility
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    ///     Provides access to tool settings stored on the local machine.
    /// </summary>
    internal static class UserPreferences
    {
        /// <summary>
        /// True if the tool should be available when clicking in the SceneView.
        /// </summary>
        public static readonly Pref<bool> Enabled = new BoolPref("Nementic/SelectionUtility/Enabled", true);

        /// <summary>
        /// The duration in milliseconds after which a held down mouse button will not trigger the context popup.
        /// </summary>
        public static readonly Pref<int> ClickTimeout = new IntPref("Nementic/SelectionUtility/ClickTimeout",
            minValue: 80, maxValue: 3000, defaultValue: 300);

        [SettingsProvider]
        public static SettingsProvider CreateSettings()
        {
            GUIContent enabledLabel = new GUIContent("Enabled",
                "Checked if the tool should be available when clicking in the SceneView.");

            GUIContent timoutLabel = new GUIContent("Click Timeout",
                "The duration in milliseconds after which a held mouse button will not trigger the context popup.");

            return new SettingsProvider("Nementic/Selection Utility", SettingsScope.User)
            {
                guiHandler = (searchContext) =>
                {
                    Enabled.DrawProperty(enabledLabel, SceneViewGuiHandler.SetEnabled);
                    ClickTimeout.DrawProperty(timoutLabel);
                },
                keywords = new HashSet<string>(new[] { "Nementic", "Selection", "Utility" })
            };
        }

        public abstract class Pref<T>
        {
            protected readonly string key;
            protected readonly T defaultValue;

            private T cachedValue = default;
            private bool cacheInitialized = false;

            public Pref(string key, T defaultValue = default)
            {
                this.key = key;
                this.defaultValue = defaultValue;
            }

            public virtual T Value
            {
                get
                {
                    if (cacheInitialized == false)
                    {
                        cachedValue = ReadValue();
                        cacheInitialized = true;
                    }
                    return cachedValue;
                }
                set
                {
                    WriteValue(value);
                    cachedValue = value;
                    cacheInitialized = true;
                }
            }

            public static implicit operator T(Pref<T> pref)
            {
                return pref.Value;
            }

            public void DrawProperty(GUIContent label, Action<T> onChanged = null)
            {
                EditorGUI.BeginChangeCheck();
                T newValue = DrawProperty(label, Value);
                if (EditorGUI.EndChangeCheck())
                {
                    Value = newValue;
                    onChanged?.Invoke(newValue);
                }
            }

            protected abstract T DrawProperty(GUIContent label, T value);

            protected abstract T ReadValue();
            protected abstract void WriteValue(T value);
        }

        private class BoolPref : Pref<bool>
        {
            public BoolPref(string key, bool defaultValue = false) : base(key, defaultValue)
            {
            }

            protected override bool DrawProperty(GUIContent label, bool value)
            {
                return EditorGUILayout.Toggle(label, value);
            }

            protected override bool ReadValue() => EditorPrefs.GetBool(key, defaultValue);
            protected override void WriteValue(bool value) => EditorPrefs.SetBool(key, value);
        }

        private class IntPref : Pref<int>
        {
            private readonly int minValue;
            private readonly int maxValue;

            public IntPref(string key, int defaultValue = 0) : base(key, defaultValue)
            {
            }

            public IntPref(string key, int minValue, int maxValue, int defaultValue = 0) : base(key, defaultValue)
            {
                this.minValue = minValue;
                this.maxValue = maxValue;
            }

            protected override int DrawProperty(GUIContent label, int value)
            {
                value = EditorGUILayout.IntField(label, value);
                return Mathf.Clamp(value, minValue, maxValue);
            }

            public override int Value
            {
                get => base.Value;
                set => base.Value = Mathf.Clamp(value, minValue, maxValue);
            }

            protected override int ReadValue() => EditorPrefs.GetInt(key, defaultValue);
            protected override void WriteValue(int value) => EditorPrefs.SetInt(key, value);
        }
    }
}
