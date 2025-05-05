using UnityEngine;

namespace Sergei.Safonov.Unity {

    public class ColoredParticles : MonoBehaviour {
        [SerializeField]
        ParticleSystem[] _particleSystems;

        public void SetColor(Color color) {
            if (_particleSystems == null) {
                return;
            }

            for (int i = 0; i < _particleSystems.Length; ++i) {
                var ps = _particleSystems[i];
                ps.startColor = color;
            }
        }
    }
}