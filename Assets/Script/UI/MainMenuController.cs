using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class MainMenuController : MonoBehaviour
{
    [Header("Configuration")]
    public List<LevelData> levels;
    public string gameSceneName = "GameScene";

    [Header("UI References")]
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI gridSizeText;

    private int _currentIndex = 0;

    private void Start()
    {
        UpdateUI();
    }

    public void OnArrowRight()
    {
        _currentIndex++;
        // If go further than the end, go start
        if (_currentIndex >= levels.Count) _currentIndex = 0;
        UpdateUI();
    }

    public void OnArrowLeft()
    {
        _currentIndex--;
        // If before the start, go end
        if (_currentIndex < 0) _currentIndex = levels.Count - 1;
        UpdateUI();
    }

    private void UpdateUI()
    {
        LevelData current = levels[_currentIndex];
        levelNameText.text = current.name;
        gridSizeText.text = $"{current.columns} x {current.rows}";
    }

    public void OnPlayButton()
    {
        // Save the level choice
        GameContext.SelectedLevel = levels[_currentIndex];

        // Load game scene
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnQuitButton()
    {
        Utils.ColorLog("Quitter le jeu", "purple");
        Application.Quit();
    }
}
