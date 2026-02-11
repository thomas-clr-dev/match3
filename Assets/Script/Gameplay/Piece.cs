using UnityEngine;
using System.Collections;

public class Piece : MonoBehaviour
{

    public int x;
    public int y;
    public PieceType type;

    private SpriteRenderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    public void Setup(int x, int y,  PieceType type)
    {
        this.x = x;
        this.y = y;
        this.type = type;
        _renderer.sprite = type.visual;

        // Name the object to facilitate the hierarchy debug
        gameObject.name = $"Piece_{x}_{y}";
    }

    public IEnumerator MoveTo(Vector3 targetPos, float duration)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0;

        while (elapsed < duration)
        {
            // Critic Security : If the object is destroy, we break
            if (this == null) yield break;

            // Lerp = Linear Interpolation (fluid movevement)
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed);
            elapsed += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        if (this != null) transform.position = targetPos;
    }

}
