using UnityEngine;

namespace Sergei.Safonov.Unity {

    /// <summary>
    /// Extensions for <see cref="GameObject"/> class.
    /// </summary>
    public static class GameObjectExt {

        /// <summary>
        /// Gets a particular component of a <see cref="GameObject"/>. Adds the component
        /// if there is no such one attached to the game object.
        /// </summary>
        /// <typeparam name="T"> type of the needed component </typeparam>
        /// <param name="obj"> game object </param>
        /// <returns> gotten or added component </returns>
        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component {
            if (!obj.TryGetComponent<T>(out var comp)) {
                comp = obj.AddComponent<T>();
            }
            return comp;
        }

        /// <summary>
        /// Destroys all the child game objects of the specified game object.
        /// </summary>
        /// <param name="transform"> parent game object transform </param>
        public static void DestroyAllChildObjects(this Transform transform) {
            while (transform.childCount > 0) {
                GameObject.Destroy(transform.GetChild(0).gameObject);
            }
        }

        public static bool CompareTags(this GameObject obj, string[] tags, bool resultIfNoTags = false) {
            if (tags == null || tags.Length == 0)
                return resultIfNoTags;

            foreach (string tag in tags) {
                if (obj.CompareTag(tag)) {
                    return true;
                }
            }

            return false;
        }
    }
}
