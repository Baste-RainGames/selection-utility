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

## [1.1.0] - 2020-02-02
### Added
- Use UIElements for Unity version 2019.3 going forward.
- Toolbar search field at the top of the popup when using the UIElements version.

## [1.1.1] - 2020-02-05
### Fixed
- Project not compiling when added to the packages folder of a project in 2018.4 or earlier.
- Disabled the utility when the editor is compiling, as that causes glitchy behaviour.

## [1.1.2] - 2020-02-05
### Fixed
- Handle domain reload by serializing popup state.
- Revert fix from v1.1.1 that blocks the utility while recompiling, since this is now supported.

## [1.1.3] - 2020-02-20
### Fixed
- Errors when the objects shown in the popup was deleted by scripts or by scene loads in the IMGUI (2018.4) version