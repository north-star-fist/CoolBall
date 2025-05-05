using System;
using System.Collections.Generic;

namespace Coolball {

    /// <summary>
    /// Game UI Manager interface (responsible for the game UI).
    /// </summary>
    /// <seealso cref="GameView"/>
    public interface IGameUiManager {

        /// <summary>
        /// Is raised when user clicks 'Shuffle' button.
        /// </summary>
        public IObservable<bool> ShuffleClicked { get; }

        /// <summary>
        /// Is raised when user clicks 'Change Ball' button.
        /// </summary>
        public IObservable<bool> ChangeBallClicked { get; }

        /// <summary>
        /// Is raised when user wants to restart.
        /// </summary>
        public IObservable<bool> RestartClicked { get; }

        public void Init(IReadOnlyDictionary<int, int> spriteIndices);

        /// <summary>
        /// Sets current game score.
        /// </summary>
        public void SetScore(int score);

        /// <summary>
        /// Sets Max/Best Score.
        /// </summary>
        public void SetMaxScore(int maxScore);

        /// <summary>
        /// Sets number of eliminated top balls.
        /// </summary>
        public void SetTopBallsEliminated(int topBallsEliminated);

        /// <summary>
        /// Sets ball which with the current one can be changed.
        /// </summary>
        public void SetChangeBall(int ball);

        /// <summary>
        /// Enable ball changing UI.
        /// </summary>
        public void EnableBallChanging();
        /// <summary>
        /// Disable ball changing UI.
        /// </summary>
        public void DisableBallChanging();

        /// <summary>
        /// Updates UI on game start.
        /// </summary>
        public void OnGameStarted(int maxScore, uint ballsLeft);

        /// <summary>
        /// Updates UI on Game Over event.
        /// </summary>
        void OnGameOver();
    }
}
