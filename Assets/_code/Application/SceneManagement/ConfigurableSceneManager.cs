using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Sergei.Safonov.SceneManagement {

    public class ConfigurableSceneManager : ISceneManager {
        /// <summary>
        /// Active scene loaders.
        /// </summary>
        readonly Dictionary<string, ISceneLoader> _activeSceneLoaders = new();

        private readonly Func<string, ISceneLoader> _defaultFactory;
        readonly Dictionary<string, ISceneLoader> _customLoaders = new();


        public ConfigurableSceneManager(
            Func<string, ISceneLoader> defaultLoaderFactory,
            IReadOnlyDictionary<string, ISceneLoader> customLoaders = null
        ) {
            _defaultFactory = defaultLoaderFactory;
            if (customLoaders != null) {
                foreach ((var k, var v) in customLoaders) {
                    _customLoaders.Add(k, v);
                }
            }
        }

        public async UniTask<(bool loadedSuccessfully, Scene scene)> LoadAsync(string sceneKey, bool forceReload = false) {
            if (!_activeSceneLoaders.TryGetValue(sceneKey, out var loader)) {
                if (_customLoaders.TryGetValue(sceneKey, out loader)) {
                    _activeSceneLoaders.Add(sceneKey, loader);
                } else {
                    loader = _defaultFactory(sceneKey);
                    _activeSceneLoaders.Add(sceneKey, loader);
                }
            }

            return await loader.LoadAsync(forceReload);
        }

        public async UniTask UnloadAsync(string sceneKey) {
            if (_activeSceneLoaders.TryGetValue(sceneKey, out var loader)) {
                await loader.UnloadAsync();
            }
        }
    }
}
