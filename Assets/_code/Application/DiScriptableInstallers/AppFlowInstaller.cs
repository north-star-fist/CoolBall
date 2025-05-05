using Coolball.Flow;
using Sergei.Safonov.SceneManagement;
using UnityEngine;
using VContainer;

namespace Coolball.Configuration {

    /// <summary>
    /// Installer that configures <see cref="IAppFlow"/> and registers it in DI container.
    /// </summary>
    [CreateAssetMenu(menuName = "Cool Ball/DI Installers/AppFlow", fileName = "AppFlow Installer")]
    public class AppFlowInstaller : AScriptableInstaller {
        public override void Install(IContainerBuilder builder) {
            builder.Register<AppFlow>(Lifetime.Scoped).As<IAppFlow>();

            builder.RegisterBuildCallback(container => {
                // Creating App State Machine when DI Context is ready
                var appFlow = container.Resolve<IAppFlow>();
                appFlow.RegisterState(new AppStateBoot());
                var sceneManager = container.Resolve<ISceneManager>();
                AppStateGame gameState = new AppStateGame(sceneManager);
                appFlow.RegisterState(gameState);
            });
        }
    }
}
