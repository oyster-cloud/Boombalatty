using System.Collections;
using UnityEngine;

// This script handles merging behavior between boombas that collide and share the same value.
[RequireComponent(typeof(BoombaProperties))]
public class BoombaMergeHandler : MonoBehaviour
{
  private static float lastMergeTime = 0f;  // shared cooldown tracker
  private static float mergeDelay = 0.5f;  // shared cooldown tracker
  public bool isMerging = false; // Flag to prevent double-merging during physics events
  [SerializeField] string snackLayerName = "Snack";
  [SerializeField] string boombaLayerName = "Boomba";
  [SerializeField] GameObject snackPoofPrefab;

  // What it does: Resets the merging flag whenever this component is enabled.
  // What it's used for: Ensures pooled or reactivated boombas can merge again correctly.
  // Refactor?: Handle this in BoombaEvents?
  public void OnEnable()
  {
    isMerging = false;
  }

  // What it does: Handles collision logic to determine if a merge should occur between this boomba and another.
  // What it's used for: Runs the core rules for merging Snacks and Boombas, preventing double merges and triggering merged spawn.
  private void OnCollisionEnter2D(Collision2D collision)
  {
    // Don't proceed if already merging or disabled
    if (isMerging || !gameObject.activeInHierarchy)
      return;

    // Get BoombaMergeHandler on the other colliding object. Return if no exist, active or isMerging
    BoombaMergeHandler other = collision.gameObject.GetComponent<BoombaMergeHandler>();
    if (other == null || other.isMerging || !other.gameObject.activeInHierarchy)
      return;

    // Get BoombaProperties for both objects
    BoombaProperties thisProps = GetComponent<BoombaProperties>();
    BoombaProperties otherProps = other.GetComponent<BoombaProperties>();
    if (thisProps == null || otherProps == null)
      return;

    // What it does: Prevents merging if either boomba is the final variant in the chain.
    // What it's used for: Enforces the rule that "last variant" pieces cannot merge further.
    if (thisProps.IsLastVariant || otherProps.IsLastVariant)
    {
      Physics2D.IgnoreCollision(
        collision.collider,
        GetComponent<Collider2D>()
      );
      return;
    }

    // --- LAYER/TAG detection: are these Snacks? ---
    // Prefer layer; if you use a tag instead, swap to CompareTag("Snack")
    int snackLayer = LayerMask.NameToLayer(snackLayerName);
    int boombaLayer = LayerMask.NameToLayer(boombaLayerName);

    bool aIsSnack = (snackLayer != -1) && (gameObject.layer == snackLayer);
    bool bIsSnack = (snackLayer != -1) && (collision.gameObject.layer == snackLayer);
    bool aIsBoomba = (boombaLayer != -1) && (gameObject.layer == boombaLayer);
    bool bIsBoomba = (boombaLayer != -1) && (collision.gameObject.layer == boombaLayer);

    // What it does: Ensures only Snack–Boomba pairs (in either order) can proceed to merge.
    // What it's used for: Prevents Snack–Snack and Boomba–Boomba collisions from triggering merge logic.
    if (!((aIsSnack && bIsBoomba) || (aIsBoomba && bIsSnack)))
      return;

    // What it does: Checks if the values on both boombas qualify for merging according to the merge rules.
    // What it's used for: Enforces merge eligibility based on their logical level/value.
    if (!BoombaMergeRules.ShouldMerge(thisProps.Value, otherProps.Value))
      return;

    // What it does: Ensures only one side of the collision initiates the merge (based on instance ID).
    // What it's used for: Avoids duplicate merge coroutines being started for the same collision pair.
    if (GetInstanceID() < other.GetInstanceID())
    {
      isMerging = true; // should is merging be set in the function below?
      other.isMerging = true;
      StartCoroutine(SpawnMergedBoombaAndDisable(other.gameObject));
    }
  }

