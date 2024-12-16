using System;
using System.Collections.Generic;
using System.Linq;
using Venwin.DataStructures;

namespace Venwin.Grid.Pathfinding
{

    public class DijkstraAlgorithmGrid
    {
        /// <summary>
        /// Finds all the grid cells within the specified maximum cost from the starting cell.
        /// </summary>
        /// <param name="start">Starting Cell</param>
        /// <param name="maxCost">The maximum cost allowed to spend</param>
        /// <param name="includingStartCell">Indicates whether the starting cell should be part of the returned HashSet.</param>
        /// <param name="considerIsNavigatable">Indicates whether to check the <see cref="GridCell.IsNavigatable"/> on each grid cell when considering pathing.</param>
        /// <returns>A list of cells that are accessible within the maximum cost.</returns>
        /// <exception cref="ArgumentException">Thrown if start cell is not navigatable and <paramref name="considerIsNavigatable"/> is set to true.</exception>
        public static HashSet<GridCell> FindAllCellsWithinCost(GridCell start, int maxCost, bool includingStartCell = false, bool considerIsNavigatable = false)
        {
            if (considerIsNavigatable)
            {
                if (!start.IsNavigatable) { throw new ArgumentException("Start Cell must be navigatable"); }
            }

            var frontier = new PriorityQueue<GridCell>();
            frontier.Enqueue(start, 0);

            var accessibleCells = new HashSet<GridCell>(); // List to store accessible cells
            var costSoFar = new Dictionary<GridCell, int>();

            costSoFar[start] = 0;
            if (includingStartCell) { accessibleCells.Add(start); }

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                IReadOnlyCollection<GridCell> nextCellsToConsider = considerIsNavigatable ? current.Neighbors.Where(cell => cell.IsNavigatable).ToList()
                                                                                          : current.Neighbors;

                // Iterate through neighbors
                foreach (var next in nextCellsToConsider)
                {
                    int newCost = costSoFar[current] + next.GetCostToEnter(current);

                    // Only proceed if cost is within acceptable range
                    if (newCost <= maxCost)
                    {
                        if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                        {
                            costSoFar[next] = newCost;
                            int priority = newCost; // Cost is used as priority
                            frontier.Enqueue(next, priority);

                            accessibleCells.Add(next);
                        }
                    }
                }
            }

            return accessibleCells;
        }
    }

}
