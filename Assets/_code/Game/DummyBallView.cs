using UnityEngine;

namespace Coolball {

    /// <summary>
    /// Dummy ball to show to player before it's real twin brother is kicked out.
    /// </summary>
    public class DummyBallView : MonoBehaviour {

        [SerializeField]
        private Renderer _ballRenderer;

        private int _ball;

        public int Ball => _ball;


        // Start is called before the first frame update
        public void Setup(int ball, Material renderMat) {
            _ball = ball;
            _ballRenderer.material = renderMat;
        }
    }
}