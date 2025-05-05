using UnityEditor;
using UnityEngine;

namespace Sergei.Safonov.Unity {

    [CustomPropertyDrawer(typeof(LayerAttribute))]
    public class LayerAttributeEditor : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            property.intValue = EditorGUI.LayerField(position, label, property.intValue);
        }
    }
}
