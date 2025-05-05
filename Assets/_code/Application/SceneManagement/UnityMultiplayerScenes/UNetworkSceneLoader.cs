#if UNITY_NETCODE_SCENES
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Sergei.Safonov.SceneManagement
{
    public class UNetworkSceneLoader : ISceneLoader
    {
        private readonly string _sceneName;
        private Scene _loadedScene;
        private bool _loaded = false;
        private bool _unloaded = true;

        public UNetworkSceneLoader(string sceneName)
        {
            _sceneName = sceneName;
        }

        public async Awaitable UnloadAsync()
        {
            if (_loadedScene == null || !_loadedScene.IsValid())
            {
                return;
            }
            try
            {
                NetworkManager.Singleton.SceneManager.OnUnloadEventCompleted += handleUnloadingEnding;
                var lStatus = NetworkManager.Singleton.SceneManager.UnloadScene(_loadedScene);
                if (lStatus != SceneEventProgressStatus.Started)
                {
                    return;
                }
                while (!_unloaded)
                {
                    await Awaitable.EndOfFrameAsync();
                }
            }
            finally
            {
                NetworkManager.Singleton.SceneManager.OnUnloadEventCompleted -= handleUnloadingEnding;
            }

            void handleUnloadingEnding(
                string sceneName,
                LoadSceneMode loadSceneMode,
                List<ulong> clientsCompleted,
                List<ulong> clientsTimedOut
            )
            {
                if (sceneName != _sceneName)
                {
                    return;
                }
                var scene = SceneManager.GetSceneByName(_sceneName);
                if (!scene.isLoaded)
                {
                    _unloaded = true;
                }
            }
        }

        public async Awaitable<(bool loadedSuccessfully, Scene scene)> LoadAsync(bool forceReload = false)
        {
            if (_loaded && !forceReload)
            {
                return (true, _loadedScene);
            }
            await UnloadAsync();
            try
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += handleLoadingEnding;
                var lStatus = NetworkManager.Singleton.SceneManager.LoadScene(_sceneName, LoadSceneMode.Additive);
                if (lStatus != SceneEventProgressStatus.Started)
                {
                    return (false, default);
                }
                while (!_loaded)
                {
                    await Awaitable.EndOfFrameAsync();
                }
                if (
                    _loadedScene != null
                    && _loadedScene.IsValid())
                {
                    return (true, _loadedScene);
                }
                else
                {
                    await UnloadAsync();
                }
                return (false, default);
            }
            finally
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= handleLoadingEnding;
            }

            void handleLoadingEnding(
                string sceneName,
                LoadSceneMode loadSceneMode,
                List<ulong> clientsCompleted,
                List<ulong> clientsTimedOut
            )
            {
                if (sceneName != _sceneName)
                {
                    return;
                }
                var scene = SceneManager.GetSceneByName(_sceneName);
                if (scene.IsValid())
                {
                    _loadedScene = scene;
                    _loaded = true;
                }
            }
        }
    }
}
#endif
