using Coolball.Configuration;
using System;
using UnityEngine;

namespace Coolball {

    /// <summary>
    /// Game view interface (responsible for the game level scene).
    /// </summary>
    /// <seealso cref="IGameUiManager"/>
    public interface IGameView {

        /// <summary>
        /// Ball is shot event.
        /// </summary>
        public IObservable<bool> OnShot { get; }

        /// <summary>
        /// Event that is raised when a ball eliminated with it's number (starting from 0) and coordinates.
        /// Note when top ball is eliminated it's actually means that two balls are eliminated (but event is only one).
        /// </summary>
        public IObservable<(int, Vector3)> BallEliminated { get; }

        /// <summary>
        /// Count of eliminated top balls.
        /// </summary>
        public IObservable<int> TopBallEliminated { get; }

        /// <summary>
        /// Sets game settings according specified level.
        /// </summary>
        // TODO: rewrite it with explicit settings not SO
        public void Setup(LevelSO level);

        /// <summary>
        /// Updates the view on game started.
        /// </summary>
        void OnGameStarted();

        /// <summary>
        /// Updates the view on game is over.
        /// </summary>
        void OnGameOver();

        /// <summary>
        /// Loads a ball to shoot.
        /// </summary>
        /// <param name="ball"> the ball number </param>
        /// <param name="manually"> pass true if the ball is changed by 'Change' button </param>
        public void LoadBall(int ball, bool manually = false);

        /// <summary>
        /// Shuffle balls on the table (kick em all).
        /// </summary>
        public void Shuffle();

        /// <summary>
        /// Sets current score.
        /// </summary>
        public void SetScore(int score);

        /// <summary>
        /// Sets current number of eliminated top balls.
        /// </summary>
        public void SetEights(int eights);

        /// <summary>
        /// Sets number of balls/shots left.
        /// </summary>
        /// <param name="balls"> balls left (number of shots allowed) </param>
        /// <param name="maxBalls"> max balls allowed on the table </param>
        public void SetBallsLeft(uint balls, uint maxBalls);
    }
}
