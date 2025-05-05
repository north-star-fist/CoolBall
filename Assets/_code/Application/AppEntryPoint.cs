using Cysharp.Threading.Tasks;
using System.Threading;
using VContainer;
using VContainer.Unity;

namespace Coolball.Flow {

    /// <summary>
    /// Entry point of the application logic (See VContainer docs).
    /// </summary>
    public class AppEntryPoint : IAsyncStartable {

        private readonly IAppFlow _appStateService;

        [Inject]
        public AppEntryPoint(IAppFlow appStateService) {
            _appStateService = appStateService;
        }

        public async UniTask StartAsync(CancellationToken cancellation = default) {
            await _appStateService.StartAsync<AppStateBoot>(null, cancellation);
        }
    }
}
