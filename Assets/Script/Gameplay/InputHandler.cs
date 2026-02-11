using System;
using UnityEngine;

public class InputHandler : MonoBehaviour
{

    private Piece _selectedPiece;
    private bool _isLocked = false;

    private void OnEnable()
    {
        // Subscribe to event to block clicks
        GameEvents.OnInputLocked += LockInput;
        GameEvents.OnInputUnlocked += UnlockInput;
    }

    private void OnDisable()
    {
        // Subscribe to event to block clicks
        GameEvents.OnInputLocked -= LockInput;
        GameEvents.OnInputUnlocked -= UnlockInput;
    }

    void LockInput() => _isLocked = true;
    void UnlockInput() => _isLocked = false;

    void Update()
    {
        if (_isLocked) return;

        if (Input.GetMouseButtonDown(0)) // LMB
        {
            // Convert mouse position in 2D
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                Piece clickedPiece = hit.collider.GetComponent<Piece>();
                if (clickedPiece != null)
                {
                    ProcessClick(clickedPiece);
                }
            }
        }
}

    private void ProcessClick(Piece clickedPiece)
    {
        if (_selectedPiece == null)
        {
            // First click : Selection
            _selectedPiece = clickedPiece;
            // Small visual feedback (get the piece bigger)
            _selectedPiece.transform.localScale = Vector3.one * 1.2f;
        }
        else
        {
            // Second click : Action try
            _selectedPiece.transform.localScale = Vector3.one; // Reset tio normal size

            if (IsAdjacent(_selectedPiece, clickedPiece))
            {
                // It's valid, we call Chief (GridManager)
                GridManager grid = ServiceLocator.Get<GridManager>();
                grid.RequestSwap(_selectedPiece, clickedPiece);
            }

            // In all case, we unselect after second click
            _selectedPiece = null;
        }
    }

    bool IsAdjacent(Piece a, Piece b)
    {
        // Distance should be exactly 1 (no diagonal)
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
    }
}