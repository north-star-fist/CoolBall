using Sergei.Safonov.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Coolball.Configuration {

    /// <summary>
    /// Level configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "level", menuName = "Cool Ball/Levels/Level")]
    public class LevelSO : ScriptableObject {
        [field: SerializeField]
        public string LevelSceneKyey { get; private set; }

        [field: SerializeField, Tooltip("Maximum of balls/shots accessible for player")]
        public uint MaxShots = 15;
        [field: SerializeField]
        public List<BallSettings> BallsSettings = new();
        [field: SerializeField]
        public Vector2Int NewBallsRange = new Vector2Int(0, 2);
        [field: SerializeField]
        public Vector2Int ChangeBallsRange = new Vector2Int(3, 5);

        [Serializable]
        public class BallSettings {
            public int Score = 1;
            public Material Material;
            [Layer]
            public int Layer;
            public int SpriteIndex;
            public Color Color;
        }
    }
}
