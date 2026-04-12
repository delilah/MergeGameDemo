using UnityEngine;
using System.Text;
using TMPro;
using Zenject;
using UnityEngine.UI;
using MergeGame.Core;
using MergeGame.Systems;
using MergeGame.Data;
 
namespace MergeGame.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject _introPanel;
        [SerializeField] private GameObject _gamePanel;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private GameObject _dynamicCanvas;

        [SerializeField] private TMP_Text _introPanelText;
        
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _playAgainButton;

        [SerializeField] private TMP_Text[] _catNameTexts;
        [SerializeField] private Image[] _catIcons;

        private CatCollectionConfig _catsConfig;

        private CollectionManager _collectionManager;
        private GameConfig _gameConfig;
        private GameManager _gameManager;

        // Reusable StringBuilder to avoid allocations
        private StringBuilder _textBuilder = new StringBuilder();

        [Inject]
        public void Construct(CollectionManager collectionManager, GameConfig gameConfig, GameManager gameManager, CatCollectionConfig catsConfig)
        {
            _collectionManager = collectionManager;
            _gameConfig = gameConfig;
            _gameManager = gameManager;
            _catsConfig = catsConfig;
        }

        /// <summary>
        /// Show the intro panel
        /// </summary>
        public void ShowIntro() 
        { 
            _introPanel.SetActive(true); 
            _gamePanel.SetActive(false); 
            _gameOverPanel.SetActive(false); 
            _dynamicCanvas.SetActive(false); 
        }

        /// <summary>
        /// Show the game panel
        /// </summary>
        public void ShowGame() 
        { 
            _introPanel.SetActive(false); 
            _gamePanel.SetActive(true); 
            _gameOverPanel.SetActive(false); 
            _dynamicCanvas.SetActive(true); 
        }

        /// <summary>
        /// Show the game over panel
        /// </summary>
        public void ShowGameOver() 
        { 
            _introPanel.SetActive(false); 
            _gamePanel.SetActive(false); 
            _gameOverPanel.SetActive(true); 
            _dynamicCanvas.SetActive(false); 
        }

        private void Start()
        {
            if (_startButton == null)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("UIManager: _startButton is not assigned.");
                #endif
            }
            
            if (_playAgainButton == null)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("UIManager: _playAgainButton is not assigned.");
                #endif
            }
            
            // Note: _gameManager and _collectionManager are guarded by Zenject against null
            _gameManager.OnGameStarted += ShowGame;
            _gameManager.OnPlayAgain += ShowIntro;
            _collectionManager.OnWinConditionMet += ShowGameOver;

            _startButton.onClick.AddListener(_gameManager.StartGame);
            _playAgainButton.onClick.AddListener(_gameManager.PlayAgain);

            PopulateIntroText();
            UpdateCatBadges();
            
            ShowIntro();
        }

        private void OnDestroy()
        {
            if (_gameManager != null)
            {
                _gameManager.OnGameStarted -= ShowGame;
                _gameManager.OnPlayAgain -= ShowIntro;
            }
            
            if (_collectionManager != null)
            {
                _collectionManager.OnWinConditionMet -= ShowGameOver;
            }
            
            _startButton.onClick.RemoveListener(_gameManager.StartGame);
            _playAgainButton.onClick.RemoveListener(_gameManager.PlayAgain);
        }

        private void PopulateIntroText()
        {
            if (_introPanelText == null || _gameConfig == null) return;

            _textBuilder.Clear();
            _textBuilder.Append($"Merge and collect {_gameConfig.itemsToWin} to win\n(demo)");

            _introPanelText.text = _textBuilder.ToString();
        }

        private void UpdateCatBadges()
        {
            if (_catsConfig?.trackedItems == null)
            {
                return;
            }

            for (int i = 0; i < _catsConfig.trackedItems.Length; i++)
            {
                if (_catsConfig.trackedItems[i] == null)
                {
                    continue;
                }

                if (i < _catNameTexts.Length && _catNameTexts[i] != null)
                {
                    _catNameTexts[i].text = _catsConfig.trackedItems[i].itemName;
                }

                if (i < _catIcons.Length && _catIcons[i] != null)
                {
                     _catIcons[i].sprite = _catsConfig.trackedItems[i].sprite;
                }
            }
        }
    }
}
