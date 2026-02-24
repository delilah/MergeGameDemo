using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using TMPro;
using Zenject;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject _introPanel;
    [SerializeField] private GameObject _gamePanel;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _dynamicCanvas;

    [SerializeField] private TMP_Text _introPanelText;
    
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _playAgainButton;

    private CollectionManager _collectionManager;
    private GameConfig _gameConfig;
    private GameManager _gameManager;

    // Reusable StringBuilder to avoid allocations
    private StringBuilder _textBuilder = new StringBuilder();

    [Inject]
    public void Construct(CollectionManager collectionManager, GameConfig gameConfig, GameManager gameManager)
    {
        _collectionManager = collectionManager;
        _gameConfig = gameConfig;
        _gameManager = gameManager;
    }
    

    public void ShowIntro() 
    { 
        _introPanel.SetActive(true); 
        _gamePanel.SetActive(false); 
        _gameOverPanel.SetActive(false); 
        _dynamicCanvas.SetActive(false); 
    }

    public void ShowGame() 
    { 
        _introPanel.SetActive(false); 
        _gamePanel.SetActive(true); 
        _gameOverPanel.SetActive(false); 
        _dynamicCanvas.SetActive(true); 
    }

    public void ShowGameOver() 
    { 
        _introPanel.SetActive(false); 
        _gamePanel.SetActive(false); 
        _gameOverPanel.SetActive(true); 
        _dynamicCanvas.SetActive(false); 
    }
    private void Start()
    {
        if (_gameManager != null)
        {
            _gameManager.OnGameStarted += ShowGame;
            _gameManager.OnPlayAgain += ShowIntro;
        }
        
        if (_collectionManager != null)
        {
            _collectionManager.OnGameOver += ShowGameOver;
        }

        _startButton.onClick.AddListener(_gameManager.StartGame);
        _playAgainButton.onClick.AddListener(_gameManager.PlayAgain);

        PopulateIntroText();
        
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
            _collectionManager.OnGameOver -= ShowGameOver;
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
}

