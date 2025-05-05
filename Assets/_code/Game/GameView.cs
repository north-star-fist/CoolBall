using Cinemachine;
using Coolball.Configuration;
using Coolball.Input;
using R3;
using R3.Triggers;
using Sergei.Safonov.Unity;
using Sergei.Safonov.Unity.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
using UnityEngine.UI;
using static Coolball.Configuration.LevelSO;

namespace Coolball {

    /// <summary>
    /// Game view (responsible for the game level scene).
    /// </summary>
    /// <seealso cref="GameUiManager"/>
    public class GameView : MonoBehaviour, IGameView, GameplayInput.IGameplayActionsActions {

        // Number of points to draw 'circle'
        private const int HintCirclePointsCount = 16;
        private const float MinStartSpeed = 0.001f;
        private const float MaxStartSpeed = 100f;
        // After this interval explosion goes back to pool
        private const float VfxReleaseTime = 3f;

        [Header("Balls")]
        [SerializeField] private Transform _ballSpawner;
        [SerializeField, Range(MinStartSpeed, MaxStartSpeed)] private float _ballStartSpeed = 5f;
        [SerializeField] private BallView _ballPrefab;
        [SerializeField]
        private BoxCollider _kinematicArea;

        [Header("Camera")]
        [SerializeField]
        private Camera _gameplayCamera;
        [SerializeField]
        private CinemachineVirtualCameraBase _cinemachineCamera;
        [SerializeField]
        private CinemachineImpulseSource _cinemachineImpulseSource;

        [SerializeField, Tooltip("Layer(s) that is used for aiming balls through touching the 'ground'")]
        private LayerMask _touchableLayers = -1;

        [Header("Spawner")]
        [SerializeField] private DummyBallView _dummyBallPrefab;
        [SerializeField, Tooltip("Place/parent for dummy ball that shows what ball will be kicked")]
        private Transform _dummyBallPlace;
        [
            SerializeField,
            Tooltip("Balls can be pushed in directions within this transform rotation -_limitAngle/2 - _limitAngle/2")
        ]
        private float _limitAngle = 120f;
        [SerializeField]
        private LineRenderer _hintRayRenderer;
        [SerializeField]
        private LineRenderer _hintCircleRenderer;
        [SerializeField]
        private LayerMask _hintRayCollisionMask = -1;

        [Header("World UI")]
        [SerializeField]
        private TMP_Text _scoreText;
        [SerializeField]
        private TMP_Text _8xText;
        [SerializeField]
        private Image _shotsImage;
        [SerializeField]
        private int _topBallBwSpriteIndex = 0;

        [Header("SFX")]
        [SerializeField]
        private AudioClip _shotSfx;
        [SerializeField]
        private AudioClip _wallBounceSfx;
        [SerializeField]
        private AudioClip _ballBounceSfx;
        [SerializeField]
        private AudioClip _ballEliminationSfx;
        [SerializeField]
        private AudioClip _topBallEliminationSfx;
        [SerializeField]
        private AudioClip _gameStartSfx;
        [SerializeField]
        private AudioClip _gameOverSfx;
        [SerializeField]
        private AudioClip _kickSfx;
        [SerializeField]
        private AudioClip _changeBallSfx;
        [SerializeField]
        private AudioSource _audioSource;
        [SerializeField]
        private LayerMask _ballLayers = (1 << 8) + (1 << 9) + (1 << 10) + (1 << 11)
                                    + (1 << 12) + (1 << 13) + (1 << 14) + (1 << 15);
        [SerializeField]
        private LayerMask _wallLayers = 1;

        [Header("VFX")]
        [SerializeField]
        private ColoredParticles _ballEliminationVfx;
        [SerializeField]
        private ColoredParticles _topBallEliminationVfx;


        public IObservable<bool> OnShot => _onShot.AsSystemObservable();
        private readonly Subject<bool> _onShot = new();
        public IObservable<(int, Vector3)> BallEliminated => _ballEliminated.AsSystemObservable();
        private readonly Subject<(int, Vector3)> _ballEliminated = new();
        public IObservable<int> TopBallEliminated => _topBallsRx.AsSystemObservable();
        readonly ReactiveProperty<int> _topBallsRx = new ReactiveProperty<int>(0);

        private LevelSO _levelSettings;
        private float _ballRadius;
        private DummyBallView _dummyBall;

