using System.Collections.Generic;
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
        /// <returns>A list of cells that are accessible within the maximum cost.</returns>
        public static HashSet<GridCell> FindAllCellsWithinCost(GridCell start, int maxCost, bool includingStartCell = false)
        {
            var frontier = new PriorityQueue<GridCell>();
            frontier.Enqueue(start, 0);

            var accessibleCells = new HashSet<GridCell>(); // List to store accessible cells
            var costSoFar = new Dictionary<GridCell, int>();

            costSoFar[start] = 0;
            if (includingStartCell) { accessibleCells.Add(start); }

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                // Iterate through neighbors
                foreach (var next in current.Neighbors)
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
