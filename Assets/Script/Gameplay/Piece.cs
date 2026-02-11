using UnityEngine;
using System.Collections;

public class Piece : MonoBehaviour
{
    public int x;
    public int y;
    public PieceType type;

    private SpriteRenderer _renderer;
    private Coroutine _currentMoveCoroutine; // On garde une référence de l'animation en cours

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    public void Setup(int x, int y, PieceType type)
    {
        this.x = x;
        this.y = y;
        this.type = type;
        if (_renderer != null && type != null) _renderer.sprite = type.visual;
        gameObject.name = $"Piece_{x}_{y}";
    }

    public IEnumerator MoveTo(Vector3 targetPos, float duration)
    {
        // 1. Si une animation est DÉJÀ en cours, on l'arrête net !
        if (_currentMoveCoroutine != null) StopCoroutine(_currentMoveCoroutine);

        // 2. On lance la nouvelle animation
        _currentMoveCoroutine = StartCoroutine(MoveRoutine(targetPos, duration));
        yield return _currentMoveCoroutine;
    }

    private IEnumerator MoveRoutine(Vector3 targetPos, float duration)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0;

        while (elapsed < duration)
        {
            if (this == null) yield break;

            // Interpolation fluide
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (this != null) transform.position = targetPos;
        _currentMoveCoroutine = null; // Animation finie, on libère la référence
    }
}