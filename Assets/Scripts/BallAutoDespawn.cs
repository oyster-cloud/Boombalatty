// I don't think I need this.
// Removed from the root of the ball prefab
// May need to be reattached to the sprite renderer child
using UnityEngine;

[RequireComponent(typeof(Renderer))] // Ensures a Renderer is present to detect visibility
public class BallAutoDespawn : MonoBehaviour
{
  // Called automatically when the object goes off-screen
  void OnBecameInvisible()
  {
    // Disable the ball instead of destroying it (for pooling)
    gameObject.SetActive(false);

    // Turn off the whole pooled Ball, not just the Art child
    // transform.root.gameObject.SetActive(false);
  }
}
