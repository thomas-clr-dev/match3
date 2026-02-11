using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Match3/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Grid")]
    public int columns = 8;
    public int rows = 8;

    [Header("Goals")]
    public int targetScore = 1000;
    public int moveLimit = 20;

    [Header("Content")]
    public List<PieceType> availableTypes; // Allowed color list
    
}
