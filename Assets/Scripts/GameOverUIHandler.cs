using UnityEngine;
using System.Collections;

// What it does: Handles all game-over UI animations and transitions.
// What it's used for: Manages fading the game-over panel in and out smoothly.
public class GameOverUIHandler
{
  private readonly GameObject _gameOverPanel;
  private readonly MonoBehaviour _coroutineRunner;

  public GameOverUIHandler(GameObject gameOverPanel, MonoBehaviour coroutineRunner)
  {
    _gameOverPanel = gameOverPanel;
    _coroutineRunner = coroutineRunner;
  }

  // What it does: Fades in the game-over panel and enables interaction after fade completes.
  // What it's used for: Shows the game-over screen smoothly when the player loses.
  public void ShowGameOverPanel(float duration = 0.35f)
  {
    if (!_gameOverPanel)
    {
      Debug.LogWarning("GameOverUIHandler: gameOverPanel not assigned.");
      return;
    }

    _gameOverPanel.SetActive(true);

    var cg = _gameOverPanel.GetComponent<CanvasGroup>();
    if (!cg) cg = _gameOverPanel.AddComponent<CanvasGroup>();

    cg.interactable = false;
    cg.blocksRaycasts = false;
    
    _coroutineRunner.StartCoroutine(FadeCanvasGroup(cg, 0f, 1f, duration, () =>
    {
      cg.interactable = true;
      cg.blocksRaycasts = true;
    }));
  }

  // What it does: Fades out the game-over panel and disables it after fade completes.
  // What it's used for: Hides the game-over screen smoothly when restarting.
  public void HideGameOverPanel(float duration = 0.2f)
  {
    if (!_gameOverPanel) return;

    var cg = _gameOverPanel.GetComponent<CanvasGroup>() ?? _gameOverPanel.AddComponent<CanvasGroup>();
    cg.interactable = false;
    cg.blocksRaycasts = true; // Keep blocking during fade so clicks don't slip through

    _coroutineRunner.StartCoroutine(FadeCanvasGroup(cg, cg.alpha, 0f, duration, () =>
    {
      cg.blocksRaycasts = false;
      _gameOverPanel.SetActive(false);
    }));
  }

  // What it does: Lerps a CanvasGroup's alpha over time using unscaled deltaTime, then calls an optional callback.
  // What it's used for: Drives both fade-in and fade-out transitions for the game-over panel.
  private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration, System.Action onDone = null)
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
    onDone?.Invoke();
  }
}
