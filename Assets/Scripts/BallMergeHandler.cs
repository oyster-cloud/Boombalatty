using System.Collections;
using UnityEngine;

public class BallMergeHandler : MonoBehaviour
{
  private bool isMerging = false;      // Prevents simultaneous merge attempts
  private BallSpawner spawner;         // Reference to the BallSpawner in the scene

  void OnEnable()
  {
    isMerging = false;
  }

  void Awake()
  {
    spawner = FindFirstObjectByType<BallSpawner>(); // Find the spawner once at start
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    Debug.Log($"Trying to merge: {name} and {collision.gameObject.name}");

    // Don't allow merge if already merging or inactive
    if (isMerging || !gameObject.activeInHierarchy) {
      Debug.Log($"(isMerging || !gameObject.activeInHierarchy): {!gameObject.activeInHierarchy}");
      return;
    }

    Debug.Log($"2: {name} and {collision.gameObject.name}");

    // Check if the other object is a mergeable ball
    BallMergeHandler other = collision.gameObject.GetComponent<BallMergeHandler>();
    if (other == null || other.isMerging || !other.gameObject.activeInHierarchy) {
      Debug.Log($"(other == null || other.isMerging || !other.gameObject.activeInHierarchy): {(other == null || other.isMerging || !other.gameObject.activeInHierarchy)}");
      return;
    }

    // Get each ball’s value
    BallProperties thisProps = GetComponent<BallProperties>();
    BallProperties otherProps = other.GetComponent<BallProperties>();
    if (thisProps == null || otherProps == null) {
      Debug.Log($"(thisProps == null || otherProps == null): {thisProps}, {otherProps}");
      return;
    }

    // Only merge balls with the same value
    if (thisProps.value != otherProps.value) {
      Debug.Log($"(thisProps.value != otherProps.value): {otherProps.value}");
      return;
    }

    // Prevent both sides from merging simultaneously (resolve using instance ID)
    if (GetInstanceID() < other.GetInstanceID())
    {
      Debug.Log($"(GetInstanceID() < other.GetInstanceID(): {GetInstanceID()}, {other.GetInstanceID()}");
      isMerging = true;
      other.isMerging = true;
      StartCoroutine(SpawnMergedBallAndDisable(other.gameObject));
    }
  }

  private IEnumerator SpawnMergedBallAndDisable(GameObject otherBall)
  {
    yield return null; // Delay by one frame to ensure physics resolves

    // Safety check in case spawner is missing
    if (spawner == null)
      spawner = FindFirstObjectByType<BallSpawner>();

    // Calculate midpoint between two merged balls
    Vector2 midPoint = (transform.position + otherBall.transform.position) / 2f;

    // Create a new ball with value incremented by 1
    int newValue = GetComponent<BallProperties>().value + 1;
    spawner.SpawnBallWithValue(midPoint, newValue);

    // Disable the original merged balls
    gameObject.SetActive(false);
    otherBall.SetActive(false);
  }
}
