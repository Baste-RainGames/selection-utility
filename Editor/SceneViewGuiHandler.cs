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
    using Stopwatch = System.Diagnostics.Stopwatch;

    /// <summary>
    ///     The main entry point of the tool which handles the SceneView callback.
    /// </summary>
    [InitializeOnLoad]
    public static class SceneViewGuiHandler
    {
        static SceneViewGuiHandler()
        {
            SetEnabled(UserPreferences.Enabled);
        }

        public static void SetEnabled(bool enabled)
        {
            SceneView.beforeSceneGui -= OnSceneGUI;

            if (enabled)
            {
                SceneView.beforeSceneGui += OnSceneGUI;

                // Lazy-initialize members to avoid allocating memory
                // if the tool has been disabled in user preferences.
                if (initialized == false)
                {
                    clickTimer = new Stopwatch();
                    controlIDHint = "NementicSelectionUtility".GetHashCode();
                    gameObjectBuffer = new List<GameObject>(8);
                    initialized = true;
                }
            }
        }

        private static bool initialized;
        private static Stopwatch clickTimer;
        private static int controlIDHint;
        private static List<GameObject> gameObjectBuffer;

        private static void OnSceneGUI(SceneView sceneView)
        {
            try
            {
                Event current = Event.current;
                int id = GUIUtility.GetControlID(controlIDHint, FocusType.Passive);

                // Right mouse button (context-blick).
                if (current.button == 1)
                {
                    HandleMouseButton(current, id);
                }
            }
            catch (Exception ex)
            {
                // When something goes wrong, we need to reset hotControl or else
                // the SceneView mouse cursor will stay stuck as a drag hand.
                GUIUtility.hotControl = 0;

                // When opening a UnityEditor.PopupWindow EditGUI throws an exception
                // to break out of the GUI loop. We want to ignore this but still log
                // all other unintended exceptions potentially caused by this tool.
                if (ex.GetType() != typeof(ExitGUIException))
                    Debug.LogException(ex);
            }
        }

        private static void HandleMouseButton(Event current, int id)
        {
            switch (current.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    OnMouseDown();
                    break;

                case EventType.MouseUp:
                    OnMouseUp(current);
                    break;
            }
        }

        private static void OnMouseDown()
        {
            clickTimer.Start();
        }

        private static void OnMouseUp(Event current)
        {
            long elapsedTime = ResetTimer();

            // Only show the selection menu if the click was short,
            // not if the user is holding to drag the SceneView camera.
            if (clickTimer.ElapsedMilliseconds < 300)
            {
                GUIUtility.hotControl = 0;
                current.Use();

                var gameObjects = GameObjectsUnderMouse(current.mousePosition);

                if (gameObjects.Count() > 0)
                {
                    Rect activatorRect = new Rect(current.mousePosition, Vector2.zero);
                    ShowSelectableGameObjectsPopup(activatorRect, gameObjects);
                    current.Use();
                }
            }
        }

        /// <summary>
        ///     Resets the timer and returns the elapsed time of the last run.
        /// </summary>
        private static long ResetTimer()
        {
            clickTimer.Stop();
            long elapsedTime = clickTimer.ElapsedMilliseconds;
            clickTimer.Reset();
            return elapsedTime;
        }

        /// <summary>
        ///     Returns all GameObjects under the provided mouse position.
        /// </summary>
        private static List<GameObject> GameObjectsUnderMouse(Vector2 mousePosition)
        {
            gameObjectBuffer.Clear();

            // Unity does not provide an API to retrieve all GameObjects under a ScenView position.
            // So, we pick objects one by one since Unity cycles through them.
            while (true)
            {
                var go = HandleUtility.PickGameObject(
                    mousePosition,
                    selectPrefabRoot: false,
                    ignore: gameObjectBuffer.ToArray());

                if (go == null)
                    break;

                int count = gameObjectBuffer.Count;
                if (count > 0 && go == gameObjectBuffer[count - 1])
                {
                    Debug.LogError($"Could not ignore game object '{go.name}' when picking.");
                    break;
                }

                gameObjectBuffer.Add(go);
            }
            return gameObjectBuffer;
        }

        private static void ShowSelectableGameObjectsPopup(Rect rect, List<GameObject> options)
        {
            var content = new SelectionPopup(options);
            PopupWindow.Show(rect, content);
        }
    }
}