using UnityEngine;
using UnityEngine.UI;
using TMPro;

// What it does: Manages the in-game Settings panel, including mute state, score labels, and restart-from-settings behavior.
// What it's used for: Provides UI controls for pausing via the settings panel, toggling audio, viewing/resetting high score,
//                     and restarting the game without leaving the current scene.
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

  // What it does: Loads the saved mute preference, applies it, updates the mute button label,
  //               and subscribes to ScoreManager events if available.
  // What it's used for: Ensures audio state and score labels are initialized correctly when the settings UI is created.
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

  // What it does: Unsubscribes from ScoreManager events when this UI object is destroyed.
  // What it's used for: Prevents event-handler leaks or null-reference issues after the settings UI is gone.
  void OnDestroy()
  {
    if (ScoreManagerExists())
    {
      ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
      ScoreManager.Instance.OnHighScoreChanged -= HandleHighScoreChanged;
    }
  }

  // ⚙️ Settings (gear) button
  // What it does: Opens the settings panel, pauses gameplay (saving prior timescale), and refreshes score labels.
  // What it's used for: Called by the gear/settings button to show the settings UI and pause the game.
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
  // What it does: Closes the settings panel and restores the previous timescale unless the game is already over.
  // What it's used for: Called by the close button to hide the settings UI and resume play (if not in game-over state).
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
  // What it does: Flips the current mute state, applies it to the AudioListener, saves it, and updates the label.
  // What it's used for: Bound to the mute button so the player can toggle audio on/off from the settings panel.
  public void ToggleMute()
  {
    isMuted = !isMuted;
    ApplyMute(isMuted, save:true);
    UpdateMuteLabel();
  }

  // What it does: Sets the global AudioListener volume based on the mute flag and optionally persists the value to PlayerPrefs.
  // What it's used for: Central place where the actual audio mute/unmute behavior is applied and remembered.
  void ApplyMute(bool mute, bool save)
  {
    AudioListener.volume = mute ? 0f : 1f;
    if (save) PlayerPrefs.SetInt(PrefKeyMuted, mute ? 1 : 0);
  }

  // What it does: Updates the mute button text to reflect the current mute state.
  // What it's used for: Gives visual feedback in the UI showing whether sound is currently muted or not.
  void UpdateMuteLabel()
  {
    if (muteButtonLabel)
      muteButtonLabel.text = isMuted ? "On" : "Off";
  }

  // 🔁 Restart from Settings panel
  // What it does: Closes the settings panel, unpauses time, and then calls GameManager.Restart().
  // What it's used for: Provides a restart button inside the settings UI to reset the run without returning to a main menu.
  public void RestartFromSettings()
  {
    // Close panel, ensure unpause, then restart the run
    if (settingsPanel) settingsPanel.SetActive(false);
    panelOpen = false;

    Time.timeScale = 1f; // make sure GameManager restart flow runs in real time
    var gm = GameManager.Instance;
    if (gm != null) gm.Restart();
  }

  // What it does: Updates the on-panel current score label when the score changes.
  // What it's used for: Keeps the score display in the settings panel in sync with the ScoreManager.
  void HandleScoreChanged(int s)
  {
    if (currentScoreLabel) currentScoreLabel.text = $"{s}";
  }

  // What it does: Updates the on-panel high score label when the high score changes.
  // What it's used for: Shows the latest high score in the settings panel, typically as a simple number.
  void HandleHighScoreChanged(int hs)
  {
    // if (highScoreLabel) highScoreLabel.text = $"High Score: {hs}";
    if (highScoreLabel) highScoreLabel.text = $"{hs}";
  }

  // What it does: Resets the stored high score via the ScoreManager and refreshes the high score label.
  // What it's used for: Bound to the Reset High Score button so players can clear their best score from the settings UI.
  public void ResetHighScore()
  {
    if (!ScoreManagerExists()) return;
    ScoreManager.Instance.ResetHighScore();
    HandleHighScoreChanged(ScoreManager.Instance.HighScore);
  }

  // What it does: Returns true if a ScoreManager singleton instance currently exists.
  // What it's used for: Safely guards all ScoreManager access to avoid null-reference errors if no score system is present.
  bool ScoreManagerExists()
  {
    return ScoreManager.Instance != null;
  }

  // 🛑 End Game from Settings panel
  // What it does: Closes the settings panel, resets all progress, and restarts the game.
  // What it's used for: Provides an "End Game" button inside the settings UI to wipe all progress and start fresh.
  public void EndGameFromSettings()
  {
    // Close panel
    if (settingsPanel) settingsPanel.SetActive(false);
    panelOpen = false;

    Time.timeScale = 1f; // make sure GameManager flow runs in real time
    
    var gm = GameManager.Instance;
    if (gm != null) 
    {
      gm.ResetProgress(); // Wipe saved progress
      gm.Restart();       // Restart at base difficulty
    }
  }
}
