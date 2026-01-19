using UnityEngine;
using UnityEngine.SceneManagement;

// What it does: Handles main menu button actions such as starting the game and quitting.
// What it's used for: Wired to UI buttons in the main menu scene to transition into gameplay or exit the application.
public class MenuUI : MonoBehaviour
{
  [SerializeField] string gameSceneName = "Boombalatty";

  // What it does: Loads the configured game scene by name.
// What it's used for: Called by the "Start" button to enter the main gameplay scene.
  public void StartGame() {
    SceneManager.LoadScene(gameSceneName);
  }

  // What it does: Quits the game in a build, or stops play mode in the editor.
// What it's used for: Called by the "Quit" button to exit the game appropriately depending on environment.
  public void Quit()
  {
    #if UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
    #else
      Application.Quit();
    #endif
  }
}
