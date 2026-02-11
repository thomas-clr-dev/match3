using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour, IGameService
{

    public LevelData levelData;
    public GameObject piecePrefab;

    [Header("Animation Duration")]
    public float fallTime;
    public float swapTime;
    public float brainTime; // Delay for the griod manager to wait for the next thing to happen

    private Piece[,] _grid;

    private bool _isGameEnded = false;

    private void Awake()
    {
        ServiceLocator.Register(this);

        if (GameContext.SelectedLevel != null)
        {
            levelData = GameContext.SelectedLevel;
        }
    }

    // 2. ABONNE-TOI À L'ÉVÉNEMENT DE FIN DE JEU
    void OnEnable()
    {
        GameEvents.OnLevelEnded += OnGameEnded;
    }

    void OnDisable()
    {
        GameEvents.OnLevelEnded -= OnGameEnded;
    }

    private void OnGameEnded(bool win)
    {
        _isGameEnded = true;
    }

    private void Start()
    {
        InitializeGrid();
        AdaptCameraToGrid();
    }

    void InitializeGrid()
    {
        _grid = new Piece[levelData.columns, levelData.rows];

        for (int x = 0; x < levelData.columns; x++)
        {
            for (int y = 0; y < levelData.rows; y++)
            {
                // Search a type that doesn't create a Match
                PieceType type = GetTypeWithoutMatch(x, y);
                SpawnPiece(x, y, type);
            }
        }
    }

    PieceType GetTypeWithoutMatch(int x, int y)
    {
        List<PieceType> possibilities = new List<PieceType>(levelData.availableTypes);

        // If 2 on the left are identic, we remove this type
        if (x >= 2 && _grid[x - 1, y].type == _grid[x - 2, y].type)
            possibilities.Remove(_grid[x - 1, y].type);

        // If 2 on the bottom are identic, we remove this type
        if (y >= 2 && _grid[x, y - 1].type == _grid[x, y - 2].type)
            possibilities.Remove(_grid[x, y - 1].type);

        return possibilities[Random.Range(0, possibilities.Count)];
    }

    void SpawnPiece(int x, int y, PieceType type)
    {
        GameObject obj = Instantiate(piecePrefab, new Vector3(x, y, 0), Quaternion.identity, transform);
        Piece p = obj.GetComponent<Piece>();
        p.Setup(x, y, type);
        _grid[x, y] = p;
    }

    // --- GAME CORE : THE SWAP ---
    
    public void RequestSwap(Piece a, Piece b)
    {
        StartCoroutine(SwapRoutine(a, b));
    }

    IEnumerator SwapRoutine(Piece a, Piece b)
    {
        GameEvents.OnInputLocked?.Invoke(); //Lock the player

        // 1. Visual Swap
        yield return StartCoroutine(SwapPositionsAnimated(a, b));

        // 2. Logic swap in array
        SwapIndices(a, b);

        // 3. Verification
        List<Piece> matches = FindMatches();

        if (matches.Count > 0)
        {
            // Success : treating the match
            GameEvents.OnMovesChanged?.Invoke(-1);
            yield return StartCoroutine(ProcessMatches(matches));
        }
        else
        {
            // Failure : Canceling all
            SwapIndices(a, b); // Cancel logic
            yield return StartCoroutine(SwapPositionsAnimated(a, b)); // Cancel Visual
        }

        if (!_isGameEnded)
        {
            GameEvents.OnInputUnlocked?.Invoke(); // Unlock the player
        }
    }

    IEnumerator SwapPositionsAnimated(Piece a, Piece b)
    {
        Vector3 posA = a.transform.position;
        Vector3 posB = b.transform.position;

        // Launch both animation in parallel
        StartCoroutine(a.MoveTo(posB, 0.2f));
        yield return StartCoroutine(b.MoveTo(posA, swapTime));
    }

    void SwapIndices(Piece a, Piece b)
    {
        Piece temp = _grid[a.x, a.y];
        _grid[a.x, a.y] = b;
        _grid[b.x, b.y] = temp;

        int tempX = a.x; int tempY = a.y;
        a.x = b.x; a.y = b.y;
        b.x = tempX; b.y = tempY;
    }

    // --- MATCH LOGIC ---

    List<Piece> FindMatches()
    {
        HashSet<Piece> matchedPieces = new HashSet<Piece>();

        // Horizontal
        for (int y = 0; y < levelData.rows; y++)
        {
            for (int x = 0; x < levelData.columns - 2; x++) 
            {
                Piece p1 = _grid[x, y], p2 = _grid[x + 1, y], p3 = _grid[x + 2, y];
                if (p1 == null || p2 == null || p3 == null) continue; 
                if (p1.type == null || p2.type == null || p3.type == null) continue;
                if (p1.type == p2.type && p2.type == p3.type)
                {
                    matchedPieces.Add(p1); matchedPieces.Add(p2); matchedPieces.Add(p3);
                }
            }
        }

        // Vertical
        for (int x = 0; x < levelData.columns; x++) 
        {
            for (int y = 0; y < levelData.rows - 2; y++)
            {
                Piece p1 = _grid[x, y], p2 = _grid[x, y + 1], p3 = _grid[x, y + 2];
                if (p1 == null || p2 == null || p3 == null) continue;  
                if (p1.type == null || p2.type == null || p3.type == null) continue;
                if (p1.type == p2.type && p2.type == p3.type)
                {
                    matchedPieces.Add(p1); matchedPieces.Add(p2); matchedPieces.Add(p3);
                }
            }
        }
        return new List<Piece>(matchedPieces);
    }

    IEnumerator ProcessMatches(List<Piece> matches)
    {
        // 1. Score calcul and Destroy
        int score = 0;
        foreach (Piece p in matches)
        {
            score += p.type.pointsValue;
            _grid[p.x, p.y] = null; // Empty the logic cell
            if (p != null) Destroy(p.gameObject); // Destroy the Unity Object
        }
        GameEvents.OnScoreChanged?.Invoke(score);
        yield return new WaitForSeconds(0.1f);

        // 2. Gravity (fall of pieces)
        yield return StartCoroutine(ApplyGravity());

        // 3. Refill
        yield return StartCoroutine(RefillGrid());

        // 4. Recursivity (Does the new pieces make a match ?)
        List<Piece> newMatches = FindMatches();
        if (newMatches.Count > 0)
        {
            yield return StartCoroutine(ProcessMatches(newMatches));
        }
    }

    IEnumerator ApplyGravity()
    {
        for (int x = 0; x < levelData.columns; x++)
        {
            for (int y = 0; y < levelData.rows; y++)
            {
                if (_grid[x, y] == null) // Hole found
                {
                    for (int k = y + 1; k < levelData.rows; k++)
                    {
                        if (_grid[x, k] != null)
                        {
                            // Moving the k piece toward the hole y
                            _grid[x, y] = _grid[x, k];
                            _grid[x, k] = null;
                            _grid[x, y].y = y; // Coordinate update
                            StartCoroutine(_grid[x, y].MoveTo(new Vector3(x, y, 0), fallTime));
                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(brainTime);
    }

    IEnumerator RefillGrid()
    {
        for (int x = 0; x < levelData.columns; x++)
        {
            for (int y =0; y < levelData.rows; y++)
            {
                if (_grid[x, y] == null)
                {
                    PieceType type = levelData.availableTypes[Random.Range(0, levelData.availableTypes.Count)];
                    SpawnPiece(x, y, type);

                    // Animation : they spawn on top (y + 5) and fall
                    _grid[x, y].transform.position = new Vector3(x, y + 5, 0);
                    StartCoroutine(_grid[x, y].MoveTo(new Vector3(x, y, 0), fallTime));
                }
            }
        }
        yield return new WaitForSeconds(brainTime);
    }

    void AdaptCameraToGrid()
    {
        // 1. PLACER LA CAMÉRA AU CENTRE
        // Le centre est toujours : (NbColonnes - 1) / 2
        float xCenter = (levelData.columns - 1) / 2f;
        float yCenter = (levelData.rows - 1) / 2f;

        // On décale un peu le Y vers le haut ou le bas si tu as beaucoup d'UI (Score/Menu)
        // Ici je le laisse centré
        Camera.main.transform.position = new Vector3(xCenter, yCenter, -10f);

        // 2. CALCULER LE ZOOM (Orthographic Size)

        // Ratio de l'écran (Largeur / Hauteur)
        float screenRatio = (float)Screen.width / (float)Screen.height;

        // Taille nécessaire pour la hauteur (Rows / 2 car orthographicSize est une demi-hauteur)
        float targetHeight = levelData.rows / 2f;

        // Taille nécessaire pour la largeur (Columns / 2 / Ratio)
        float targetWidth = (levelData.columns / 2f) / screenRatio;

        // On prend le maximum des deux pour être sûr que tout rentre (Largeur ET Hauteur)
        float requiredSize = Mathf.Max(targetHeight, targetWidth);

        // 3. AJOUTER UNE MARGE (PADDING)
        // Ajoute +1.5f ou +2f pour laisser de la place sur les bords pour ton UI (Score, etc.)
        float padding = 2.0f;

        Camera.main.orthographicSize = requiredSize + padding;
    }
}
