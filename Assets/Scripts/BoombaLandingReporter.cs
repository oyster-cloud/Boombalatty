using UnityEngine;

public class BoombaLandingReporter : MonoBehaviour
{
  private System.Action _onLanded;
  private bool _fired;

  // What it does: Initializes this landing reporter with a callback to invoke when the object first collides.
  // What it's used for: Allows external code to be notified exactly once when this rigidbody first "lands".
  public void Init(System.Action onLanded)
  {
    _onLanded = onLanded;
    _fired = false;
  }

  // What it does: Detects the first 2D collision for this object, invokes the landing callback, then destroys itself.
  // What it's used for: Used to trigger one-time landing logic (e.g., sound, effects, state changes) and then remove the reporter.
  private void OnCollisionEnter2D(Collision2D collision)
  {
    if (_fired) return;
    _fired = true;

    _onLanded?.Invoke();

    // No longer needed after first contact
    Destroy(this);
  }
}