        // Input
        GameplayInput _input;
        Vector3 _mousePosition;
        private bool _touching = false;
        // Last legit aim position (in 3D space).
        private Vector3? _legitTouchPosition;
        private Vector3? _shotDirection;

        // Null when is not loaded
        private int? _currentBall;
        private readonly Dictionary<Collider, BallView> _ballColliders = new();
        private readonly Dictionary<BallView, Vector3> _ballVelocities = new();

        private ObjectPool<BallView> _ballPool;
        private ObjectPool<ColoredParticles> _ballElimVfxPool;
        private ObjectPool<ColoredParticles> _topBallElimVfxPool;


        private void Awake() {
            if (_hintRayRenderer != null) {
                _hintRayRenderer.positionCount = 2;
                _hintRayRenderer.useWorldSpace = true;
            }
            if (_hintCircleRenderer != null) {
                _hintCircleRenderer.positionCount = HintCirclePointsCount;
                _hintCircleRenderer.loop = true;
                _hintCircleRenderer.useWorldSpace = true;
            }

            _ballRadius = _ballPrefab.Radius;

            _kinematicArea.OnTriggerExitAsObservable().Subscribe(FreeBall).AddTo(_kinematicArea);
        }

        private void OnEnable() {
            if (_input == null) {
                _input = new GameplayInput();
                _input.GameplayActions.SetCallbacks(this);
            }
            _input.GameplayActions.Enable();
        }
        private void OnDisable() {
            if (_input != null) {
                _input.GameplayActions.RemoveCallbacks(this);
                _input.GameplayActions.Disable();
            }
        }

        private void Update() {
            if (_touching && _legitTouchPosition.HasValue) {
                Aim(_legitTouchPosition.Value);
            }
        }

        private void FixedUpdate() {
            // Moving kinematic balls by hands because of PhysX prohibitting setting velocity of kinematic bodies.
            foreach ((var ball, var velocity) in _ballVelocities) {
                if (ball.IsKinematic) {
                    ball.Move(ball.transform.position + Time.fixedDeltaTime * velocity);
                }
            }
        }

        private void OnValidate() {
            if (_ballSpawner == null) {
                Debug.LogWarning($"Ball spawner transform must be set!");
            }
            if (_ballPrefab == null) {
                Debug.LogWarning($"Ball prefab must be set!");
            }
            if (_ballStartSpeed <= 0) {
                _ballStartSpeed = MinStartSpeed;
            }
        }

        private void OnDrawGizmosSelected() {
            // Using camera gizmo just to make it more simple
            Gizmos.DrawFrustum(_ballSpawner.transform.position, _limitAngle, 20, 0, 1);
        }


        public void Setup(LevelSO level) {
            // TODO: rewrite it to prohibit direct SO changing
            _levelSettings = level;
        }

        #region Gameplay

        public void OnGameStarted() {
            // TODO: show aiming hint

            // Clean
            foreach (var ball in _ballColliders.Values) {
                if (ball != null) {
                    ReleaseBallToPool(ball);
                }
            }
            _ballColliders.Clear();
            _ballVelocities.Clear();

            // Reset
            SetScore(0);
            SetEights(0);

            SetBallsLeft(_levelSettings.MaxShots, _levelSettings.MaxShots);

            PlaySound(_gameStartSfx);
        }

        public void OnGameOver() {
            // TODO: hide aiming hint

            PlaySound(_gameOverSfx);
        }

        public void LoadBall(int ball, bool manually = false) {
            _currentBall = ball;
            if (_dummyBall == null) {
                _dummyBall = Instantiate(_dummyBallPrefab, _dummyBallPlace);
            }
            _dummyBall.Setup(ball, _levelSettings.BallsSettings[ball].Material);
            _dummyBall.gameObject.SetActive(true);
            if (manually) {
                PlaySound(_changeBallSfx);
            }
        }


        public void Shuffle() {
            foreach (var b in _ballColliders.Values) {
                if (!b.IsKinematic) {
                    Vector2 dir = UnityEngine.Random.insideUnitCircle;
                    b.Kick(dir, _ballStartSpeed);
                }
            }
            PlaySound(_kickSfx);
            if (_cinemachineImpulseSource != null) {
                _cinemachineImpulseSource.GenerateImpulse();
            }
        }

        private void ShootBall() {
            if (UIUtil.IsPointerOverUI(_mousePosition)) {
                return;
            }
            var groundPos = GetGroundCoordinates(_mousePosition);
            if (groundPos.HasValue) {
                HideDummy();
                SpawnBall();
                _currentBall = null;
                _onShot.OnNext(true);
            }
        }

