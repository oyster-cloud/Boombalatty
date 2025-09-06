using System; 
using UnityEngine;

[RequireComponent(typeof(Renderer))] // Ensures a Renderer is present to detect visibility
public class AnimalAutoDespawn : MonoBehaviour
{
  public static event Action<AnimalProperties> OnAnimalOffscreen;

  // Called automatically when the object goes off-screen
  void OnBecameInvisible()
  {
    // Get the root (your pooled Animal)
    var root = transform.root;
    var props = root.GetComponent<AnimalProperties>();

    // Notify listeners BEFORE disabling
    OnAnimalOffscreen?.Invoke(props);

    // Return to pool / disable
    root.gameObject.SetActive(false);
  }
}
