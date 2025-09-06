using System.Collections;
using UnityEngine;

// This script handles merging behavior between balls that collide and share the same value.
[RequireComponent(typeof(AnimalProperties))]
public class AnimalMergeHandler : MonoBehaviour
{
  public bool isMerging = false; // Flag to prevent double-merging during physics events

  public void OnEnable()
  {
    isMerging = false;
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    // Debug.Log($"Trying to merge: {name} and {collision.gameObject.name}");

    // Don't proceed if already merging or disabled
    if (isMerging || !gameObject.activeInHierarchy)
      return;

    // Get AnimalMergeHandler on the other colliding object
    AnimalMergeHandler other = collision.gameObject.GetComponent<AnimalMergeHandler>();
    if (other == null || other.isMerging || !other.gameObject.activeInHierarchy)
      return;

    // Get AnimalProperties for both objects
    AnimalProperties thisProps = GetComponent<AnimalProperties>();
    AnimalProperties otherProps = other.GetComponent<AnimalProperties>();
    if (thisProps == null || otherProps == null)
      return;

    // Only merge if the values match, according to game rules
    if (!AnimalMergeRules.ShouldMerge(thisProps.Value, otherProps.Value))
      return;

    // Prevent both objects from merging simultaneously; one side "wins" based on instance ID
    if (GetInstanceID() < other.GetInstanceID())
    {
      isMerging = true; // should is merging be set in the function below?
      other.isMerging = true;
      StartCoroutine(SpawnMergedAnimalAndDisable(other.gameObject));
    }
  }

  // Coroutine that spawns a new merged ball, then disables the original two
  private IEnumerator SpawnMergedAnimalAndDisable(GameObject other)
  {
    yield return new WaitForEndOfFrame(); // Optional short delay before merge

    AnimalProperties thisProps = GetComponent<AnimalProperties>();
    if (thisProps == null) yield break;

    // Determine new value to spawn
    int mergedValue = AnimalMergeRules.GetNextValue(thisProps.Value);

    // Find the spawner and create the new ball at the average position
    AnimalSpawner spawner = FindAnyObjectByType<AnimalSpawner>();
    GameObject mergedGO = null;
    if (spawner != null)
    {
      Vector2 mergedPosition = (transform.position + other.transform.position) / 2f;
      mergedGO = spawner.SpawnAnimalWithValue(mergedPosition, mergedValue);
    }

     // 🔊 notify audio/UI/etc. — AFTER spawning, BEFORE disabling originals
    AnimalProperties otherProps = other ? other.GetComponent<AnimalProperties>() : null;
    AnimalProperties resultProps = mergedGO ? mergedGO.GetComponent<AnimalProperties>() : null;
    AnimalEvents.RaiseMerged(thisProps, otherProps, resultProps);

    // Disable both original balls
    gameObject.SetActive(false);
    other.SetActive(false);
    isMerging = false; // should this be set in the caller?
  }
}
