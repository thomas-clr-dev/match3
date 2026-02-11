using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour, IGameService
{
    private int _currentScore = 0;
    private int _movesLeft;
    private LevelData _data;

    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    private void Start()
    {
        // Retriving level data via GridManager
        GridManager grid = ServiceLocator.Get<GridManager>();
        _data = grid.levelData;
        _movesLeft = _data.moveLimit;

        // Subscribing
        GameEvents.OnScoreChanged += AddScore;
        GameEvents.OnMovesChanged += UseMove;

        UpdateUI();
    }

    private void OnDestroy()
    {
        GameEvents.OnScoreChanged -= AddScore;
        GameEvents.OnMovesChanged -= UseMove;
    }

    private void AddScore(int amount)
    {
        _currentScore += amount;
        CheckWinCondition();
        UpdateUI();
    }


    private void UseMove(int amount) // amount in this case is usually -1
    {
        _movesLeft += amount;
        CheckLoseCondition();
        UpdateUI();
    }

    private void CheckWinCondition()
    {
        if (_currentScore >= _data.targetScore)
        {
            Utils.ColorLog("VICORY !", "green");
            GameEvents.OnLevelEnded?.Invoke(true);
            GameEvents.OnInputLocked?.Invoke(); // Game End
            Time.timeScale = 0f; // Freeze the game
        }
    }
    private void CheckLoseCondition()
    {
        if (_movesLeft <= 0 && _currentScore < _data.targetScore)
        {
            Utils.ColorLog("DEFEAT !", "red");
            GameEvents.OnLevelEnded?.Invoke(false);
            GameEvents.OnInputLocked?.Invoke(); // Game End
            Time.timeScale = 0f; // Freeze the game
        }
    }

    private void UpdateUI()
    {
        // Ici tu pourrais mettre à jour tes textes TMPro si tu en avais
        // Exemple : scoreText.text = _currentScore.ToString();
    }
}
