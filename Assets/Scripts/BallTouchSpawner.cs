using UnityEngine;

/// <summary>
/// Listens for touch or mouse input and spawns a ball through the BallSpawner.
/// </summary>
public class BallTouchSpawner : MonoBehaviour
{
  [Header("Dependencies")]
  public BallSpawner ballSpawner; // Reference to the spawner used to create balls

  /// <summary>
  /// Converts a screen position to world coordinates, clamps it to the spawn area, and spawns a ball.
  /// </summary>
  /// <param name="screenPosition">The screen position from the user's input.</param>
  public void HandleTouch(Vector2 screenPosition)
  {
    // Convert screen to world position using the main camera
    Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

    // Clamp the position within allowed spawn boundaries
    worldPosition = new Vector2(
      Mathf.Clamp(worldPosition.x, ballSpawner.spawnAreaMin.x, ballSpawner.spawnAreaMax.x),
      Mathf.Clamp(worldPosition.y, ballSpawner.spawnAreaMin.y, ballSpawner.spawnAreaMax.y)
    );

    // Spawn the ball at the clamped world position
    ballSpawner.SpawnBallWithValue(worldPosition, 1);
  }

  /// <summary>
  /// Checks for user input each frame.
  /// </summary>
  void Update()
  {
    if (Input.GetMouseButtonDown(0)) // left-click or first touch
    {
      HandleTouch(Input.mousePosition);
    }
  }
}
