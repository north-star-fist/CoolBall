using UnityEngine;
using VContainer;

namespace Coolball.Configuration {
    public abstract class AScriptableInstaller : ScriptableObject {
        public abstract void Install(IContainerBuilder builder);
    }
}
