using UnityEngine;
using System.Collections;

// What it does: Displays a stage transition message when the player progresses to a new difficulty level.
// What it's used for: Shows visual feedback between whale completion and the next round starting.
public class StageTransitionUI
{
  private readonly GameObject _transitionPanel;
  private readonly MonoBehaviour _coroutineRunner;
  private readonly TMPro.TextMeshProUGUI _stageText;

  public StageTransitionUI(GameObject transitionPanel, MonoBehaviour coroutineRunner)
  {
    _transitionPanel = transitionPanel;
    _coroutineRunner = coroutineRunner;
    
    if (_transitionPanel)
    {
      _stageText = _transitionPanel.GetComponentInChildren<TMPro.TextMeshProUGUI>();
    }
  }

  // What it does: Shows a stage transition message for the specified duration.
  // What it's used for: Displays "Stage 2" (or similar) between rounds with a fade animation.
  public IEnumerator ShowStageTransition(int stageNumber, float duration = 2f)
  {
    if (!_transitionPanel)
    {
      Debug.LogWarning("StageTransitionUI: transitionPanel not assigned.");
      yield break;
    }

    // Set the text
    if (_stageText)
    {
      _stageText.text = $"Level {stageNumber}";
    }

    _transitionPanel.SetActive(true);

    // Get or add CanvasGroup for fading
    var cg = _transitionPanel.GetComponent<CanvasGroup>();
    if (!cg) cg = _transitionPanel.AddComponent<CanvasGroup>();

    // Fade in
    yield return FadeCanvasGroup(cg, 0f, 1f, 0.3f);
    
    // Hold
    yield return new WaitForSecondsRealtime(duration - 0.6f); // Total duration minus fade times
    
    // Fade out
    yield return FadeCanvasGroup(cg, 1f, 0f, 0.3f);

    _transitionPanel.SetActive(false);
  }

  // What it does: Lerps a CanvasGroup's alpha over time using unscaled deltaTime.
  // What it's used for: Smoothly fades the stage transition in and out.
  private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
  {
    float t = 0f;
    cg.alpha = from;
    
    while (t < duration)
    {
      t += Time.unscaledDeltaTime;
      float k = Mathf.Clamp01(t / duration);
      cg.alpha = Mathf.Lerp(from, to, k);
      yield return null;
    }
    
    cg.alpha = to;
  }
}
