using System.Linq;
using UnityEngine;
using Venwin.Utilities;

namespace Venwin.Systems
{
    /// <summary>
    /// Script that handles ghosting objects. Designed to be overriden as needed.
    /// </summary>
    public class GhostGameObject : MonoBehaviour
    {
        [Tooltip("This is the layer the game object will reside on")]
        [SerializeField] protected LayerMask ghostLayer;

        [Tooltip("This is the layer that the ghost will be raycasted against")]
        [SerializeField] protected LayerMask layersToGhostAgainst;


        [Header("Layers to Avoid")]
        [SerializeField] protected LayerMask layersToAvoidGhostingAgainst;

        [Tooltip("These are the renderers that will be enabled/disabled as it ghosts. If none is set, will grab all renderers in this object and children.")]
        [SerializeField] protected Renderer[] renderers;

        protected bool currentRenderState = true;

        /// <summary>
        /// Gets whether the ghost is being placed against a valid layer.
        /// </summary>
        /// <remarks>
        /// In default cases: where an invalid layer is infront of a valid one, <see cref="GhostIsValid"/> will consider this "invalid"
        /// in order to prevent placing objects inside or past one another.
        /// </remarks>
        public bool GhostIsValid { get; protected set; }

        /// <summary>
        /// Enables an object to be a ghost.
        /// </summary>
        public virtual void StartGhosting()
        {
            if (renderers == null || renderers.Length == 0)
            {
                renderers = gameObject.GetAllRenderers<Renderer>(true);
                gameObject.SetAllCollidersEnabled<Collider>(false, true);
            }

            gameObject.layer = ghostLayer;

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Disables an object and ends the ghosting.
        /// </summary>
        public virtual void StopGhosting()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Follows the cursors and moves the ghost object. Will prioritize not ghosting against invalid layers before finding what it can ghost against.
        /// </summary>
        protected virtual void FollowCursor()
        {
            if(IsGhostingOnBadLayers()) { return; }

            CursorInformation cursorInfo = CursorUtilities.CursorToWorldPosition(layersToGhostAgainst);

            if (cursorInfo.DidHit)
            {
                SetRenderersActive(true);
                GhostIsValid = true;
                transform.position = cursorInfo.Point;
                return;
            }

            OnInvalidGhostOffLayer();
        }

        protected bool IsGhostingOnBadLayers()
        {
            if (layersToAvoidGhostingAgainst != 0 && CursorUtilities.CursorToWorldPosition(layersToAvoidGhostingAgainst).DidHit)
            {
                OnInvalidGhostOffLayer();
                return true;
            }

            return false;
        }

        protected virtual void Awake()
        {
            
        }

        protected virtual void Update()
        {
            FollowCursor();
        }

        /// <summary>
        /// Called whenever a ghost is projected against the correct layer but is considered "invalid" for whatever reason.
        /// </summary>
        /// <remarks>Base implementation sets <see cref="GhostIsValid"/> and turns off renderers. Not called in base implementation.<br/>
        /// Use case example would be turning the ghost red due to inadequate resources.
        /// </remarks>
        protected virtual void OnInvalidGhostOnLayer()
        {
            SetRenderersActive(false);
            GhostIsValid = false;
        }

        /// <summary>
        /// Called whenever a ghost is not being projected against a valid layer.
        /// </summary>
        /// <remarks>Base implementation sets <see cref="GhostIsValid"/> and turns off renderers.</remarks>
        protected virtual void OnInvalidGhostOffLayer()
        {
            SetRenderersActive(false);
            GhostIsValid = false;
        }

        /// <summary>
        /// Sets all the assigned renderers in this ghost object to an enabled/disabled state.
        /// </summary>
        /// <remarks>
        /// Efficient to call even if setting renderers to the same value. A short circuit check is called first.
        /// </remarks>
        /// <param name="enabled">Sets all the renderers enable state to this state.</param>
        protected void SetRenderersActive(bool enabled)
        {
            if(currentRenderState == enabled) { return; }

            foreach(Renderer renderer in renderers)
            {
                renderer.enabled = enabled;
            }

            currentRenderState = enabled;
        }
    }
}
