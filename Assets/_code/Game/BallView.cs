using UnityEngine;

namespace Coolball {

    /// <summary>
    /// Game ball facade.
    /// </summary>
    public class BallView : MonoBehaviour {

        // We want to roll balls mostly, not to just kick. So we apply our force not to center of a ball but upper.
        private const float KickVerticalShift = .8f;

        [SerializeField]
        private Renderer _ballRenderer;
        [SerializeField]
        private SphereCollider _ballCollider;
        [SerializeField]
        private SphereCollider _triggerCollider;
        [SerializeField]
        private Rigidbody _rigidBody;

        private int _ball;


        public int BallNumber => _ball;

        public bool IsKinematic => _rigidBody.isKinematic;

        public float Radius => _ballCollider.radius;


        public Collider Collider => _ballCollider;

        public Collider TriggerCollider => _triggerCollider;

        public Rigidbody Rigidbody => _rigidBody;

        public Vector3 RigidbodyVelocity => _rigidBody.velocity;

        public void Kick(Vector2 dir, float speed) {
            if (_rigidBody.isKinematic) {
                // This approach does not work in Unity 2022.1 and later

                // just changing velocity
                // There will no any rotation, but for the very short time so we don't care
                // _rigidBody.velocity = dir * speed;
            } else {
                // assuming pivot of the ball object is it's bottom
                Vector3 forcePos = transform.position + Vector3.up * _ballCollider.bounds.size.y * KickVerticalShift;
                Vector3 force = new Vector3(dir.x, 0, dir.y) * speed;
                _rigidBody.AddForceAtPosition(force, forcePos, ForceMode.VelocityChange);
            }
        }

        // Start is called before the first frame update
        public void Setup(int ball, Material renderMat, int layer, bool kinematic = true) {
            _ball = ball;
            _ballCollider.gameObject.layer = layer;
            _ballRenderer.material = renderMat;
            SetKinematic(kinematic);
        }

        public void SetKinematic(bool on) {
            _rigidBody.isKinematic = on;
        }

        public void Move(Vector3 newPosition) {
            _rigidBody.MovePosition(newPosition);
        }
    }
}