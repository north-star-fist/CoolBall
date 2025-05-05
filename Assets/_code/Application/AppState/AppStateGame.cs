using Coolball.Configuration;
using Cysharp.Threading.Tasks;
using R3;
using Sergei.Safonov.SceneManagement;
using Sergei.Safonov.StateMachine;
using Sergei.Safonov.Unity;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


namespace Coolball.Flow {

    public class AppStateGame : IState {

        private const string SceneKeyGameplay = "Gameplay";
        private const float GameOverDelay = 5f;

        private readonly ISceneManager _sceneManager;

        private IGameView _lvlMap;

        private IGameUiManager _uiManager;

        private int _currentLevel;

        private DisposableBag _disposables = new DisposableBag();
        private LevelSO _level;
        private readonly ReactiveProperty<uint> _ballsLeft = new();

        /// <summary>
        /// Max Score (non-persistent for simplicity).
        /// </summary>
        private int _maxScore;
        private int _score;

        // Currently loaded ball. Null if no ball loaded.
        private int? _ball;
        // Ball that we get when press 'Change ball' button.
        private int? _swapBall;

        public string StateId => nameof(AppStateGame);

        private Observable<Unit> _gameOverTicker => _ballsLeft.Debounce(TimeSpan.FromSeconds(GameOverDelay))
            .Where(b => b == 0)
            .Select(_ => Unit.Default);

        public AppStateGame(ISceneManager sceneManager) {
            _sceneManager = sceneManager;
        }

        public void SetUp(int currentLevel) {
            _currentLevel = currentLevel;
        }

        public async UniTask OnStateEnterAsync() {
            _disposables.Dispose();
            _disposables = new();

            _level = Resources.Load<LevelSO>($"levels/level_{_currentLevel + 1}");
            var lResult = await _sceneManager.LoadAsync(_level.LevelSceneKyey);
            var gResult = await _sceneManager.LoadAsync(SceneKeyGameplay);

            if (!lResult.loadedSuccessfully || !lResult.scene.TryGetRootGameObject<IGameView>(out _lvlMap)) {
                throw new InvalidOperationException(
                    $"Level {_currentLevel} was not loaded! Maybe there is no {nameof(GameView)} object."
                );
            }
            if (!gResult.loadedSuccessfully
                || !gResult.scene.TryGetRootGameObject<IGameUiManager>(out _uiManager)
                ) {
                throw new InvalidOperationException(
                    $"Game manager scene was not loaded! Maybe there is no {nameof(IGameUiManager)} object."
                );
            }

            Dictionary<int, int> spriteIndices = new();
            for (int i = 0; i < _level.BallsSettings.Count; ++i) {
                spriteIndices.Add(i, _level.BallsSettings[i].SpriteIndex);
            }
            _uiManager.Init(spriteIndices);
            _uiManager.ShuffleClicked.ToObservable().Subscribe(Shuffle).AddTo(ref _disposables);
            _uiManager.ChangeBallClicked.ToObservable().Subscribe(ChangeBall).AddTo(ref _disposables);
            _uiManager.RestartClicked.ToObservable().Subscribe(_ => Restart()).AddTo(ref _disposables);

            _lvlMap.Setup(_level);
            _lvlMap.BallEliminated.ToObservable().Subscribe(ballData => HandleBallElimination(ballData)).AddTo(ref _disposables);
            _lvlMap.TopBallEliminated.ToObservable().Subscribe(eights => UpdateTopScore(eights)).AddTo(ref _disposables);
            _lvlMap.OnShot.ToObservable().Subscribe(_ => HandleShot()).AddTo(ref _disposables);
            _ballsLeft.Subscribe(HandleBallsCount).AddTo(ref _disposables);

            _gameOverTicker.Subscribe(_ => HandleGameOver()).AddTo(ref _disposables);

            _ballsLeft.Value = _level.MaxShots;
            Time.timeScale = 1f;

            return;
        }

        private void Restart() {
            _uiManager.OnGameStarted(_maxScore, _level.MaxShots);

            _score = 0;
            _ballsLeft.Value = _level.MaxShots;
            _lvlMap.OnGameStarted();
            _ball = GetNextBall();
            _lvlMap.LoadBall(_ball.Value);
            _swapBall = GetChangeBall();
            _uiManager.SetChangeBall(_swapBall.Value);
        }

        private void HandleShot() {
            if (_ballsLeft.Value == 0) {
                Debug.LogWarning("Shot happened with no balls!");
                return;
            }
            _ball = null;
            _ballsLeft.Value--;
            if (_ballsLeft.Value > 0) {
                _swapBall = GetChangeBall();
                _uiManager.EnableBallChanging();
                _uiManager.SetChangeBall(_swapBall.Value);
            } else {
                _swapBall = null;
                _uiManager.DisableBallChanging();
            }
        }

        private void HandleBallsCount(uint balls) {
            _lvlMap.SetBallsLeft(balls, _level.MaxShots);
            if (balls > 0 && !_ball.HasValue) {
                _ball = GetNextBall();
                _lvlMap.LoadBall(_ball.Value);
            }
        }

        private void HandleGameOver() {
            _uiManager.OnGameOver();
            _lvlMap.OnGameOver();
            if (_maxScore < _score) {
                _maxScore = _score;
            }
        }

        private void HandleBallElimination((int, Vector3) ballData) {
            (int ballNum, Vector3 ballPosition) = ballData;

            // -1 ball from the table
            _ballsLeft.Value += (IsTopBall(ballNum)) ? 2u : 1u;
            if (_ballsLeft.Value > 0 && !_swapBall.HasValue) {
                _swapBall = GetChangeBall();
                _uiManager.EnableBallChanging();
                _uiManager.SetChangeBall(_swapBall.Value);
            }
            _score += _level.BallsSettings[ballNum].Score;
            _uiManager.SetScore(_score);
            _lvlMap.SetScore(_score);
        }

        private void UpdateTopScore(int eights) {
            _uiManager.SetTopBallsEliminated(eights);
            _lvlMap.SetEights(eights);
        }

        public async UniTask OnStateExitAsync() {
            await _sceneManager.UnloadAsync(SceneKeyGameplay);
            _disposables.Dispose();
        }

        public async UniTask<Type> StartAsync(CancellationToken cancelToken) {
            _lvlMap.OnGameStarted();
            _ball = GetNextBall();
            _lvlMap.LoadBall(_ball.Value);
            _swapBall = GetChangeBall();
            _uiManager.EnableBallChanging();
            _uiManager.SetChangeBall(_swapBall.Value);

            while (!cancelToken.IsCancellationRequested) {
                await UniTask.WaitForFixedUpdate();
            }
            return null;
        }


        /// <summary>
        /// Instant action or caching selected action index.
        /// </summary>
        /// <param name="actionInd"> action index </param>
        private void Shuffle(bool _) {
            _lvlMap.Shuffle();
        }

        private void ChangeBall(bool _) {
            if (!_swapBall.HasValue) {
                return;
            }
            _lvlMap.LoadBall(_swapBall.Value, true);
            _swapBall = GetChangeBall();
            _uiManager.SetChangeBall(_swapBall.Value);
        }

        private int GetNextBall() {
            return UnityEngine.Random.Range(_level.NewBallsRange.x, _level.NewBallsRange.y + 1);
        }

        private int GetChangeBall() {
            return UnityEngine.Random.Range(_level.ChangeBallsRange.x, _level.ChangeBallsRange.y + 1);
        }

        private bool IsTopBall(int ballNum) => ballNum == _level.BallsSettings.Count - 1;
    }
}
