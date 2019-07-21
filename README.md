# Selection Utility

## Description
The Nementic Selection Utility is a small Unity tool which facilitates selecting GameObjects in the SceneView by displaying a context menu with all objects currently under the mouse cursor as a dropdown similar to how layers in common image editing software can be selected.

![Preview: Selection Utility in the SceneView](Documentation~/Preview.png)

## Setup
This tool has no dependencies other than the Unity editor itself and becomes available as soon as it is installed. The currently verified Unity Version is 2019.1.

## Usage
Right-click over GameObjects in the SceneView to show a dropdown of all objects that can be selected. The list is sorted by depth from front to back and displays icons for each component on a selectable GameObject. Left-click an item in the list to select it.