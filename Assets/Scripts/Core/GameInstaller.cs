using Zenject;
using UnityEngine;
using MergeGame.Systems;
using MergeGame.Grid;

namespace MergeGame.Core
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private GameConfig _gameConfig;
        [SerializeField] private AudioManager _audioManagerPrefab;
        [SerializeField] private GameManager _gameManagerPrefab;
        [SerializeField] private GridManager _gridManagerPrefab;
        [SerializeField] private CollectionManager _collectionManagerPrefab;

        public override void InstallBindings()
        {
            // Bind GameConfig as a singleton
            Container.BindInstance(_gameConfig).AsSingle();
            
            // Bind managers as singletons
            Container.Bind<AudioManager>().FromComponentInNewPrefab(_audioManagerPrefab).AsSingle();
            Container.Bind<GameManager>().FromComponentInNewPrefab(_gameManagerPrefab).AsSingle();
            Container.Bind<CollectionManager>().FromComponentInNewPrefab(_collectionManagerPrefab).AsSingle();
            Container.Bind<GridManager>().FromComponentInNewPrefab(_gridManagerPrefab).AsSingle();
        
        }
    }
}
