using Coolball.Configuration;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Coolball.Flow {

    /// <summary>
    /// Starts the application registering <see cref="AppEntryPoint"/> through
    /// <see cref="VContainer.IContainerBuilder"/>.
    /// By the way in goes through provided
    /// <see cref="AScriptableInstaller"/>s and launches them.
    /// </summary>
    public class AppStarter : LifetimeScope {

        [SerializeField]
        private AScriptableInstaller[] _diInstallers;

        protected override void Configure(IContainerBuilder builder) {
            base.Configure(builder);
            if (_diInstallers != null) {
                foreach (var installer in _diInstallers) {
                    installer.Install(builder);
                }
            }
            builder.RegisterEntryPoint<AppEntryPoint>();
        }
    }
}
