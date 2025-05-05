using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Sergei.Safonov.SceneManagement {

    public interface ISceneManager {
        UniTask<(bool loadedSuccessfully, Scene scene)> LoadAsync(string sceneKey, bool forceReload = false);

        UniTask UnloadAsync(string sceneKey);
    }
}
