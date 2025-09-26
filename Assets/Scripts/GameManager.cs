using UnityEngine;

public class GameManager : MonoBehaviour
{
  [Header("What to disable when game ends")]
  [SerializeField] MonoBehaviour[] toDisable;   // e.g., BoombaSpawner, BoombaTouchSpawner

  [Header("UI")]
  [SerializeField] GameObject gameOverPanel;
  [SerializeField] bool pauseOnEnd = true;

  bool ended = false;

  void Awake()
  {
    if (gameOverPanel) gameOverPanel.SetActive(false);
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
    EndGame();
  }

  public void EndGame()
  {
    Debug.Log("EndGame");

    if (ended) return;
    ended = true;

    if (toDisable != null)
      for (int i = 0; i < toDisable.Length; i++)
        if (toDisable[i]) toDisable[i].enabled = false;

    if (gameOverPanel) gameOverPanel.SetActive(true);
    if (pauseOnEnd) Time.timeScale = 0f;
  }

  public void Restart()
  {
    if (toDisable != null)
      for (int i = 0; i < toDisable.Length; i++)
        if (toDisable[i]) toDisable[i].enabled = true;

    if (gameOverPanel) gameOverPanel.SetActive(false);
    if (pauseOnEnd) Time.timeScale = 1f;

    ended = false;
  }
}
