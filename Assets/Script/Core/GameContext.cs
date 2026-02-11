using UnityEngine;

public static class GameContext
{
    // Here, we store the level selected by the player in the menu, so that we can retrieve it in the GridManager to initialize the grid with the right data.
    public static LevelData SelectedLevel;
}
