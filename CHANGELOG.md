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

## [1.0.1] - 2019-07-22
### Fixed
- Click timer was not being used. Now it works again and cancels the popup when holding the mouse longer than 300ms.

## [1.0.2] - 2019-07-23
### Added
- Support for Unity 2018.3.
- User preferences setting to configure the click timeout.
- Additional tooltips in user preferences.

## [1.0.3] - 2019-07-24
### Added
- Support holding down the shift and control keys to add to or remove from the selection.
