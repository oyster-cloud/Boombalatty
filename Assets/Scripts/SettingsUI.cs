using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
  [Header("Score UI")]
  [SerializeField] TMP_Text currentScoreLabel;
  [SerializeField] TMP_Text highScoreLabel;
  [SerializeField] UnityEngine.UI.Button resetHighScoreButton;

  [Header("Hook these in Inspector")]
  [SerializeField] GameObject settingsPanel;
  [SerializeField] Button muteButton;
  [SerializeField] TMP_Text muteButtonLabel;

  [Header("Optional: Restart button (wire in Inspector)")]
  [SerializeField] Button restartButton;

  const string PrefKeyMuted = "AudioMuted";
  bool isMuted;
  float prevTimeScale = 1f;
  bool panelOpen = false;

  void Awake()
  {
    isMuted = PlayerPrefs.GetInt(PrefKeyMuted, 0) == 1;
    ApplyMute(isMuted, save:false);
    UpdateMuteLabel();

    if (ScoreManagerExists())
    {
      ScoreManager.Instance.OnScoreChanged += HandleScoreChanged;
      ScoreManager.Instance.OnHighScoreChanged += HandleHighScoreChanged;
    }
  }

  void OnDestroy()
  {
    if (ScoreManagerExists())
    {
      ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
      ScoreManager.Instance.OnHighScoreChanged -= HandleHighScoreChanged;
    }
  }

  // ⚙️ Settings (gear) button
  public void OpenSettings()
  {
    if (panelOpen) return;
    panelOpen = true;

    if (settingsPanel) settingsPanel.SetActive(true);

    // Pause gameplay; remember previous scale in case you were already paused
    prevTimeScale = Time.timeScale;
    Time.timeScale = 0f;

    if (ScoreManagerExists())
    {
      HandleScoreChanged(ScoreManager.Instance.CurrentScore);
      HandleHighScoreChanged(ScoreManager.Instance.HighScore);
    }
  }

  // X Close on the panel
  public void CloseSettings()
  {
    if (!panelOpen) return;
    panelOpen = false;

    if (settingsPanel) settingsPanel.SetActive(false);

    // Resume only if not in Game Over; otherwise keep paused
    var gm = GameManager.Instance;
    if (gm != null && gm.IsGameOver)
      return;

    Time.timeScale = prevTimeScale;
  }

  // 🔊 Mute toggle
  public void ToggleMute()
  {
    isMuted = !isMuted;
    ApplyMute(isMuted, save:true);
    UpdateMuteLabel();
  }

  void ApplyMute(bool mute, bool save)
  {
    AudioListener.volume = mute ? 0f : 1f;
    if (save) PlayerPrefs.SetInt(PrefKeyMuted, mute ? 1 : 0);
  }

  void UpdateMuteLabel()
  {
    if (muteButtonLabel)
      muteButtonLabel.text = isMuted ? "Unmute Game Sound" : "Mute Game Sound";
  }

  // 🔁 Restart from Settings panel
  public void RestartFromSettings()
  {
    // Close panel, ensure unpause, then restart the run
    if (settingsPanel) settingsPanel.SetActive(false);
    panelOpen = false;

    Time.timeScale = 1f; // make sure GameManager restart flow runs in real time
    var gm = GameManager.Instance;
    if (gm != null) gm.Restart();
  }

  void HandleScoreChanged(int s)
  {
    if (currentScoreLabel) currentScoreLabel.text = $"Score: {s}";
  }

  void HandleHighScoreChanged(int hs)
  {
    // if (highScoreLabel) highScoreLabel.text = $"High Score: {hs}";
    if (highScoreLabel) highScoreLabel.text = $"{hs}";
  }

  public void ResetHighScore()
  {
    Debug.Log("ResetHighScore EXE");
    if (!ScoreManagerExists()) return;
    Debug.Log("ResetHighScore EXE 2");
    ScoreManager.Instance.ResetHighScore();
    HandleHighScoreChanged(ScoreManager.Instance.HighScore);
  }

  bool ScoreManagerExists()
  {
    return ScoreManager.Instance != null;
  }

}
