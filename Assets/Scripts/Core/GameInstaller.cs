using Zenject;
using UnityEngine;
using MergeGame.Systems;
using MergeGame.Grid;
using MergeGame.Data;
using MergeGame.Entities;

namespace MergeGame.Core
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private GameConfig _gameConfig;
        [SerializeField] private AudioManager _audioManagerPrefab;
        [SerializeField] private GameManager _gameManagerPrefab;
        [SerializeField] private GridManager _gridManagerPrefab;
        [SerializeField] private CollectionManager _collectionManagerPrefab;
        [SerializeField] private ItemManager _itemManagerPrefab;
        [SerializeField] private SpawnerManager _spawnerManagerPrefab;
        [SerializeField] private EnergyManager _energyManagerPrefab;

        public override void InstallBindings()
        {
            if (_gameConfig == null) throw new System.Exception("GameInstaller: _gameConfig is not assigned.");
            if (_audioManagerPrefab == null) throw new System.Exception("GameInstaller: _audioManagerPrefab is not assigned.");
            if (_gameManagerPrefab == null) throw new System.Exception("GameInstaller: _gameManagerPrefab is not assigned.");
            if (_gridManagerPrefab == null) throw new System.Exception("GameInstaller: _gridManagerPrefab is not assigned.");
            if (_collectionManagerPrefab == null) throw new System.Exception("GameInstaller: _collectionManagerPrefab is not assigned.");
            if (_itemManagerPrefab == null) throw new System.Exception("GameInstaller: _itemManagerPrefab is not assigned.");
            if (_spawnerManagerPrefab == null) throw new System.Exception("GameInstaller: _spawnerManagerPrefab is not assigned.");
            if (_energyManagerPrefab == null) throw new System.Exception("GameInstaller: _energyManagerPrefab is not assigned.");


            // Bind GameConfig as a singleton
            Container.BindInstance(_gameConfig).AsSingle();

            // Bind managers as singletons
            Container.Bind<AudioManager>().FromComponentInNewPrefab(_audioManagerPrefab).AsSingle();
            Container.Bind<GameManager>().FromComponentInNewPrefab(_gameManagerPrefab).AsSingle();
            Container.Bind<CollectionManager>().FromComponentInNewPrefab(_collectionManagerPrefab).AsSingle();
            Container.Bind<GridManager>().FromComponentInNewPrefab(_gridManagerPrefab).AsSingle();
            Container.Bind<ItemManager>().FromComponentInNewPrefab(_itemManagerPrefab).AsSingle();
            Container.Bind<SpawnerManager>().FromComponentInNewPrefab(_spawnerManagerPrefab).AsSingle();
            Container.Bind<EnergyManager>().FromComponentInNewPrefab(_energyManagerPrefab).AsSingle();
        }
    }
}