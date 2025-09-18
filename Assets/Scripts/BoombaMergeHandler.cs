using System.Collections;
using UnityEngine;

// This script handles merging behavior between boombas that collide and share the same value.
[RequireComponent(typeof(BoombaProperties))]
public class BoombaMergeHandler : MonoBehaviour
{
  public bool isMerging = false; // Flag to prevent double-merging during physics events

  public void OnEnable()
  {
    isMerging = false;
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    // Don't proceed if already merging or disabled
    if (isMerging || !gameObject.activeInHierarchy)
      return;

    // Get BoombaMergeHandler on the other colliding object
    BoombaMergeHandler other = collision.gameObject.GetComponent<BoombaMergeHandler>();
    if (other == null || other.isMerging || !other.gameObject.activeInHierarchy)
      return;

    // Get BoombaProperties for both objects
    BoombaProperties thisProps = GetComponent<BoombaProperties>();
    BoombaProperties otherProps = other.GetComponent<BoombaProperties>();
    if (thisProps == null || otherProps == null)
      return;

    // Only merge if the values match, according to game rules
    if (!BoombaMergeRules.ShouldMerge(thisProps.Value, otherProps.Value))
      return;

    // Prevent both objects from merging simultaneously; one side "wins" based on instance ID
    if (GetInstanceID() < other.GetInstanceID())
    {
      isMerging = true; // should is merging be set in the function below?
      other.isMerging = true;
      StartCoroutine(SpawnMergedBoombaAndDisable(other.gameObject));
    }
  }

  // Coroutine that spawns a new merged boomba, then disables the original two
  private IEnumerator SpawnMergedBoombaAndDisable(GameObject other)
  {
    yield return new WaitForEndOfFrame(); // Optional short delay before merge

    BoombaProperties thisProps = GetComponent<BoombaProperties>();
    if (thisProps == null) yield break;

    // Determine new value to spawn
    int mergedValue = BoombaMergeRules.GetNextValue(thisProps.Value);

    // Find the spawner and create the new boomba at the average position
    BoombaSpawner spawner = FindAnyObjectByType<BoombaSpawner>();
    GameObject mergedGO = null;
    if (spawner != null)
    {
      Vector2 mergedPosition = (transform.position + other.transform.position) / 2f;
      mergedGO = spawner.SpawnBoombaWithValue(mergedPosition, mergedValue);
    }

     // 🔊 notify audio/UI/etc. — AFTER spawning, BEFORE disabling originals
    BoombaProperties otherProps = other ? other.GetComponent<BoombaProperties>() : null;
    BoombaProperties resultProps = mergedGO ? mergedGO.GetComponent<BoombaProperties>() : null;
    BoombaEvents.RaiseMerged(thisProps, otherProps, resultProps);

    // Disable both original boombas
    gameObject.SetActive(false);
    other.SetActive(false);
    isMerging = false; // should this be set in the caller?
  }
}