        private void HandleSameColoredBump(Collider col, BallView triggerOwner) {
            // We assume also that triggers have default layer so they collide with Ball layers.
            // So here we have ball collider trespassing another ball trigger (and vice versa)

            if (_ballColliders.TryGetValue(col, out var penetrator)) {
                if (penetrator != null && penetrator.gameObject.activeInHierarchy) {
                    // it's alive. Let's check that our balls have the same color actually
                    // Now let's check is trigger alive itself
                    if (triggerOwner != null && triggerOwner.gameObject.activeInHierarchy) {
                        // Ok both balls are alive
                        if (penetrator.BallNumber != triggerOwner.BallNumber) {
                            // Something went wrong, trigger collider was touched by physical collider before 
                            // physical ones do. Let's just go out
                            return;
                        }
                        // Ok we are first
                        var penetratorVel = GetBallVelocity(penetrator);
                        var triggerVel = GetBallVelocity(triggerOwner);

                        if (penetrator.BallNumber == _levelSettings.BallsSettings.Count - 1) {
                            // It's the top. Eliminating both balls. It's special case

                            Eliminate(penetrator, false);
                            Eliminate(triggerOwner);
                            _topBallsRx.Value++;
                        } else {
                            // Let's eliminate the one who is slower
                            if (penetratorVel.sqrMagnitude < triggerVel.sqrMagnitude) {
                                // Penetrator lost
                                Eliminate(penetrator);
                                Repaint(triggerOwner);
                            } else {
                                // Trigger owner lost
                                Eliminate(triggerOwner);
                                Repaint(penetrator);
                            }
                        }
                    }
                }
            }
        }

        private void Repaint(BallView ball) {
            if (_levelSettings.BallsSettings.Count == 0) {
                return;
            }
            int ballNum = ball.BallNumber + 1;
            if (ballNum >= _levelSettings.BallsSettings.Count) {
                ballNum = 0;
            }
            PaintBall(ball, ballNum, false);
        }

        private void PaintBall(BallView ballObj, int ball, bool kinematic) {
            List<BallSettings> ballSettings = _levelSettings.BallsSettings;
            ballObj.Setup(ball, ballSettings[ball].Material, ballSettings[ball].Layer, kinematic);
        }

        private void SpawnBall() {
            if (!_shotDirection.HasValue) {
                return;
            }

            if (!_currentBall.HasValue) {
                return;
            }
            BallView ballObj = GetBallFromPool();
            PaintBall(ballObj, _currentBall.Value, true);
            ballObj.Kick(_shotDirection.Value, _ballStartSpeed);

            PlaySound(_shotSfx);
        }

        private void Eliminate(BallView ball, bool notifyListeners = true) {
            // Cleaning collections
            _ballVelocities.Remove(ball);
            _ballColliders.Remove(ball.Collider);

            if (notifyListeners) {
                _ballEliminated.OnNext((ball.BallNumber, ball.transform.position));

                // dirty hack with sfx/vfx. We don't play it when no notification (when twin top ball explodes)
                if (ball.BallNumber == _levelSettings.BallsSettings.Count - 1 && _topBallEliminationSfx != null) {
                    PlaySound(_topBallEliminationSfx);
                } else {
                    PlaySound(_ballEliminationSfx);
                }
                Explode(ball.BallNumber, ball.transform.position);
            }
            ReleaseBallToPool(ball);
        }

        private void Aim(Vector3 groundPos) {
            _shotDirection = null;
            if (!IsWithinAimingLimits(groundPos, out var dir)) {
                return;
            }
            Vector3 thisPosition = _ballSpawner.transform.position;
            _shotDirection = dir;
            if (_hintRayRenderer != null) {
                Vector3? wallPoint = GetWallPoint(dir);
                if (wallPoint.HasValue) {
                    var circleCenter = wallPoint.Value - dir * _ballRadius;
                    _hintRayRenderer.SetPosition(0, thisPosition);
                    _hintRayRenderer.SetPosition(1, circleCenter);
                    DrawHintCircle(circleCenter);
                }
            }
        }

        // Now the ball can go physically
        private void FreeBall(Collider col) {
            if (col.isTrigger) {
                // inner trigger went out kinematic area. We don't care about it
                return;
            }
            _ballColliders[col].SetKinematic(false);
        }

