using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject _introPanel;
    [SerializeField] private GameObject _gamePanel;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _dynamicCanvas;

    [SerializeField] private TMP_Text _introPanelText;
    [SerializeField] private GameConfig _gameConfig;

    // Reusable StringBuilder to avoid allocations
    private StringBuilder _textBuilder = new StringBuilder();

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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted += ShowGame;
            GameManager.Instance.OnPlayAgain += ShowIntro;
        }
        
        if (CollectionManager.Instance != null)
        {
            CollectionManager.Instance.OnGameOver += ShowGameOver;
        }

        PopulateIntroText();
        
        ShowIntro();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted -= ShowGame;
            GameManager.Instance.OnPlayAgain -= ShowIntro;
        }
        
        if (CollectionManager.Instance != null)
        {
            CollectionManager.Instance.OnGameOver -= ShowGameOver;
        }
    }

    private void PopulateIntroText()
    {
        if (_introPanelText == null || _gameConfig == null) return;

        _textBuilder.Clear();
        _textBuilder.Append($"Merge and collect {_gameConfig.itemsToWin} to win\n(demo)");

        _introPanelText.text = _textBuilder.ToString();
    }
}

