using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
  [SerializeField] string gameSceneName = "Boombalatty";

  public void StartGame() {
    Debug.Log("Start game");
    SceneManager.LoadScene(gameSceneName);
  }

  public void Quit()
  {
    #if UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
    #else
      Application.Quit();
    #endif
  }
}