  // What it does: Waits for cooldown, spawns a merged boomba at the midpoint, purges matching Snacks, and disables originals.
  // What it's used for: Encapsulates the full merge sequence, from spawning the result to handling last-variant special behavior.
  // Refactor?: Handle last-variant (Whale) logic in WhaleBehavior.cs?
  private IEnumerator SpawnMergedBoombaAndDisable(GameObject otherGO)
  {
    // prevent rapid chain merges
    float elapsed = Time.time - lastMergeTime;
    if (elapsed < mergeDelay)
      yield return new WaitForSeconds(mergeDelay - elapsed);

    lastMergeTime = Time.time;

    yield return new WaitForEndOfFrame();

    BoombaProperties thisProps = GetComponent<BoombaProperties>();
    if (thisProps == null) yield break;

    int mergedValue = BoombaMergeRules.GetNextValue(thisProps.Value);

    BoombaSpawner spawner = FindAnyObjectByType<BoombaSpawner>();
    GameObject mergedGO = null;
    if (spawner != null)
    {
      Vector2 mergedPosition = (transform.position + otherGO.transform.position) / 2f;
      mergedGO = spawner.SpawnBoombaWithValue(mergedPosition, mergedValue);
    }

    // --- Remove all other Snacks matching the merging value (before disabling originals) ---
    PurgeOtherSnacksOfValue(thisProps.Value, this.gameObject, otherGO);

    // --- Force merged Boomba into LIVE layer so it can continue merging ---
    if (mergedGO != null)
    {
      int liveBoombaLayer = LayerMask.NameToLayer(boombaLayerName);
      var trs = mergedGO.GetComponentsInChildren<Transform>(true);
      for (int i = 0; i < trs.Length; i++)
        trs[i].gameObject.layer = liveBoombaLayer;

      var flip = mergedGO.GetComponent<GameOverZoneFlipLayer>();
      if (flip != null)
        flip.ForceLiveLayer();
    }

    var otherProps = otherGO ? otherGO.GetComponent<BoombaProperties>() : null;
    var resultProps = mergedGO ? mergedGO.GetComponent<BoombaProperties>() : null;

    // What it does: Raises a global event describing the merge participants and result.
    // What it's used for: Notifies other systems (e.g., scoring, effects, UI) that a merge just occurred.
    // Refactor?: Only used for Sounds Effects right now. Should I use it for other stuff?
    BoombaEvents.RaiseMerged(thisProps, otherProps, resultProps);

    // What it does: Handles special behavior when the merged boomba is the final variant (centering, freezing, hiding).
    // What it's used for: Implements endgame or special-case logic when the highest-level boomba appears.
    // Refactor?: Should all of this be handled in WhaleBehavior.cs?
    if (resultProps != null && resultProps.IsLastVariant)
    {
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

      // Lock physics + input for the whale display duration, then hide and unlock
      var gm = GameManager.Instance ?? FindAnyObjectByType<GameManager>();
      if (gm != null)
        gm.LockForWhaleDisplay(mergedGO, 2f);
    }

    // Only now disable the originals (after coroutine finishes)
    // REfactor?: Should I be resetting bodyType to Dynamic?
    gameObject.SetActive(false);
    if (otherGO) otherGO.SetActive(false);
    isMerging = false;
  }

  // What it does: Finds and deactivates all other Snack objects that share the given value, excluding the two merging ones.
  // What it's used for: Cleans up extra Snacks of the same level when a merge happens, to enforce your game’s merge rules.
  void PurgeOtherSnacksOfValue(int value, GameObject exceptA, GameObject exceptB)
  {
    int snackLayer = LayerMask.NameToLayer(snackLayerName); // uses your serialized field
    if (snackLayer == -1) return;

    var all = FindObjectsByType<BoombaProperties>(FindObjectsSortMode.None);
    for (int i = 0; i < all.Length; i++)
    {
      var bp = all[i];
      if (!bp || !bp.gameObject.activeInHierarchy) continue;

      var go = bp.gameObject;
      if (go == exceptA || go == exceptB) continue;
      if (go.layer != snackLayer) continue;        // only purge Snacks
      if (bp.Value != value) continue;             // only same-level Snacks
      // Spawn smoke poof effect
      if (snackPoofPrefab != null)
      {
        // use the snack's position; adjust Z to be safe for sorting
        Vector3 pos = go.transform.position;
        pos.z = 0f;
        Instantiate(snackPoofPrefab, pos, Quaternion.identity);
      }
      go.SetActive(false);                          // or return to pool if you have one
    }
  }
}
