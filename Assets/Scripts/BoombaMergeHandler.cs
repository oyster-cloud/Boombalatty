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

    // --- LAYER/TAG detection: are these Snacks? ---
    // Prefer layer; if you use a tag instead, swap to CompareTag("Snack")
    int snackLayer = LayerMask.NameToLayer("Snack");
    bool aIsSnack = (snackLayer != -1) && (gameObject.layer == snackLayer);
    bool bIsSnack = (snackLayer != -1) && (collision.gameObject.layer == snackLayer);

    // 1) Snack + Snack -> never merge
    if (aIsSnack && bIsSnack) return;

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
    yield return new WaitForEndOfFrame();

    BoombaProperties thisProps = GetComponent<BoombaProperties>();
    if (thisProps == null) yield break;

    int mergedValue = BoombaMergeRules.GetNextValue(thisProps.Value);

    BoombaSpawner spawner = FindAnyObjectByType<BoombaSpawner>();
    GameObject mergedGO = null;
    if (spawner != null)
    {
      Vector2 mergedPosition = (transform.position + other.transform.position) / 2f;
      mergedGO = spawner.SpawnBoombaWithValue(mergedPosition, mergedValue);
    }

    var otherProps = other ? other.GetComponent<BoombaProperties>() : null;
    var resultProps = mergedGO ? mergedGO.GetComponent<BoombaProperties>() : null;
    BoombaEvents.RaiseMerged(thisProps, otherProps, resultProps);

    // Debug.Log("BEFORE IF");

    // --- Handle last variant behavior ---
    if (resultProps != null && resultProps.IsLastVariant)
    {
      // DISABLING THESE SEEMS TO CAUSE A PROBLEM
      foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
        sr.enabled = false;
      foreach (var sr in other.GetComponentsInChildren<SpriteRenderer>())
        sr.enabled = false;

      Vector3 center = Camera.main != null
        ? Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0))
        : Vector3.zero;
      center.z = 0f;
      mergedGO.transform.position = center;

      Rigidbody2D rb = mergedGO.GetComponent<Rigidbody2D>();
      if (rb)
      {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
      }

      Debug.Log("ABOUT TO PAUSE");

    //   yield return new WaitForSecondsRealtime(2f);
    //   Debug.Log("GO FALSE");
    //   mergedGO.SetActive(false);
    // }

    var gm = GameManager.Instance ?? FindAnyObjectByType<GameManager>();
    if (gm != null)
      gm.HideAfterSecondsRealtime(mergedGO, 2f);

    }

    // Only now disable the originals (after coroutine finishes)
    gameObject.SetActive(false);
    if (other) other.SetActive(false);
    isMerging = false;
  }

}
