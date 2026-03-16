using UnityEngine;

// What it does: Handles saving and loading game state to persist progress across sessions.
// What it's used for: Saves current stage/difficulty so players don't lose progress when closing the game.
public class GameStateManager
{
  private const string PREF_CURRENT_STAGE = "CurrentStage";
  private const string PREF_CURRENT_BOOMBA_COUNT = "CurrentBoombaCount";

  // What it does: Saves the current stage and difficulty to PlayerPrefs.
  // What it's used for: Called whenever the player progresses to preserve their progress.
  public void SaveGameState(int currentStage, int currentBoombaCount)
  {
    PlayerPrefs.SetInt(PREF_CURRENT_STAGE, currentStage);
    PlayerPrefs.SetInt(PREF_CURRENT_BOOMBA_COUNT, currentBoombaCount);
    PlayerPrefs.Save();
    
    Debug.Log($"Game state saved: Stage {currentStage}, Boomba Count {currentBoombaCount}");
  }

  // What it does: Loads the saved stage number, or returns 1 if no save exists.
  // What it's used for: Restores the player's stage progress when the game starts.
  public int LoadCurrentStage()
  {
    return PlayerPrefs.GetInt(PREF_CURRENT_STAGE, 1); // Default to stage 1
  }

  // What it does: Loads the saved boomba count, or returns the starting count if no save exists.
  // What it's used for: Restores the player's difficulty progress when the game starts.
  public int LoadCurrentBoombaCount(int defaultCount)
  {
    return PlayerPrefs.GetInt(PREF_CURRENT_BOOMBA_COUNT, defaultCount);
  }

  // What it does: Deletes all saved game progress.
  // What it's used for: Resets progress back to stage 1 (e.g., from a settings menu).
  public void ResetProgress()
  {
    PlayerPrefs.DeleteKey(PREF_CURRENT_STAGE);
    PlayerPrefs.DeleteKey(PREF_CURRENT_BOOMBA_COUNT);
    PlayerPrefs.Save();
    
    Debug.Log("Game progress reset");
  }

  // What it does: Checks if any saved game state exists.
  // What it's used for: Determines whether to show "Continue" vs "New Game" options.
  public bool HasSavedProgress()
  {
    return PlayerPrefs.HasKey(PREF_CURRENT_STAGE);
  }
}
