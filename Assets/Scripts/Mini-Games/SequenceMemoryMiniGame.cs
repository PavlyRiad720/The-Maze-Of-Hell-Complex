using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static IHackMinigame;

public class SequenceMemoryMiniGame : MonoBehaviour, IHackMinigame
{
    [Header("UI and Sequence Setup")]
    public List<Button> sequenceButtons;
    public List<Image> buttonImages;
    public TextMeshProUGUI statusText;
    public Color normalColor = Color.gray;
    public Color flashColor = Color.cyan;
    public Color successColor = Color.green;
    public Color errorColor = Color.red;

    [Header("Difficulty Config")]
    public int sequenceLength = 4;
    public float flashDuration = 0.4f;
    public float pauseDuration = 0.2f;

    private List<int> generatedSequence = new List<int>();
    private int playerInputIndex = 0;
    private bool isPatternPlaying = false;

    private System.Action onSuccess;
    private System.Action onFailure;

    public void InitializeMinigame(System.Action onSolveSuccess, System.Action onSolveFailed)
    {
        onSuccess = onSolveSuccess;
        onFailure = onSolveFailed;

        for (int i = 0; i < sequenceButtons.Count; i++)
        {
            int index = i;
            sequenceButtons[i].onClick.RemoveAllListeners();
            sequenceButtons[i].onClick.AddListener(() => OnButtonClicked(index));
        }

        ResetMinigame();
    }

    public void ResetMinigame()
    {
        StopAllCoroutines();
        generatedSequence.Clear();
        playerInputIndex = 0;
        isPatternPlaying = false;

        for (int i = 0; i < sequenceLength; i++)
        {
            generatedSequence.Add(Random.Range(0, sequenceButtons.Count));
        }

        StartCoroutine(PlayPatternRoutine());
    }

    private IEnumerator PlayPatternRoutine()
    {
        isPatternPlaying = true;
        if (statusText != null) statusText.text = "MEMORIZE SEQUENCE...";

        yield return new WaitForSeconds(0.5f);

        foreach (int btnIndex in generatedSequence)
        {
            yield return FlashButton(btnIndex, flashColor, flashDuration);
            yield return new WaitForSeconds(pauseDuration);
        }

        isPatternPlaying = false;
        playerInputIndex = 0;
        if (statusText != null) statusText.text = "REPEAT SEQUENCE";
    }

    private void OnButtonClicked(int buttonIndex)
    {
        if (isPatternPlaying) return;

        if (buttonIndex == generatedSequence[playerInputIndex])
        {
            StartCoroutine(FlashButton(buttonIndex, successColor, 0.2f));
            playerInputIndex++;

            if (playerInputIndex >= generatedSequence.Count)
            {
                if (statusText != null) statusText.text = "OVERRIDE ACCEPTED!";
                onSuccess?.Invoke();
            }
        }
        else
        {
            StartCoroutine(FlashButton(buttonIndex, errorColor, 0.4f));
            if (statusText != null) statusText.text = "SEQUENCE ERROR! RETRYING...";
            onFailure?.Invoke();
            ResetMinigame();
        }
    }
    private IEnumerator FlashButton(int index, Color color, float duration)
    {
        if (index < 0 || index >= buttonImages.Count) yield break;

        buttonImages[index].color = color;
        yield return new WaitForSeconds(duration);
        buttonImages[index].color = normalColor;
    }

}
