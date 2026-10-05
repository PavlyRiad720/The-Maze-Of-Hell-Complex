using UnityEngine;

public interface IHackMinigame
{
    void InitializeMinigame(System.Action onSolveSuccess, System.Action onSolveFailed);
    void ResetMinigame();
}
