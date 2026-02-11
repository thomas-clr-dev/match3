using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputSettings;

public class GameUIManager : MonoBehaviour
{
    [Header("HUD")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI movesText;

    [Header("EndScreen")]
    public GameObject endPanel;
    public TextMeshProUGUI endTitle;
    public TextMeshProUGUI finalScoreText;
    public Image panelBackground;

    private int _score = 0;
    private int _moves = 0;

    private void OnEnable()
    {
        GameEvents.OnScoreChanged += UpdateScore;
        GameEvents.OnMovesChanged += UpdateMoves;
        GameEvents.OnLevelEnded += ShowEndScreen;
    }

    private void OnDisable()
    {
        GameEvents.OnScoreChanged -= UpdateScore;
        GameEvents.OnMovesChanged -= UpdateMoves;
        GameEvents.OnLevelEnded -= ShowEndScreen;
    }

    private void Start()
    {
        Utils.SafeSetActive(endPanel, false);

        // Init texts with level value
        GridManager grid = ServiceLocator.Get<GridManager>();
        if (grid != null)
        {
            _moves = grid.levelData.moveLimit;
            movesText.text = $"Moves : {_moves}";
            scoreText.text = "Score : 00000";
        }
    }

    void UpdateScore(int newPoints)
    {
        _score += newPoints;
        scoreText.text = $"Score : {_score}";
    }

    void UpdateMoves(int usedMove)
    {
        _moves += usedMove;
        movesText.text = $"Moves : {_moves}";
    }

    void ShowEndScreen(bool isWin)
    {
        Utils.SafeSetActive(endPanel, true);

        if (isWin)
        {
            endTitle.text = "VICTORY !";
            endTitle.color = Color.green;
            if (panelBackground) panelBackground.color = new Color(0, 0, 0, 0.9f);
        }
        else
        {
            endTitle.text = "DEFEAT...";
            endTitle.color = Color.red;
            if (panelBackground) panelBackground.color = new Color(0.2f, 0, 0, 0.9f);
        }

        finalScoreText.text = $"Final Score : {_score}\n Move Left : {_moves}";
    }

    // --- BUTTONS ---

    public void OnRestartButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
