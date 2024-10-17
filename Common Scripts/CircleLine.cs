using UnityEngine;

namespace Venwin.CommonScripts
{
    /// <summary>
    /// Draws a circle with a <see cref="LineRenderer"/>.
    /// </summary>
    public class CircleLine : MonoBehaviour
    {
        [Header("Drawing Instructions")]
        [SerializeField] LineRenderer lineRenderer;
        [SerializeField] DrawLocation drawLocation = DrawLocation.Floor;
        [SerializeField] float heightModifier = 0.1f;
        [SerializeField] bool isMoving = true;

        [Header("Circle")]
        [SerializeField] int steps = 100;
        [SerializeField] float radius = 1.0f;

        private bool drawn = false;
        private Vector3 previousPosition;
        private Vector3 heightVector = Vector3.zero;

        public enum DrawLocation
        {
            Wall,
            Floor
        }

        // Start is called before the first frame update
        void Start()
        {
            lineRenderer.loop = true;
            heightVector = GetHeightVector();
            DrawCircle(steps, radius);
        }

        // Update is called once per frame
        void Update()
        {
            if (isMoving && drawn)
            {
                UpdateSteps();
            }
        }

        private void UpdateSteps()
        {
            Vector3[] newPositions = new Vector3[lineRenderer.positionCount];
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                newPositions[i] = lineRenderer.GetPosition(i) + transform.position - previousPosition;
            }

            previousPosition = transform.position;
            lineRenderer.SetPositions(newPositions);
        }

        void DrawCircle(int steps, float radius)
        {
            lineRenderer.positionCount = steps;
            Vector3 currentPosition = Vector3.zero;

            for (int currentStep = 0; currentStep < steps; currentStep++)
            {
                float circumferenceProgress = (float)currentStep / steps;

                float currentRadian = circumferenceProgress * 2 * Mathf.PI;

                float xScaled = Mathf.Cos(currentRadian);
                float yScaled = Mathf.Sin(currentRadian);

                float x = xScaled * radius;
                float y = yScaled * radius;

                switch (drawLocation)
                {
                    case DrawLocation.Wall:
                        currentPosition = new Vector3(x, y, 0) + transform.position + heightVector;
                        break;
                    case DrawLocation.Floor:
                        currentPosition = new Vector3(x, 0, y) + transform.position + heightVector;
                        break;
                }

                lineRenderer.SetPosition(currentStep, currentPosition);
            }

            previousPosition = transform.position;
            drawn = true;
        }

        private Vector3 GetHeightVector()
        {
            switch (drawLocation)
            {
                case DrawLocation.Wall:
                    return new Vector3(0, 0, heightModifier);
                case DrawLocation.Floor:
                    return new Vector3(0, heightModifier, 0);
                default:
                    return Vector3.zero;
            }
        }
    }

}
