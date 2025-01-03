using System;
using System.Collections.Generic;
using System.Linq;
using Venwin.DataStructures;

#nullable enable

namespace Venwin.Grid.Pathfinding
{
    /// <summary>
    /// AStar pathfinding that takes into consideration the three dimensions.
    /// </summary>
    public class AStarGrid3D
    {
        /// <summary>
        /// Finds a path between two <see cref="GridCell"/>s.
        /// </summary>
        /// <param name="start">Starting cell.</param>
        /// <param name="goal">End cell.</param>
        /// <param name="considerIsNavigatable">If set to true cells are not considered part of the pathfinding unless <see cref="GridCell.IsNavigatable"/> is set to true on a Grid Cell's neigbors.</param>
        /// <returns>The path of <see cref="GridCell"/>'s pointing to the previous cell it came from. If no path is found returns null.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="considerIsNavigatable"/> is true and the <paramref name="start"/> or <paramref name="goal"/> cells have <see cref="GridCell.IsNavigatable"/> set to false. </exception>
        public static Dictionary<GridCell, GridCell>? FindPath(GridCell start, GridCell goal, bool considerIsNavigatable = false)
        {
            if (considerIsNavigatable)
            {
                if (!start.IsNavigatable) { throw new ArgumentException("Start Cell must be navigatable"); }
                if (!goal.IsNavigatable) { throw new ArgumentException("End Cell must be navigatable"); }
            }
            var frontier = new PriorityQueue<GridCell>();
            frontier.Enqueue(start, 0);

            var cameFrom = new Dictionary<GridCell, GridCell>();
            var costSoFar = new Dictionary<GridCell, int>();

            cameFrom[start] = null;
            costSoFar[start] = 0;

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current.Equals(goal))
                {
                    return cameFrom;
                }

                IReadOnlyCollection<GridCell> nextCellsToConsider = considerIsNavigatable ? current.Neighbors.Where(cell => cell.IsNavigatable).ToList()
                                                                                          : current.Neighbors;

                foreach (var next in nextCellsToConsider)
                {
                    int newCost = costSoFar[current] + next.GetCostToEnter(current);
                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        int priority = newCost + Heuristic(goal, next, current);
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a path between two cells from the starting cell to the goal cell.
        /// </summary>
        /// <param name="start">Starting cell.</param>
        /// <param name="goal">Ending cell.</param>
        /// <param name="considerIsNavigatable">If set to true cells are not considered part of the pathfinding unless <see cref="GridCell.IsNavigatable"/> is set to true on a Grid Cell's neigbors.</param>
        /// <returns>A list of the cells from start to end. If none is found, returns null instead.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="considerIsNavigatable"/> is true and the <paramref name="start"/> or <paramref name="goal"/> cells have <see cref="GridCell.IsNavigatable"/> set to false. </exception>
        public static List<GridCell>? GetPath(GridCell start, GridCell goal, bool considerIsNavigatable = false)
        {
            return GetPathFromFoundPath(FindPath(start, goal, considerIsNavigatable), start, goal);
        }

        private static List<GridCell>? GetPathFromFoundPath(Dictionary<GridCell, GridCell>? cameFrom, GridCell start, GridCell goal)
        {
            if (cameFrom == null) { return null; }

            var path = new List<GridCell>();
            var current = goal;

            while (!current.Equals(start))
            {
                path.Add(current);
                current = cameFrom[current];
            }

            path.Add(start);
            path.Reverse(); // Since we added the goal first, reverse the path to start from the beginning
            return path;
        }


        /// <summary>
        /// The valuation logic on what makes a path more attractive or less attractive to pursue in comparison to others.
        /// </summary>
        /// <param name="goal">The end goal cell.</param>
        /// <param name="b">The cell we're evaluating.</param>
        /// <param name="currentCell">The cell we're currently on.</param>
        /// <returns>An integer value that is used to prioritize certain pathing. Lower is more important.</returns>
        private static int Heuristic(GridCell goal, GridCell b, GridCell currentCell)
        {
            // Manhattan distance on a square grid
            int distance = Math.Abs(goal.GridCoordinates.x - b.GridCoordinates.x) +
                           Math.Abs(goal.GridCoordinates.z - b.GridCoordinates.z) +
                           Math.Abs(goal.GridCoordinates.y - b.GridCoordinates.y);
             
            if(goal.GridCoordinates.y != b.GridCoordinates.y)
            {
                if(b.CellRampDetails != null
                    // Are we getting closer to the goal y coordinate if we take the ramp?
                    && (IsCloserThan(b.GridCoordinates.y + b.CellRampDetails.YIndexUpperRamp, currentCell.GridCoordinates.y, goal.GridCoordinates.y) ||
                        IsCloserThan(currentCell.GridCoordinates.y - b.CellRampDetails.YIndexLowerRamp, currentCell.GridCoordinates.y, goal.GridCoordinates.y))
                   )
                {
                    // Prioritize this path a bit more
                    distance -= 2;
                }
            }
            
            return distance;
        }

        /// <summary>
        /// Evaluates if the first value closer to the target than another value.
        /// </summary>
        /// <param name="value1">First value</param>
        /// <param name="value2">Second value</param>
        /// <param name="target">Target value</param>
        /// <returns>True if first value is closer, else false.</returns>
        private static bool IsCloserThan(int value1, int value2, int target)
        {
            return Math.Abs(value1 - target) < Math.Abs(value2 - target);
        }
    }
}
