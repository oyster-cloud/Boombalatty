using System.Collections;
using UnityEngine;

// This script handles merging behavior between balls that collide and share the same value.
[RequireComponent(typeof(BallProperties))]
public class BallMergeHandler : MonoBehaviour
{
  public bool isMerging = false; // Flag to prevent double-merging during physics events

  public void OnEnable()
  {
    isMerging = false;
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    Debug.Log($"Trying to merge: {name} and {collision.gameObject.name}");

    // Don't proceed if already merging or disabled
    if (isMerging || !gameObject.activeInHierarchy)
      return;

    // Get BallMergeHandler on the other colliding object
    BallMergeHandler other = collision.gameObject.GetComponent<BallMergeHandler>();
    if (other == null || other.isMerging || !other.gameObject.activeInHierarchy)
      return;

    // Get BallProperties for both objects
    BallProperties thisProps = GetComponent<BallProperties>();
    BallProperties otherProps = other.GetComponent<BallProperties>();
    if (thisProps == null || otherProps == null)
      return;

    // Only merge if the values match, according to game rules
    if (!BallMergeRules.ShouldMerge(thisProps.value, otherProps.value))
      return;

    // Prevent both objects from merging simultaneously; one side "wins" based on instance ID
    if (GetInstanceID() < other.GetInstanceID())
    {
      isMerging = true;
      other.isMerging = true;
      StartCoroutine(SpawnMergedBallAndDisable(other.gameObject));
    }
  }

  // Coroutine that spawns a new merged ball, then disables the original two
  private IEnumerator SpawnMergedBallAndDisable(GameObject other)
  {
    yield return new WaitForEndOfFrame(); // Optional short delay before merge

    BallProperties thisProps = GetComponent<BallProperties>();
    if (thisProps == null) yield break;

    // Determine new value to spawn
    int mergedValue = BallMergeRules.GetNextValue(thisProps.value);

    // Find the spawner and create the new ball at the average position
    BallSpawner spawner = FindAnyObjectByType<BallSpawner>();
    if (spawner != null)
    {
      Vector2 mergedPosition = (transform.position + other.transform.position) / 2f;
      spawner.SpawnBallWithValue(mergedPosition, mergedValue);
    }

    // Disable both original balls
    gameObject.SetActive(false);
    other.SetActive(false);
    isMerging = false;
  }
}
