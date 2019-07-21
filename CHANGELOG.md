# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2019-07-21
### Added
- Initial release of the tool.
- Context-click (right-click) in the SceneView to open a dropdown list of selectable GameObjects under the mouse cursor.
- The selectable GameObject list displays the object names along with their relevant component icons (the Transform and MeshFilter components are ignored).
- The list window has a maximum size and shows a vertical scrollbar if the number of items does not fit within the window.
- The window will attempt to fit within the screen size, shrinking if not enough pixels are available.
- The tool can be enabled or disabled in the user preferences menu under the path 'Nementic/Selection Utility'.