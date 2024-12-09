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