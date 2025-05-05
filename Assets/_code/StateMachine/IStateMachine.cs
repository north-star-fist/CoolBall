using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace Sergei.Safonov.StateMachine {

    public interface IStateMachine {
        #region Events
        IObservable<(string, string)> OnStateTransitionStarted { get; }
        IObservable<(string, string, bool)> OnStateTransitionFinished { get; }
        IObservable<string> OnStateEntered { get; }
        IObservable<string> OnStateExited { get; }
        #endregion

        /// <summary>
        /// Registers a state instance corresponding it's type. Must be invoked before a state machine switching to it.
        /// </summary>
        /// <typeparam name="T"> type of state </typeparam>
        /// <param name="state"> state instance </param>
        public void RegisterState<T>(T state) where T : class, IState;

        /// <summary>
        /// Starts the state machine setting it in the initial state.
        /// </summary>
        /// <typeparam name="T"> initial state type </typeparam>
        /// <param name="stateSetupAction"> optional set up action for preparing the state </param>
        /// <param name="cancelToken"> cancellation token that can be used to stop the state machine </param>
        /// <returns></returns>
        public UniTask StartAsync<T>(Action<T> stateSetupAction = null, CancellationToken cancelToken = default)
            where T : class, IState;
    }
}
