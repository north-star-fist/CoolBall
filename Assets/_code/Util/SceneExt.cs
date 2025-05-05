using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sergei.Safonov.Unity {

    public static class SceneExt {
        public static bool TryGetRootGameObject<T>(this Scene scene, out T result) {
            result = default;
            foreach (GameObject gameObject in scene.GetRootGameObjects()) {
                if (gameObject.TryGetComponent(out T component)) {
                    result = component;
                    return true;
                }
            }
            return false;
        }

        public static void GetRootGameObjects<T>(this Scene scene, List<T> resultList) {
            foreach (GameObject gameObject in scene.GetRootGameObjects()) {
                if (gameObject.TryGetComponent(out T component)) {
                    resultList.Add(component);
                }
            }
        }
    }
}
