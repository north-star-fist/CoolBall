using UnityEngine;

namespace Sergei.Safonov.Unity {

    public static class VectorExt {

        #region Vectors quick amending
        public static Vector3 WithX(this Vector3 v, float x) => new Vector3(x, v.y, v.z);
        public static Vector2 WithX(this Vector2 v, float x) => new Vector2(x, v.y);

        public static Vector3 WithY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);
        public static Vector2 WithY(this Vector2 v, float y) => new Vector2(v.x, y);

        public static Vector3 WithZ(this Vector3 v, float z) => new Vector3(v.x, v.y, z);

        public static Vector3 WithXIf(this Vector3 v, float x, bool condit) => condit ? new Vector3(x, v.y, v.z) : v;
        public static Vector2 WithXIf(this Vector2 v, float x, bool condit) => condit ? new Vector2(x, v.y) : v;

        public static Vector3 WithYIf(this Vector3 v, float y, bool condit) => condit ? new Vector3(v.x, y, v.z) : v;
        public static Vector2 WithYIf(this Vector2 v, float y, bool condit) => condit ? new Vector2(v.x, y) : v;

        public static Vector3 WithZIf(this Vector3 v, float z, bool condit) => condit ? new Vector3(v.x, v.y, z) : v;
        #endregion


        #region Extraction Vector2 from Vector3
        public static Vector2 XY(this Vector3 v) => new Vector2(v.x, v.y);
        public static Vector2 XZ(this Vector3 v) => new Vector2(v.x, v.z);
        public static Vector2 YZ(this Vector3 v) => new Vector2(v.y, v.z);
        public static Vector2 YX(this Vector3 v) => new Vector2(v.y, v.x);
        public static Vector2 ZX(this Vector3 v) => new Vector2(v.z, v.x);
        public static Vector2 ZY(this Vector3 v) => new Vector2(v.z, v.y);
        #endregion

        public static Vector3 ChooseClosest(this Vector3 position, Vector3 distant1, Vector3 distant2) {
            var sqrDistance1 = (distant1 - position).sqrMagnitude;
            var sqrDistance2 = (distant2 - position).sqrMagnitude;
            return sqrDistance1 <= sqrDistance2 ? distant1 : distant2;
        }

        public static Vector3 ChooseFarthest(this Vector3 position, Vector3 distant1, Vector3 distant2) {
            var sqrDistance1 = (distant1 - position).sqrMagnitude;
            var sqrDistance2 = (distant2 - position).sqrMagnitude;
            return sqrDistance1 >= sqrDistance2 ? distant1 : distant2;
        }
    }
}
