using UnityEngine;

public class AutoDestroyAfter : MonoBehaviour
{
  public float lifetime = 0.3f;
  void Start() => Destroy(gameObject, lifetime);
}
