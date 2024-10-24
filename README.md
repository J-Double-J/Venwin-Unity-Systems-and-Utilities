# About

This is my personal collection of various Unity systems that I have created over time that I can import into my projects quickly.

I am treating this like you would any other package: using semantic versioning and changelogs. However, Unity prevents a package from being less than 1.0.0.
This package is far from a state I would comfortably describe as "stable". Use at your own risk if you find systems in here that are helpful.

## Semantic Versioning

This package will not respect "only non-code-breaking changes in minor releases". As the solution evolves I will likely be starting off with a fairly concrete
solution that overtime becomes more generic and broad as I need it. Breaking changes will be noted between versions as best as I can. Consider 1.x.x to be a beta release.

# Features

- Grid
	- A grid with square cells that can be quickly created
	- A-star Pathfinding
	- Logic for placing and hovering objects onto the grid.
	- Dynamic "Cell Details" which allows custom properties to be placed on a grid cell which can exhibit custom behavior
		- Could describe walkability of the cell
		- Perhaps fire is sitting on the cell which makes it dangerous?

- Object Pooling
	- Manager class that allows pooled objects be neatly organized based on their types.

- Various simple scripts
	- Circle Drawer
	- Priority Queue
	- Rotating Objects (used for things like windmills)

- Utilities
	- Adds methods that help with:
		- Clicking on objects in game
		- Layer Mask logic
		- Math
		- Logic regarding finding components in parent and child objects
		- Transform shortcuts
