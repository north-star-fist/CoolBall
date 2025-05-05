using Sergei.Safonov.SceneManagement;
using System;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AddressableAssets;

using VContainer;

namespace Coolball.Configuration {

    [CreateAssetMenu(
        fileName = "Scene Manager Installer",
        menuName = "Cool Ball/DI Installers/Scene Manager Installer")
    ]
    public class SceneManagerInstaller : AScriptableInstaller {

        [SerializeField]
        private List<AddressableScene> _addressableScenes = new();


        public override void Install(IContainerBuilder builder) {
            Func<string, ISceneLoader> defaultFactory = (string key) => new RegularSceneLoader(key);
            Dictionary<string, ISceneLoader> factoryMap = new();

            foreach (var addrScene in _addressableScenes) {
                factoryMap[addrScene.SceneKey] = new AddressableSceneLoader(addrScene.SceneReference);
            }


            ConfigurableSceneManager sceneManager = new ConfigurableSceneManager(defaultFactory, factoryMap);
            builder.RegisterInstance(sceneManager).AsImplementedInterfaces();
        }


        [Serializable]
        public class AddressableScene {
            public string SceneKey;

            [AssetReferenceUILabelRestriction(AddressablesTags.Scene)]
            public AssetReference SceneReference;
        }
    }
}
