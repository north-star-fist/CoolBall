using Coolball.Configuration;
using R3;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Coolball {

    /// <summary>
    /// Game UI Manager (responsible for the game level UI).
    /// </summary>
    /// <seealso cref="GameView"/>
    public class GameUiManager : MonoBehaviour, IGameUiManager {
        [Header("Game UI")]
        [SerializeField] private Button _shuffleButton;
        [SerializeField] private Button _changeBallButton;
        [SerializeField] private TMP_Text _changeBallButtonText;


        [Header("Top Buttons")]
        [SerializeField] private Button _helpButton;
        [SerializeField] private Button _soundMenuButton;
        [SerializeField] private Button _soundToggleButton;
        [SerializeField] private Button _restartButton;

        [Header("Game Over")]
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _maxScoreText;
        [SerializeField] private TMP_Text _topBallsEliminatedText;
        [SerializeField] private Button _gameOverRestartButton;

        [Header("Sound")]
        [SerializeField] private GameObject _soundMenuPanel;
        [SerializeField] private Slider _masterSlider;
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;

        [Header("Help")]
        [SerializeField] private GameObject _tutorialPanel;

        [Header("Other")]
        [SerializeField]
        private int _topBallBwSpriteIndex = 0;

        [Inject]
        private ISettingsManager _settingsManager;

        private Settings _currentSettings;
        private IReadOnlyDictionary<int, int> _spriteIndices;

        public IObservable<bool> ShuffleClicked => _shuffleButton.OnClickAsObservable().Select(_ => true)
            .AsSystemObservable();

        public IObservable<bool> ChangeBallClicked => _changeBallButton.OnClickAsObservable().Select(_ => true)
            .AsSystemObservable();

        public IObservable<bool> RestartClicked => _restartButton.OnClickAsObservable()
            .Merge(_gameOverRestartButton.OnClickAsObservable()).Select(_ => true)
            .AsSystemObservable();

        private void Awake() {
            if (_helpButton != null) {
                _helpButton.OnClickAsObservable().Subscribe(HandleTutorialToggle).AddTo(this);
            }
            if (_soundMenuButton != null) {
                _soundMenuButton.OnClickAsObservable().Subscribe(HandleSoundMenuToggle).AddTo(this);
            }
            if (_soundToggleButton != null) {
                _soundToggleButton.OnClickAsObservable().Subscribe(_ => AudioListener.volume = 1 - AudioListener.volume)
                    .AddTo(this);
            }
        }

        private void Start() {
            _currentSettings = _settingsManager.GetCurrentGameSettings();

            //Just for simplicity
            if (_masterSlider != null) {
                _masterSlider.value = _currentSettings.MasterVolume;
                _masterSlider.onValueChanged.AsObservable().Subscribe(SetMasterVolume).AddTo(this);
            }

            if (_sfxSlider != null) {
                _sfxSlider.value = _currentSettings.SfxVolume;
                _sfxSlider.onValueChanged.AsObservable().Subscribe(SetSfxVolume).AddTo(this);
            }

            if (_musicSlider != null) {
                _musicSlider.value = _currentSettings.MusicVolume;
                _musicSlider.onValueChanged.AsObservable().Subscribe(SetMusicVolume).AddTo(this);
            }
            if (_gameOverPanel != null) {
                _gameOverPanel.SetActive(false);
            }
        }

        public void Init(IReadOnlyDictionary<int, int> spriteIndices) {
            _spriteIndices = spriteIndices;
        }

        public void OnGameStarted(int maxScore, uint shotsLeft) {
            _gameOverPanel.gameObject.SetActive(false);
            SetMaxScore(maxScore);
            SetScore(0);
            SetTopBallsEliminated(0);
        }

        public void OnGameOver() {
            _gameOverPanel.gameObject.SetActive(true);
        }

        public void SetScore(int score) {
            if (_scoreText != null) {
                _scoreText.text = $"Score: {score.ToString()}";
            }
        }

        public void SetMaxScore(int maxScore) {
            if (_maxScoreText != null) {
                _maxScoreText.text = $"Best: {maxScore.ToString()}";
            }
        }

        public void SetTopBallsEliminated(int eights) {
            if (_topBallsEliminatedText != null) {
                _topBallsEliminatedText.text = $"<sprite={_topBallBwSpriteIndex.ToString()}>: {eights.ToString()}";
            }
        }

        public void EnableBallChanging() {
            if (_changeBallButton != null) {
                _changeBallButton.interactable = true;
            }
        }

        public void DisableBallChanging() {
            if (_changeBallButton != null) {
                _changeBallButton.interactable = false;
            }
            if (_changeBallButtonText != null) {
                _changeBallButtonText.text = "x";
            }
        }

        public void SetChangeBall(int ball) {
            if (_changeBallButtonText != null) {
                _changeBallButtonText.text = $"<sprite={_spriteIndices[ball].ToString()}>";
            }
        }


        public void SetGameSettings(Settings settings) {
            _settingsManager.ActivateSettings(settings);
            _settingsManager.SaveSettings(settings);
        }


        private void HandleTutorialToggle(Unit _) {
            if (_tutorialPanel == null) {
                return;
            }
            _tutorialPanel.gameObject.SetActive(!_tutorialPanel.gameObject.activeSelf);
        }

        private void HandleSoundMenuToggle(Unit _) {
            if (_soundMenuPanel == null) {
                return;
            }
            _soundMenuPanel.gameObject.SetActive(!_soundMenuPanel.gameObject.activeSelf);
        }


        private void SetMusicVolume(float volume) {
            Settings settings = GetCurrentGameSettings();
            settings.MusicVolume = volume;
            SetGameSettings(settings);
        }

        private void SetSfxVolume(float volume) {
            Settings settings = GetCurrentGameSettings();
            settings.SfxVolume = volume;
            SetGameSettings(settings);
        }

        private void SetMasterVolume(float volume) {
            Settings settings = GetCurrentGameSettings();
            settings.MasterVolume = volume;
            SetGameSettings(settings);
        }

        private Settings GetCurrentGameSettings() {
            return _currentSettings == null ? _settingsManager.GetCurrentGameSettings() : _currentSettings;
        }
    }
}
