using Coolball.Configuration;
using Cysharp.Threading.Tasks;
using Sergei.Safonov.StateMachine;
using System;
using System.Threading;
using VContainer;

namespace Coolball.Flow {

    public class AppStateBoot : IState {

        private ISettingsManager _settingsManager;

        public string StateId => nameof(AppStateBoot);

        [Inject]
        public void Construct(ISettingsManager settings) {
            _settingsManager = settings;
        }


        public UniTask OnStateEnterAsync() => UniTask.CompletedTask;

        public UniTask OnStateExitAsync() => UniTask.CompletedTask;


        public UniTask<Type> StartAsync(CancellationToken cancelToken) {
            _settingsManager.ActivateSettings(_settingsManager.GetCurrentGameSettings());
            return UniTask.FromResult(typeof(AppStateGame));
        }
    }
}
