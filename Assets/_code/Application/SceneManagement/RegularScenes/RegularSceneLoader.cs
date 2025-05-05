using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sergei.Safonov.SceneManagement {

    public class RegularSceneLoader : ISceneLoader {
        private readonly string _sceneName;

        private AsyncOperation _loadOp;
        private Scene _loadedScene;

        public RegularSceneLoader(string sceneName) {
            _sceneName = sceneName;
        }

        public async UniTask<(bool loadedSuccessfully, Scene scene)> LoadAsync(bool forceReload = false) {
            if (_loadedScene != default && !forceReload) {
                return (true, _loadedScene);
            }
            await UnloadAsync();
            if (_loadOp == null) {
                _loadOp = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
            }

            await _loadOp;
            _loadOp = null;
            _loadedScene = SceneManager.GetSceneByName(_sceneName);
            return (_loadedScene.IsValid(), _loadedScene);
        }

        public async UniTask UnloadAsync() {
            if (_loadOp != null) {
                await _loadOp;
                _loadOp = null;
            }
            if (_loadedScene != default) {
                await SceneManager.UnloadSceneAsync(_sceneName);
            }
            _loadedScene = default;
        }
    }
}
