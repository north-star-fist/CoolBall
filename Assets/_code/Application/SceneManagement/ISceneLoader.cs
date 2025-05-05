using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sergei.Safonov.SceneManagement {

    public interface ISceneLoader {
        UniTask<(bool loadedSuccessfully, Scene scene)> LoadAsync(bool forceReload = false);
        UniTask UnloadAsync();
    }
}
