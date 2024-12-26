## [Version 1.3.3] - 2024-12-26

### Added
- Object Pooling Manager now has a callback creation option.
	- This was already available in the ObjectPooling type, but the "Out of the Box" manager didn't provide a quick way to use it.

### Fixed
- Object Pooling Manager's FindPool method now safely does a cast to validate the name and type expectations match. If a failed cast occurs it prints an error message but safely fails.

## [Version 1.3.2] - 2024-12-25

### Added
- Object Pooling can now have a callback action on any new objects that are created when all pooled objects are unavailable.

## [Version 1.3.1] - 2024-12-25

### Added
- Added a ReadOnlyQueue collection type.
- Object Pooling now has a readonly Pool Queue that can be used to loop over pool objects. 

## [Version 1.3] - 2024-12-25

### Changed
- Object Pooling is now generic that works with MonoBehaviors

### Fixed
- Object Pooling now can actually reuse a pool across the same object types.

## [Version 1.2.4] - 2024-12-22

### Fixed
- Added virtual to `GridObject`'s `Awake()` that was missing previously.
- `ParentChildUtilities.SetObjectAndChildrenToLayer` now works properly.
	- It is an API change, however, considering it never worked in the first place so it'll be considered a patch.	

## [Version 1.2.3] - 2024-12-16

### Changed
- Grid Pathfinding algorithms now have optional bool flag that can consider a Cell's navigatability when finding a path.
	- Non breaking change. Set to false by default.

## [Version 1.2.2] - 2024-12-15

### Added
- Grid Cells now can detect just the immediate game objects that components are attached to, not just the root object.
- JsonFileUtil for exporting and loading in JSON files.
- Cursor Utilities now has short hand for checking if cursor is over a UI element.

## [Version 1.2.1] - 2024-12-09

### Fixed
- `ObjectPoolMonoBehaviorManager` registers instance on awake instead of start now.

## [Version 1.2.0] - 2024-10-23

### Added
- Generic Grid Cell
	- Custom grid cell support has now been added to grids. This allows implementors to add custom grid cell logic.
	- This does introduce breaking changes to current Grids.
- Grid Cell detection
    - Grid Cells can now check in a box above them for any objects. Useful if GridObjects are not directly assigned to a cell.
- Grid Gizmo Drawer
	- Takes in a grid and will draw gizmos that can help the debug process. Currently only draws the GridCell Physics.Overlap box.
- Find Component in Parent or Children
	- Method added to ParentChildUtilities

### Changed
- Grid Debugging
	- Two bool properties have been added to Grid: DrawDebug and PrintDebug, these are set to true and false by default.
	- This will allow debugging tools that work with a Grid out of the box to be turned on or off.
- Grid Cell CenterOfCellWorldSpace
	- Now a property, no longer a method. Breaking change.

## [Version 1.1.1] - 2024-10-20

### Fixed
- Ambiguity bug for Debug between System and Unity.

## [Version 1.1.0] - 2024-10-20

### Added
- Object Pool System
	- The Object Pool is specifically designed for Monobehaviors to be managed by the pools. Part of the advantage of the solution is that the pool can make objects aware of it so that there is fast lookup.

## [Version 1.0.1] - 2024-10-19

### Added
- Square Grid class
	- Same behavior as Grid but secures an implementation designed solely for square grid cells.