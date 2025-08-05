using UnityEngine;

public class BallTouchSpawner : MonoBehaviour
{
  [Header("Dependencies")]
  public BallSpawner ballSpawner; // Reference to your BallSpawner script in the scene

  void Update()
  {
    // Check for mouse click or screen tap (left click or first finger)
    if (Input.GetMouseButtonDown(0))
    {
      // Convert screen coordinates (mouse/tap) to world space coordinates
      Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

      // Clamp the world position to the BallSpawner’s configured spawn area
      worldPosition = new Vector2(
        Mathf.Clamp(worldPosition.x, ballSpawner.spawnAreaMin.x, ballSpawner.spawnAreaMax.x),
        Mathf.Clamp(worldPosition.y, ballSpawner.spawnAreaMin.y, ballSpawner.spawnAreaMax.y)
      );

      // Ask the BallSpawner to create a random ball at the clamped position
      ballSpawner.SpawnBallWithValue(worldPosition, 1);
    }
  }
}
