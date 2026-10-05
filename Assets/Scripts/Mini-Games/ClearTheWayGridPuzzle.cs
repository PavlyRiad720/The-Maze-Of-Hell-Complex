using UnityEngine;

public class ClearTheWayMinigame : MonoBehaviour, IHackMinigame
{
    [Header("Key Block & Target Exit")]
    public RectTransform keyBlock;
    public RectTransform exitZone;
    public float gridStepSize = 64f; // Tile cell spacing in UI pixels

    private System.Action onSuccess;
    private System.Action onFailure;

    public void InitializeMinigame(System.Action onSolveSuccess, System.Action onSolveFailed)
    {
        onSuccess = onSolveSuccess;
        onFailure = onSolveFailed;
        ResetMinigame();
    }

    public void ResetMinigame()
    {
        // Re-anchor blocks back to puzzle startup positions
    }

    public void MoveBlock(RectTransform block, Vector2 direction)
    {
        // Translate block position along valid grid axis
        block.anchoredPosition += direction * gridStepSize;

        CheckExitCondition();
    }

    private void CheckExitCondition()
    {
        if (keyBlock == null || exitZone == null) return;

        // Verify if key block has reached exit threshold
        if (Vector2.Distance(keyBlock.anchoredPosition, exitZone.anchoredPosition) < gridStepSize * 0.5f)
        {
            Debug.Log("Clear the Way puzzle solved!");
            onSuccess?.Invoke();
        }
    }
}