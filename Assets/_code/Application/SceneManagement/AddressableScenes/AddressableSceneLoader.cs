#if ADDRESSABLE_SCENES
using Cysharp.Threading.Tasks;
using ProtoTerminatar.Util;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Sergei.Safonov.SceneManagement {
    public class AddressableSceneLoader : ISceneLoader {

        private AsyncOperationHandle<SceneInstance> _loadedSceneOperation;
        private readonly AssetReference _sceneRef;

        private bool _isLoaded;
        private Scene _loadedScene;

        public AddressableSceneLoader(AssetReference sceneRef) {
            _sceneRef = sceneRef;
        }

        public async UniTask<(bool loadedSuccessfully, Scene scene)> LoadAsync(bool forceReload = false) {
            if (_isLoaded && !forceReload) {
                return (true, _loadedScene);
            }
            await UnloadAsync();
            (bool loadedSuccessfully, Scene scene) = await Load();
            return (loadedSuccessfully, scene);
        }

        public async UniTask UnloadAsync() {
            if (!_isLoaded) {
                return;
            }
            _loadedScene = default;
            if (_loadedSceneOperation.IsValid()) {
                await Addressables.UnloadSceneAsync(_loadedSceneOperation);
            }
            _isLoaded = false;
        }


        private async UniTask<(bool loadedSuccessfully, Scene scene)> Load() {
            _loadedSceneOperation = Addressables.LoadSceneAsync(_sceneRef, LoadSceneMode.Additive);
            SceneInstance sceneInstance = await _loadedSceneOperation;
            _isLoaded = true;
            _loadedScene = sceneInstance.Scene;
            return (true, sceneInstance.Scene);
        }
    }
}
#endif
