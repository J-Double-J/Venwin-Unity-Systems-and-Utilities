
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable

namespace Venwin.Utilities
{
    public static class ParentChildUtilities
    {
        #region Getting All Components in Parent and Children

        /// <summary>
        /// Gets all the renderers on this game object and optionally children.
        /// </summary>
        /// <typeparam name="T">Type of renderers to look for.</typeparam>
        /// <param name="self">Gameobject parent.</param>
        /// <param name="enabled">What state to set the renderers to.</param>
        /// <param name="inChildren">If true, sets any children's renderers to the same state.</param>
        public static T[] GetAllRenderers<T>(this GameObject self, bool inChildren, bool includeInactive = true) where T : Renderer
        {
            T[] renderers = self.GetComponents<T>();
            if (inChildren)
            {
                renderers = renderers.Concat(self.GetComponentsInChildren<T>(includeInactive)).ToArray();
            }

            return renderers;
        }

        #endregion

        #region Setting and Enabling Components

        /// <summary>
        /// Sets whether the behaviors for this object and its children are enabled.
        /// </summary>
        /// <typeparam name="T">Type of behaviors to look for.</typeparam>
        /// <param name="self">Gameobject parent.</param>
        /// <param name="enabled">What state to set the behaviors to.</param>
        /// <param name="inChildren">If true, sets any children's behaviors to the same state.</param>
        public static void SetAllBehaviorComponentsEnabled<T>(this GameObject self, bool enabled, bool inChildren) where T : Behaviour
        {
            if (self.TryGetComponent(out T selfComponent))
            {
                selfComponent.enabled = enabled;
            }

            foreach (T behavior in self.GetComponents<T>())
            {
                behavior.enabled = enabled;
            }

            if (inChildren)
            {
                foreach (T behavior in self.GetComponentsInChildren<T>())
                {
                    behavior.enabled = enabled;
                }
            }
        }

        /// <summary>
        /// Sets whether the colliders for this object and its children are enabled.
        /// </summary>
        /// <typeparam name="T">Type of colliders to look for.</typeparam>
        /// <param name="self">Gameobject parent.</param>
        /// <param name="enabled">What state to set the colliders to.</param>
        /// <param name="inChildren">If true, sets any children's colliders to the same state.</param>
        public static void SetAllCollidersEnabled<T>(this GameObject self, bool enabled, bool inChildren) where T : Collider
        {
            if(self.TryGetComponent(out T selfComponent))
            {
                selfComponent.enabled = enabled;
            }

            if (inChildren)
            {
                foreach (T collider in self.GetComponentsInChildren<T>())
                {
                    collider.enabled = enabled;
                }
            }
            else
            {
                foreach (T collider in self.GetComponents<T>())
                {
                    collider.enabled = enabled;
                }
            }
        }

        /// <summary>
        /// Sets whether the renderers for this object and its children are enabled.
        /// </summary>
        /// <typeparam name="T">Type of renderers to look for.</typeparam>
        /// <param name="self">Gameobject parent.</param>
        /// <param name="enabled">What state to set the renderers to.</param>
        /// <param name="inChildren">If true, sets any children's renderers to the same state.</param>
        public static void SetAllRenderersEnabled<T>(this GameObject self, bool enabled, bool inChildren) where T : Renderer
        {
            if (self.TryGetComponent(out T selfComponent))
            {
                selfComponent.enabled = enabled;
            }

            if (inChildren)
            {
                foreach (T renderer in self.GetComponentsInChildren<T>())
                {
                    renderer.enabled = enabled;
                }
            }
            else
            {
                foreach (T renderer in self.GetComponents<T>())
                {
                    renderer.enabled = enabled;
                }
            }
        }

        #endregion

        /// <summary>
        /// Gets all the children
        /// </summary>
        /// <param name="parent"></param>
        public static List<T> FindChildrenWithComponent<T>(Transform parent, List<T>? componentsFound = null) where T : Component
        {
            if (componentsFound == null)
            {
                componentsFound = new();
            }

            foreach (Transform child in parent)
            {
                if (child.TryGetComponent(out T component))
                {
                    componentsFound.Add(component);
                }

                // Recursively call FindChildrenWithComponent for each child
                FindChildrenWithComponent(child, componentsFound);
            }

            return componentsFound;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rootGameObject">The starting object to search in that contains children.</param>
        /// <returns></returns>
        public static T? GetComponentInParentOrChildren<T>(GameObject rootGameObject) where T : Component
        {
            T component;

            component = rootGameObject.GetComponent<T>();

            if(component != null) { return component; }

            component = rootGameObject.GetComponentInChildren<T>();

            return component;
        }

        /// <summary>
        /// Sets an object and all of its children to a certain layer.
        /// </summary>
        /// <param name="gameObject">Parent object</param>
        /// <param name="layerMask">Layer to set object to.</param>
        public static void SetObjectAndChildrenToLayer(GameObject gameObject, LayerMask layerMask)
        {
            gameObject.layer = layerMask;

            foreach (Transform child in gameObject.transform)
            {
                SetObjectAndChildrenToLayer(child.gameObject, layerMask);
            }
        }
    }
}
