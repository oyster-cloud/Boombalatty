using TMPro;
using UnityEngine;
using System;                    // <-- add

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

  void Awake()
  {
    Debug.Log("ResetHighScore EXE 4");
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

  void OnEnable()  => BoombaAutoDespawn.OnBoombaOffscreen += HandleOffscreen;
  void OnDisable() => BoombaAutoDespawn.OnBoombaOffscreen -= HandleOffscreen;

  void HandleOffscreen(BoombaProperties props)
  {
    if (props != null && props.IsLastVariant)
    {
      score++;
      UpdateUI();
      OnScoreChanged?.Invoke(score);   // <-- NEW
      TryUpdateHighScore();            // <-- NEW
    }
  }

  void UpdateUI()
  {
    if (scoreText) scoreText.text = $"Score: {score}";
  }

  public void ResetScore()
  {
    score = 0;
    UpdateUI();
    OnScoreChanged?.Invoke(score);     // <-- NEW
  }

  // --- NEW: high score helpers ---
  void TryUpdateHighScore()
  {
    if (score > HighScore)
    {
      HighScore = score;
      PlayerPrefs.SetInt(PrefKeyHighScore, HighScore);
      OnHighScoreChanged?.Invoke(HighScore);
    }
  }

  public void ResetHighScore()
  {
    Debug.Log("ResetHighScore EXE 3");
    HighScore = 0;
    PlayerPrefs.SetInt(PrefKeyHighScore, 0);
    OnHighScoreChanged?.Invoke(HighScore);
  }
}