        private Vector3 GetBallVelocity(BallView ball) {
            var ballVelocity = ball.RigidbodyVelocity;
            if (ball.IsKinematic) {
                // The ball is controlled by us, not physics, so it's velovity we take from the array
                ballVelocity = _ballVelocities[ball];
            }
            return ballVelocity;
        }
        #endregion

        #region Handling Input (mouse clicks)
        public void OnMousePosition(InputAction.CallbackContext context) {
            _mousePosition = context.ReadValue<Vector2>();
            var grCoordinates = GetGroundCoordinates(_mousePosition);
            if (grCoordinates.HasValue && IsWithinAimingLimits(grCoordinates.Value, out var _)) {
                _legitTouchPosition = grCoordinates.Value;
            }
        }

        public Vector3? GetGroundCoordinates(Vector2 mousePosition) {
            if (_gameplayCamera == null) {
                return default;
            }
            var ray = _gameplayCamera.ScreenPointToRay(mousePosition);
            return Physics.Raycast(ray, out RaycastHit hitInfo, float.MaxValue, _touchableLayers) ? hitInfo.point : null;
        }

        public void OnTouch(InputAction.CallbackContext context) {
            if (context.started) {
                // touch
                _touching = true;
            } else if (context.canceled) {
                // untouch
                _touching = false;
                if (_currentBall.HasValue) {
                    ShootBall();
                }
            }
        }
        #endregion


        #region World UI
        public void SetScore(int score) {
            if (_scoreText != null) {
                _scoreText.text = $"Score: {score.ToString()}";
            }
        }

        public void SetEights(int eights) {
            if (_8xText != null) {
                var curTint = _8xText.color;
                var colStr = ColorUtility.ToHtmlStringRGBA(curTint);
                _8xText.text = $"<sprite={_topBallBwSpriteIndex.ToString()} color=#{colStr}> x {eights.ToString()}";
            }
        }

        public void SetBallsLeft(uint balls, uint maxBalls) {
            if (_shotsImage != null) {
                _shotsImage.fillAmount = (float)balls / maxBalls;
            }
        }
        #endregion

        #region Ball Pool
        private BallView GetBallFromPool() {
            IObjectPool<BallView> pool = GetBallPool();
            BallView ball = pool.Get();
            ball.transform.SetPositionAndRotation(_ballSpawner.transform.position, Quaternion.identity);
            return ball;
        }

        private void ReleaseBallToPool(BallView ball) {
            IObjectPool<BallView> pool = GetBallPool();
            pool.Release(ball);
        }

        private IObjectPool<BallView> GetBallPool() {
            if (_ballPool == null) {
                _ballPool = new ObjectPool<BallView>(
                    createFunc: CreateBall,
                    actionOnGet: SwitchBallOn,
                    actionOnRelease: SwitchBallOff,
                    actionOnDestroy: Destroy
                );
            }
            return _ballPool;
        }

        private BallView CreateBall() {
            var ballObj = Instantiate(_ballPrefab, transform);
            ballObj.TriggerCollider.OnTriggerEnterAsObservable().Subscribe(col => HandleSameColoredBump(col, ballObj))
                .AddTo(ballObj);
            ballObj.Rigidbody.OnCollisionEnterAsObservable().Subscribe(HandleBallPhysicalCollision).AddTo(ballObj);
            return ballObj;
        }

        private void HandleBallPhysicalCollision(Collision otherBall) {
            // just hacky approach to split SFX between walls and balls, assuming walls have Default layer.
            if (_ballLayers.IsLayerIncluded(otherBall.collider.gameObject.layer)) {
                PlaySound(_ballBounceSfx);
            } else if (_wallLayers.IsLayerIncluded(otherBall.collider.gameObject.layer)) {
                PlaySound(_wallBounceSfx);
            }
        }

        private void SwitchBallOn(BallView ballObj) {
            _ballColliders[ballObj.Collider] = ballObj;
            _ballVelocities[ballObj] = _shotDirection.Value * _ballStartSpeed;
            ballObj.gameObject.SetActive(true);
        }

        private void SwitchBallOff(BallView ballObj) {
            ballObj.gameObject.SetActive(false);
        }
        #endregion

