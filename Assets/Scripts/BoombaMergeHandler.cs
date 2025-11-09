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

  public void OnEnable()
  {
    isMerging = false;
  }

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

    // Skip all interactions if either is the last variant
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



    // // 1) Snack + Snack -> never merge
    // if (aIsSnack && bIsSnack) return;

    // // 2) Boomba + Boomba => now disabled
    // if (!aIsSnack && !bIsSnack) return;

    // Only proceed for Snack-Boomba pair (in either order)
    if (!((aIsSnack && bIsBoomba) || (aIsBoomba && bIsSnack)))
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
      Vector2 mergedPosition = (transform.position + other.transform.position) / 2f;
      mergedGO = spawner.SpawnBoombaWithValue(mergedPosition, mergedValue);
    }

    // --- Force merged Boomba into LIVE layer so it can continue merging ---
    if (mergedGO != null)
    {
      var flip = mergedGO.GetComponent<GameOverZoneFlipLayer>();
      if (flip != null)
      {
        flip.ForceLiveLayer(); // ensures it's Boomba-layer now (and marks it flipped)
      }
      else
      {
        // Fallback: set Boomba layer on root + children
        int liveBoombaLayer = LayerMask.NameToLayer(boombaLayerName);
        var trs = mergedGO.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < trs.Length; i++)
          trs[i].gameObject.layer = liveBoombaLayer;
      }
    }

    var otherProps = other ? other.GetComponent<BoombaProperties>() : null;
    var resultProps = mergedGO ? mergedGO.GetComponent<BoombaProperties>() : null;
    BoombaEvents.RaiseMerged(thisProps, otherProps, resultProps);

    // --- Handle last variant behavior ---
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
