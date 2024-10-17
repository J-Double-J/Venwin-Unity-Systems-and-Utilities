using System.Collections.Generic;
using UnityEngine;

namespace Venwin.Grid.Pathfinding
{
    [RequireComponent(typeof(LineRenderer))]
    public class GridPathLineDebugger : MonoBehaviour 
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float heightOffset = 0.2f;

        public void Awake()
        {
            if(lineRenderer == null)
            {
                if (!TryGetComponent<LineRenderer>(out lineRenderer))
                {
                    Debug.LogError($"{typeof(GridPathLineDebugger).Name} needs a {typeof(LineRenderer).Name} attached to the game object to render properly.");
                }
            }
        }

        public void DrawPath(List<GridCell> path)
        {
            lineRenderer.positionCount = path.Count;

            for(int i = 0; i < path.Count; i++)
            {
                lineRenderer.SetPosition(i, (path[i].CenterOfCellWorldSpace() + new Vector3(0, heightOffset, 0)));
            }
        }
    }
}
