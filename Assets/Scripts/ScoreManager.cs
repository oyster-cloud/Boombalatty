using TMPro;
using UnityEngine;
using System;

// What it does: Tracks and displays the current score and high score, and exposes events for score changes.
// What it's used for: Central scoring authority used by UI, settings, and gameplay systems to show and persist player progress.
public class ScoreManager : MonoBehaviour
{
  public static ScoreManager Instance { get; private set; }

  [SerializeField] TMP_Text scoreText;
  int score;

  // --- NEW: public access + high score + events ---
  public int CurrentScore => score;
  public int HighScore { get; private set; }
  public event Action<int> OnScoreChanged;
  public event Action<int> OnHighScoreChanged;
  const string PrefKeyHighScore = "HighScore";

  // What it does: Enforces a singleton instance, loads the saved high score, and resets the current score.
  // What it's used for: Ensures one persistent ScoreManager exists and initializes score state on startup.
  void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject); // <-- Allows Settings panel to work across restarts

    HighScore = PlayerPrefs.GetInt(PrefKeyHighScore, 0);
    ResetScore();
  }

  // What it does: Subscribes to boomba offscreen events to update score.
  // What it's used for: Listens for last-variant boombas leaving the screen and awards points when they do.
  void OnEnable()  => BoombaAutoDespawn.OnBoombaOffscreen += HandleOffscreen;

  // What it does: Unsubscribes from boomba offscreen events when disabled.
  // What it's used for: Prevents event leaks if the ScoreManager is ever disabled or destroyed.
  void OnDisable() => BoombaAutoDespawn.OnBoombaOffscreen -= HandleOffscreen;

  // What it does: Increments score when a last-variant boomba goes offscreen, updates UI, and checks high score.
  // What it's used for: Implements the rule that only final-variant boombas contribute to the player's score.
  void HandleOffscreen(BoombaProperties props)
  {
    if (props != null && props.IsLastVariant)
    {
      score++;
      UpdateUI();
      OnScoreChanged?.Invoke(score);
      TryUpdateHighScore();
    }
  }

  // What it does: Refreshes the on-screen score label based on the current score.
  // What it's used for: Keeps the HUD text in sync with the internal score value.
  void UpdateUI()
  {
    if (scoreText) scoreText.text = $"{score}";
  }

  // What it does: Resets the current score to zero and updates UI and listeners.
  // What it's used for: Called on new runs or restarts to clear the previous score.
  public void ResetScore()
  {
    score = 0;
    UpdateUI();
    OnScoreChanged?.Invoke(score);
  }

  // What it does: Compares the current score to the stored high score and updates/saves if higher.
  // What it's used for: Maintains and persists the best score across runs and sessions.
  void TryUpdateHighScore()
  {
    if (score > HighScore)
    {
      HighScore = score;
      PlayerPrefs.SetInt(PrefKeyHighScore, HighScore);
      OnHighScoreChanged?.Invoke(HighScore);
    }
  }

  // What it does: Clears the saved high score and notifies listeners of the change.
// What it's used for: Used by the settings UI to allow the player to reset their high score.
  public void ResetHighScore()
  {
    HighScore = 0;
    PlayerPrefs.SetInt(PrefKeyHighScore, 0);
    OnHighScoreChanged?.Invoke(HighScore);
  }
}
