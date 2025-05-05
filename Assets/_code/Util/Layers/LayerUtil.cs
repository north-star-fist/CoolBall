using UnityEngine;

namespace Sergei.Safonov.Unity {

    public static class LayerUtil {

        /// <summary>
        /// Checks if specified layer contained in a layer set.
        /// </summary>
        /// <param name="mask"> layer mask </param>
        /// <param name="layer"> layer to find </param>
        /// <returns> true if the layer is included in the mask </returns>
        public static bool IsLayerIncluded(this LayerMask mask, int layer) => IsLayerIncluded((int)mask, layer);

        /// <summary>
        /// Checks if specified layer contained in a layer set.
        /// </summary>
        /// <param name="mask"> layer mask </param>
        /// <param name="layer"> layer to find </param>
        /// <returns> true if the layer is included in the mask </returns>
        public static bool IsLayerIncluded(int mask, int layer) => (mask & (1 << layer)) != 0;

        /// <summary>
        /// Sets layer recursively to th whole object hierarchy starting from the specified one.
        /// </summary>
        /// <param name="rootObj"> root object (the upper one) </param>
        /// <param name="layer"> layer to set </param>
        public static void SetLayerRecursively(this GameObject rootObj, int layer) {
            rootObj.layer = layer;
            foreach (Transform t in rootObj.transform) {
                t.gameObject.SetLayerRecursively(layer);
            }
        }
    }
}
