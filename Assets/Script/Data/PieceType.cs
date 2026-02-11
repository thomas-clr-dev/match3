using UnityEngine;

[CreateAssetMenu(fileName = "PieceType", menuName = "Match3/PieceType")]
public class PieceType : ScriptableObject
{
    public string id;            // Ex : "Red"
    public Sprite visual;        // Ex : Square sprite
    public int pointsValue = 10; // Score given when destroy
}
