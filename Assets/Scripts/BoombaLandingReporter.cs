using UnityEngine;

public class BoombaLandingReporter : MonoBehaviour
{
  private System.Action _onLanded;
  private bool _fired;

  public void Init(System.Action onLanded)
  {
    _onLanded = onLanded;
    _fired = false;
  }

  // Called when this (dynamic) rigidbody hits anything (floor, another boomba, walls, etc.)
  private void OnCollisionEnter2D(Collision2D collision)
  {
    if (_fired) return;
    _fired = true;

    _onLanded?.Invoke();

    // No longer needed after first contact
    Destroy(this);
  }
}
