using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace Sergei.Safonov.StateMachine {
    public interface IState {
        /// <summary>
        /// State Id for events.
        /// </summary>
        public string StateId { get; }

        /// <summary>
        /// The method that is executed on state machine entering the state. When the method ends it means
        /// that transition to the state is over and now the state machine is in this state.
        /// </summary>
        /// <seealso cref="StartAsync"/>
        public UniTask OnStateEnterAsync();

        /// <summary>
        /// The method that is executed on state machine exiting the state.
        /// </summary>
        public UniTask OnStateExitAsync();

        /// <summary>
        /// The method is invoked after <see cref="OnStateEnterAsync()"/> method finished.
        /// This method can run eternally. When it finishes it means that state machine should go
        /// to another state. So this method returns this next state.
        /// </summary>
        public UniTask<Type> StartAsync(CancellationToken cancelToken);
    }
}