        #region VFX Pools
        private ColoredParticles GetElimVfxFromPool() {
            IObjectPool<ColoredParticles> pool = GetElimVfxPool();
            ColoredParticles vfx = pool.Get();
            vfx.transform.SetPositionAndRotation(_ballSpawner.transform.position, Quaternion.identity);
            ReleaseByTime releaser = vfx.gameObject.GetOrAddComponent<ReleaseByTime>();
            releaser.ReleaseAfter(pool, vfx, VfxReleaseTime);
            return vfx;
        }

        private IObjectPool<ColoredParticles> GetElimVfxPool() {
            if (_ballElimVfxPool == null) {
                _ballElimVfxPool = new ObjectPool<ColoredParticles>(
                    createFunc: CreateElimVfx,
                    actionOnGet: SwitchElimVfxOn,
                    actionOnRelease: SwitchElimVfxOff,
                    actionOnDestroy: Destroy
                );
            }
            return _ballElimVfxPool;
        }

        private ColoredParticles CreateElimVfx() {
            var vfx = Instantiate(_ballEliminationVfx, transform);
            return vfx;
        }

        private void SwitchElimVfxOn(ColoredParticles vfx) {
            vfx.gameObject.SetActive(true);
        }

        private void SwitchElimVfxOff(ColoredParticles vfx) {
            vfx.gameObject.SetActive(false);
        }


        private ColoredParticles GetTopElimVfxFromPool() {
            IObjectPool<ColoredParticles> pool = GetTopElimVfxPool();
            ColoredParticles vfx = pool.Get();
            ReleaseByTime releaser = vfx.gameObject.GetOrAddComponent<ReleaseByTime>();
            releaser.ReleaseAfter(pool, vfx, VfxReleaseTime);
            return vfx;
        }

        private IObjectPool<ColoredParticles> GetTopElimVfxPool() {
            if (_ballElimVfxPool == null) {
                _ballElimVfxPool = new ObjectPool<ColoredParticles>(
                    createFunc: CreateTopElimVfx,
                    actionOnGet: SwitchElimVfxOn,
                    actionOnRelease: SwitchElimVfxOff,
                    actionOnDestroy: Destroy
                );
            }
            return _ballElimVfxPool;
        }

        private ColoredParticles CreateTopElimVfx() {
            var vfx = Instantiate(_topBallEliminationVfx, transform);
            return vfx;
        }
        #endregion


        private void Explode(int ballNumber, Vector3 position) {
            ColoredParticles ps = null;
            if (ballNumber == _levelSettings.BallsSettings.Count - 1 && _topBallEliminationVfx != null) {
                ps = GetTopElimVfxFromPool();
            } else {
                if (_ballEliminationVfx != null) {
                    ps = GetElimVfxFromPool();
                }
            }
            if (ps != null) {
                ps.transform.SetPositionAndRotation(position, Quaternion.identity);
                ps.SetColor(_levelSettings.BallsSettings[ballNumber].Color);
            }
        }

        private bool IsWithinAimingLimits(Vector3 point, out Vector3 direction) {
            direction = Vector3.zero;
            Vector3 thisPosition = _ballSpawner.transform.position;
            Vector3 dir = point - thisPosition;
            dir.y = 0;
            bool result = Vector3.Angle(_ballSpawner.transform.forward, dir) <= (_limitAngle / 2);
            if (result) {
                direction = dir.normalized;
            }
            return result;
        }


        private Vector3? GetWallPoint(Vector3 dir) {
            return Physics.SphereCast(
                _ballSpawner.transform.position,
                _ballRadius,
                dir,
                out var hitInfo,
                float.MaxValue,
                _hintRayCollisionMask,
                QueryTriggerInteraction.Ignore
            )
                ? hitInfo.point
                : null;
        }

        private void PlaySound(AudioClip sfx) {
            if (_audioSource == null || sfx == null) {
                return;
            }
            _audioSource.PlayOneShot(sfx, 1f);
        }

        private void DrawHintCircle(Vector3 center) {
            if (_hintCircleRenderer == null) {
                return;
            }
            for (int i = 0; i < HintCirclePointsCount; ++i) {
                float angleDelta = 2 * Mathf.PI / HintCirclePointsCount;
                Vector3 radialOffset = new Vector3(Mathf.Cos(i * angleDelta), 0, Mathf.Sin(i * angleDelta));
                _hintCircleRenderer.SetPosition(i, center + radialOffset * _ballRadius);
            }
        }

        private void HideDummy() {
            _dummyBall.gameObject.SetActive(false);
        }
    }
}
