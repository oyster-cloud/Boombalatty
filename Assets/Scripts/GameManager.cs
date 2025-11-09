using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
  public static GameManager Instance { get; private set; }

  [Header("What to disable when game ends")]
  [SerializeField] MonoBehaviour[] toDisable;   // e.g., BoombaSpawner, BoombaTouchSpawner

  [Header("UI")]
  [SerializeField] GameObject gameOverPanel;
  [SerializeField] bool pauseOnEnd = true;

  [SerializeField] BoombaSpawner boombaSpawner;
  [SerializeField] SnackTouchSpawner snackTouchSpawner;

  public bool IsGameOver { get; private set; } = false;

  void Awake()
  {
    if (Instance != null && Instance != this) {
      Destroy(gameObject); return;
    }

    Instance = this;

    DontDestroyOnLoad(gameObject);

    if (gameOverPanel)
      gameOverPanel.SetActive(false);

    Time.timeScale = 1f;
  }

  // Call this to hide any GameObject after N seconds (unscaled time)
  public void HideAfterSecondsRealtime(GameObject target, float seconds)
  {
    StartCoroutine(HideRoutine(target, seconds));
  }

  IEnumerator HideRoutine(GameObject target, float seconds)
  {
    yield return new WaitForSecondsRealtime(seconds);
    if (target) target.SetActive(false);
  }

  void OnEnable()
  {
    EndGameWhenBoombaCrossesCeiling.OnBoombaCrossedCeiling += HandleCeilingCross;
  }

  void OnDisable()
  {
    EndGameWhenBoombaCrossesCeiling.OnBoombaCrossedCeiling -= HandleCeilingCross;
  }

  void HandleCeilingCross(BoombaProperties who)
  {
    Debug.Log("HandleCeilingCross" + who);
    TriggerGameOver();
  }

  public void Restart()
  {
    // I don't think I need pauseOnEnd
    if (pauseOnEnd) Time.timeScale = 1f;

    // 2) Start fading out the panel (keep raycasts ENABLED during fade so clicks don’t slip through)
    if (gameOverPanel)
    {
      var cg = gameOverPanel.GetComponent<CanvasGroup>() ?? gameOverPanel.AddComponent<CanvasGroup>();
      cg.interactable = false;
      cg.blocksRaycasts = true; // keep blocking while fading so the user can’t click through
      StartCoroutine(FadeCanvasGroup(cg, cg.alpha, 0f, 0.2f, () =>
      {
        cg.blocksRaycasts = false;
        gameOverPanel.SetActive(false);
      }));
    }

    // Hide Game Over UI
    // if (gameOverPanel) gameOverPanel.SetActive(false);

    // Do I need this? Am I disabling anything?
    if (toDisable != null)
      for (int i = 0; i < toDisable.Length; i++)
        if (toDisable[i]) toDisable[i].enabled = true;

    // Clear the board (Boombas + Snacks + any held snack)
    ClearBoard();

    Services.Ceiling?.ResetListening();

    // Prefer registry (latest scene instances)
    var boomba = Services.BoombaSpawner;
    var snack  = Services.SnackTouchSpawner;

    // Kick off fresh run
    if (boomba != null) boomba.ResetAndStartInitialSpawn();
    if (snack  != null) snack.ResetHeldAndArm();

    IsGameOver = false;

    // if (gameOverPanel) gameOverPanel.SetActive(false);
    // if (pauseOnEnd) Time.timeScale = 1f;

    // if (gameOverPanel)
    // {
    //   var cg = gameOverPanel.GetComponent<CanvasGroup>();
    //   if (!cg) cg = gameOverPanel.AddComponent<CanvasGroup>();
    //   cg.interactable = false;
    //   cg.blocksRaycasts = false;

    //   // quick fade out then hide
    //   StartCoroutine(FadeCanvasGroup(cg, cg.alpha, 0f, 0.2f, () =>
    //   {
    //     gameOverPanel.SetActive(false);
    //   }));
    // }

    // IsGameOver = false;
  }

  // Destroys all dynamic pieces. If you pool, replace Destroy with SetActive(false).
  void ClearBoard()
  {
    // Boombas (anything that has BoombaProperties)
    var boombas = FindObjectsByType<BoombaProperties>(FindObjectsSortMode.None);
    for (int i = 0; i < boombas.Length; i++)
      if (boombas[i]) Destroy(boombas[i].gameObject);

    // Snacks (either by component or by layer names)
    var snacks = FindObjectsByType<SnackLifecycle>(FindObjectsSortMode.None);
    for (int i = 0; i < snacks.Length; i++)
      if (snacks[i]) Destroy(snacks[i].gameObject);

    // Safety: also clear any objects on Snack/SnackPreLand layers if they lack components
    int snack = LayerMask.NameToLayer("Snack");
    int snackPre = LayerMask.NameToLayer("SnackPreLand");
    var all = FindObjectsByType<Transform>(FindObjectsSortMode.None);
    for (int i = 0; i < all.Length; i++)
    {
      var go = all[i].gameObject;
      if (!go || go.scene.rootCount == 0) continue;
      int L = go.layer;
      if (L == snack || L == snackPre) Destroy(go);
    }
  }

  // Call this instead of directly setting panel active in TriggerGameOver
  public void TriggerGameOver()
  {
    Debug.Log("## TriggerGameOver ##");
    if (IsGameOver) return;
    IsGameOver = true;

    if (toDisable != null)
      for (int i = 0; i < toDisable.Length; i++)
        if (toDisable[i]) toDisable[i].enabled = false;

    // Fade in the overlay
    StartFadeInGameOverPanel(0.35f); // 350 ms feels snappy

    if (pauseOnEnd) Time.timeScale = 0f;
  }

  void StartFadeInGameOverPanel(float duration)
  {
    if (!gameOverPanel)
    {
      Debug.LogWarning("GameManager: gameOverPanel not assigned.");
      return;
    }

    gameOverPanel.SetActive(true);

    var cg = gameOverPanel.GetComponent<CanvasGroup>();
    if (!cg) cg = gameOverPanel.AddComponent<CanvasGroup>();

    cg.interactable = false;
    cg.blocksRaycasts = false;
    StartCoroutine(FadeCanvasGroup(cg, 0f, 1f, duration, () =>
    {
      // enable interaction after fade completes
      cg.interactable = true;
      cg.blocksRaycasts = true;
    }));
  }

  IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration, System.Action onDone = null)
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
