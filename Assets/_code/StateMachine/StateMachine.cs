using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using System.Threading;


namespace Sergei.Safonov.StateMachine {

    /// <summary>
    /// Default implementation of <see cref="IStateMachine"/> interface.
    /// </summary>
    public class StateMachine : IStateMachine {

        public IObservable<(string, string)> OnStateTransitionStarted => _onTransitionStarted.AsSystemObservable();
        private readonly Subject<(string, string)> _onTransitionStarted = new();
        public IObservable<(string, string, bool)> OnStateTransitionFinished => _onTransitionFinished.AsSystemObservable();
        private readonly Subject<(string, string, bool)> _onTransitionFinished = new();
        public IObservable<string> OnStateEntered => _onStateEntered.AsSystemObservable();
        private readonly Subject<string> _onStateEntered = new();
        public IObservable<string> OnStateExited => _onStateExited.AsSystemObservable();
        private readonly Subject<string> _onStateExited = new();


        private readonly Dictionary<Type, IState> _states = new();
        private IState _currentState = null;

        private bool _started;


        public virtual void RegisterState<T>(T state) where T : class, IState {
            ThrowIfStarted();
            Type type = typeof(T);
            if (_states.ContainsKey(type)) {
                throw new InvalidOperationException($"Already contains handler for state {type}");
            }
            _states[type] = state;
        }

        public async UniTask StartAsync<T>(Action<T> stateSetupAction = null, CancellationToken cancelToken = default)
            where T : class, IState {
            // There can be thrown an exception but we do not handle it because the whole responsibility for the
            // state machine configuration is on it's client.

            ThrowIfStarted();
            _started = true;

            Type type = typeof(T);
            IState nextState = GetStateByType(type);

            stateSetupAction?.Invoke(nextState as T);

            while (nextState != null && !cancelToken.IsCancellationRequested) {
                type = await StartStateAsync(nextState, cancelToken);
                nextState = GetStateByType(type);
            }
        }

        private async UniTask<Type> StartStateAsync(IState state, CancellationToken cancelToken = default) {

            string oldStateId = _currentState != null ? _currentState.StateId : null;
            string newStateId = state.StateId;
            _onTransitionStarted.OnNext((oldStateId, newStateId));
            bool success = false;
            try {
                if (_currentState != null) {
                    await _currentState.OnStateExitAsync();
                    _onStateExited.OnNext(oldStateId);
                }
                await state.OnStateEnterAsync();
                _onStateEntered.OnNext(newStateId);
                success = true;
            } catch (Exception ex) {
                UnityEngine.Debug.LogError($"Could not switch to {state.StateId}, {ex}");
                throw;
            } finally {
                _onTransitionFinished.OnNext((oldStateId, newStateId, success));
            }

            _currentState = state;

            return await state.StartAsync(cancelToken);
        }


        private IState GetStateByType(Type type) {
            if (type == null) {
                return null;
            }
            if (!_states.TryGetValue(type, out var nextState)) {
                throw new InvalidOperationException($"State {type} was not registered!");
            }

            return nextState;
        }

        private void ThrowIfStarted() {
            if (_started) {
                throw new InvalidOperationException("The state machine is already running.");
            }
        }
    }
}
