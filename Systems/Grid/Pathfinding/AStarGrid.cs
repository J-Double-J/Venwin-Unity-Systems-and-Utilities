﻿using System;
using System.Collections.Generic;
using Venwin.DataStructures;

#nullable enable

namespace Venwin.Grid.Pathfinding
{
    public class AStarGrid
    {
        /// <summary>
        /// Finds a path between two <see cref="GridCell"/>s.
        /// </summary>
        /// <param name="start">Starting cell.</param>
        /// <param name="goal">End cell.</param>
        /// <returns>The path of <see cref="GridCell"/>'s pointing to the previous cell it came from. If no path is found returns null.</returns>
        public static Dictionary<GridCell, GridCell>? FindPath(GridCell start, GridCell goal)
        {
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

                foreach (var next in current.Neighbors)
                {
                    int newCost = costSoFar[current] + next.GetCostToEnter(current);
                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        int priority = newCost + Heuristic(goal, next);
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
        /// <returns>A list of the cells from start to end. If none is found, returns null instead.</returns>
        public static List<GridCell>? GetPath(GridCell start, GridCell goal)
        {
            return GetPathFromFoundPath(FindPath(start, goal), start, goal);
        }

        private static List<GridCell>? GetPathFromFoundPath(Dictionary<GridCell, GridCell>? cameFrom, GridCell start, GridCell goal)
        {
            if(cameFrom == null) { return null; }

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

        private static int Heuristic(GridCell a, GridCell b)
        {
            // Manhattan distance on a square grid
            return Math.Abs(a.GridCoordinates.x - b.GridCoordinates.x) + Math.Abs(a.GridCoordinates.z - b.GridCoordinates.z);
        }
    }
}
