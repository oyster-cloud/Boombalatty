using System; 
using UnityEngine;

[RequireComponent(typeof(Renderer))] // Ensures a Renderer is present to detect visibility
public class BoombaAutoDespawn : MonoBehaviour
{
  // What it does: Exposes a static event that fires when a Boomba goes offscreen, passing its BoombaProperties.
  // What it's used for: Allows other systems (score, spawn manager, analytics, etc.) to react when a Boomba leaves the visible area before it is disabled.
  public static event Action<BoombaProperties> OnBoombaOffscreen;

  // What it does: Detects when this object is no longer visible by any camera, invokes the offscreen event, then disables the root Boomba GameObject (returning it to the pool).
  // What it's used for: Automatically despawns pooled Boombas when they leave the screen so they stop updating/rendering and can be reused by the BoombaPool.
  void OnBecameInvisible()
  {
    // Get the root (your pooled Boomba)
    var root = transform.root;
    var props = root.GetComponent<BoombaProperties>();

    // Notify listeners BEFORE disabling
    OnBoombaOffscreen?.Invoke(props);

    // Return to pool / disable
    root.gameObject.SetActive(false);
  }
}
