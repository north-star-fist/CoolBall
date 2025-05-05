using Sergei.Safonov.Persistence;
using UnityEngine;
using VContainer;

namespace Coolball.Configuration {
    [CreateAssetMenu(fileName = "Save Managers Installer", menuName = "Cool Ball/DI Installers/Save Managers")]
    public class SaveManagersInstaller : AScriptableInstaller {
        [SerializeField]
        private string _dataFolder = "game_data";

        public override void Install(IContainerBuilder builder) {
            // General save manager
            builder.Register<JsonFileSaveManager>(Lifetime.Singleton).WithParameter(_dataFolder).As<ISaveManager>();
        }

        private void OnValidate() {
            // TODO: verify the folder name
        }
    }
}
