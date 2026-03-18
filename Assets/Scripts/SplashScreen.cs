using UnityEngine;
using System.Collections;

public class SplashScreen : MonoBehaviour
{
  [Header("Settings")]
  [SerializeField] GameObject splashPanel;
  [SerializeField] float displayDuration = 2f;
  [SerializeField] float fadeOutDuration = 0.5f;

  public static event System.Action OnSplashComplete; // Add this event

  void Start()
  {
    StartCoroutine(ShowSplashThenStart());
  }

  IEnumerator ShowSplashThenStart()
  {
    if (!splashPanel)
    {
      Debug.LogWarning("SplashScreen: splashPanel not assigned!");
      OnSplashComplete?.Invoke(); // Fire event even if no panel
      yield break;
    }

    splashPanel.SetActive(true);

    var cg = splashPanel.GetComponent<CanvasGroup>();
    if (!cg) cg = splashPanel.AddComponent<CanvasGroup>();

    cg.alpha = 1f;
    cg.interactable = false;
    cg.blocksRaycasts = true;

    yield return new WaitForSeconds(displayDuration);

    // Fade out
    float elapsed = 0f;
    while (elapsed < fadeOutDuration)
    {
      elapsed += Time.deltaTime;
      cg.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
      yield return null;
    }

    cg.alpha = 0f;
    cg.blocksRaycasts = false;
    splashPanel.SetActive(false);

    // Fire event when splash is complete
    OnSplashComplete?.Invoke();
  }
}