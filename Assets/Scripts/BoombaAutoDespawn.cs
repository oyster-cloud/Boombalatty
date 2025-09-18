using System; 
using UnityEngine;

[RequireComponent(typeof(Renderer))] // Ensures a Renderer is present to detect visibility
public class BoombaAutoDespawn : MonoBehaviour
{
  public static event Action<BoombaProperties> OnBoombaOffscreen;

  // Called automatically when the object goes off-screen
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
