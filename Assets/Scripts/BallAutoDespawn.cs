using UnityEngine;

[RequireComponent(typeof(Renderer))] // Ensures a Renderer is present to detect visibility
public class BallAutoDespawn : MonoBehaviour
{
  // Called automatically when the object goes off-screen
  void OnBecameInvisible()
  {
    // Disable the ball instead of destroying it (for pooling)
    gameObject.SetActive(false);
  }
}
