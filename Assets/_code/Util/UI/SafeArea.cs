using UnityEngine;

namespace Sergei.Safonov.Unity.UI {

    /// <summary>
    /// Add this component to your root UI panel to scale it to device screen Safe Area automatically.
    /// </summary>
    public class SafeArea : MonoBehaviour {
        private RectTransform _safeAreaRectTransform;
        private ScreenOrientation _currentOrientation;
        private Rect _currentSafeArea;
        private Canvas _canvas;
        private void Awake() {
            _canvas = transform.GetComponentInParent<Canvas>();
            _safeAreaRectTransform = GetComponent<RectTransform>();
        }
        // Start is called before the first frame update
        void Start() {
            _currentOrientation = Screen.orientation;
            _currentSafeArea = Screen.safeArea;
            ApplySafeArea();
        }
        private void ApplySafeArea() {
            if (_safeAreaRectTransform == null) {
                return;
            }
            Rect safeArea = Screen.safeArea;
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= _canvas.pixelRect.width;
            anchorMin.y /= _canvas.pixelRect.height;
            anchorMax.x /= _canvas.pixelRect.width;
            anchorMax.y /= _canvas.pixelRect.height;
            _safeAreaRectTransform.anchorMin = anchorMin;
            _safeAreaRectTransform.anchorMax = anchorMax;
            _currentOrientation = Screen.orientation;
            _currentSafeArea = Screen.safeArea;
        }
        // Update is called once per frame
        void Update() {
            if (_currentOrientation != Screen.orientation || _currentSafeArea != Screen.safeArea) {
                ApplySafeArea();
            }
        }
    }
}
