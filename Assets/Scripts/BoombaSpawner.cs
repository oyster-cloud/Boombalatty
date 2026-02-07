using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Represents a specific type of boomba with size, sprite, and value
[System.Serializable]
public class BoombaVariant
{
  public Sprite sprite;
  public GameObject visualPrefab; // SpriteVisual OR SpineVisual
  [Range(0.3f, 5.0f)]
  public float size;
  public int value;
  [Range(0.1f, 2f)]
  public float ImageScale = 1f;
}

public class BoombaSpawner : MonoBehaviour
{
  [Header("Dependencies")]
  public BoombaPool boombaPool;

  [Header("Testing")]
  public bool requireInitialSpawnCompleted = false; // test-only hook

  [Header("Spawn Settings")]
  public int minBoombas = 3;
  public int maxBoombas = 7;
  public Vector2 spawnAreaMin = new Vector2(-2.5f, 2.5f);
  public Vector2 spawnAreaMax = new Vector2(2.5f, 2.5f);
  public float spawnDelay = 0.8f;

  [Header("Boomba Variants")]
  public List<BoombaVariant> boombaVariants = new List<BoombaVariant>();

  // True once the initial delayed spawn wave completes.
  // I'm defining this in multiple places. I want to use this in one universal place
  public bool InitialSpawnCompleted { get; private set; } = false;

  /// Fired once when the initial wave finishes.
  public event System.Action OnInitialSpawnComplete;

  private int _nextSortingOrder = 0;

  // What it does: Registers this instance with the global Services locator when enabled.
  // What it's used for: Allows other systems to easily find the active BoombaSpawner.
  void OnEnable()
  {
    Services.BoombaSpawner = this;
  }

  // What it does: Clears the Services reference when this spawner is disabled.
  // What it's used for: Prevents stale references to an inactive BoombaSpawner.
  void OnDisable()
  {
    if (Services.BoombaSpawner == this)
      Services.BoombaSpawner = null;
  }

  // What it does: Validates that the pool and variant list are configured correctly.
  // What it's used for: Called to ensure the spawner is safe to use before spawning begins.
  public void Initialize()
  {
    if (boombaPool == null)
    {
      Debug.LogError("BoombaPool is not assigned in BoombaSpawner!");
      return;
    }

    if (boombaVariants == null || boombaVariants.Count == 0)
    {
      Debug.LogError("No boomba variants assigned!");
      return;
    }
  }

  // What it does: Starts the initial spawn coroutine on game start.
  // What it's used for: Kicks off the first wave of boombas when the scene begins.
  void Start()
  {
    // Spawn boombas when game starts
    InitialSpawnCompleted = false;
    StartCoroutine(SpawnBoombasWithDelay());
  }

  // What it does: Listens for a debug key (R) to restart the game for testing.
  // What it's used for: Allows quick in-editor restarts without building UI for it.
  void Update()
  {
    // Press 'R' to restart (for testing)
    if (Input.GetKeyDown(KeyCode.R))
    {
      RestartGame();
    }
  }

  // What it does: Spawns a randomized batch of boombas over time within the configured spawn area.
  // What it's used for: Creates the initial wave of falling boombas and signals when that wave is done.
  IEnumerator SpawnBoombasWithDelay()
  {
    if (boombaPool == null)
    {
      Debug.LogError("BoombaPool is not assigned in BoombaSpawner!");
      yield break;
    }

    if (boombaVariants == null || boombaVariants.Count == 0)
    {
      Debug.LogError("No boomba variants assigned!");
      yield break;
    }

    int boombaCount = Random.Range(minBoombas, maxBoombas + 1);
    
    for (int i = 0; i < boombaCount; i++)
    {      
      Vector2 randomPos = new Vector2(
        Random.Range(spawnAreaMin.x, spawnAreaMax.x),
        Random.Range(spawnAreaMin.y, spawnAreaMax.y)
      );

      int limit = Mathf.CeilToInt(boombaVariants.Count / 2f);
      int variantIndex = Random.Range(0, limit);

      BoombaVariant variant = boombaVariants[variantIndex];

      SpawnBoombaWithValue(randomPos, variant.value);

      yield return new WaitForSeconds(spawnDelay);
    }

    // ✅ mark complete and notify
    InitialSpawnCompleted = true;
    OnInitialSpawnComplete?.Invoke();
  }

  // What it does: Spawns (or reuses from the pool) a boomba of a specific value at the given position.
  // What it's used for: Used both by initial spawning and by merge logic to create correctly configured boombas.
  public virtual GameObject SpawnBoombaWithValue(Vector2 position, int value)
  {
    BoombaVariant variant = boombaVariants.Find(v => v.value == value);

    if (variant == null)
    {
      return null;
    }

    GameObject boomba = boombaPool.GetBoomba(position);
    boomba.transform.localScale = Vector3.one * variant.size;

    // Find visual containers
    Transform art = boomba.transform.Find("VisualRoot");
    if (art == null) {
      Debug.LogError("Boomba prefab missing child: Art");
      return boomba;
    }
    
    // What it does: Assigns a unique sorting order for each spawned boomba via its SortingGroup.
    // What it's used for: Ensures overlapping boombas render in a stable, flicker-free order.
    var sortingGroup = art.GetComponent<UnityEngine.Rendering.SortingGroup>();
    if (sortingGroup != null)
    {
      sortingGroup.sortingOrder = _nextSortingOrder++;
    }

    Transform spriteArt = art.Find("SpriteArt");
    Transform animationArt = art.Find("AnimationArt");
    if (spriteArt == null || animationArt == null) {
      Debug.LogError("Boomba prefab missing SpriteArt or AnimationArt under Art");
      return boomba;
    }

    // Always reset visuals (important for pooling)
    spriteArt.gameObject.SetActive(false);
    animationArt.gameObject.SetActive(false);

    // Clear previous animation visuals
    for (int i = animationArt.childCount - 1; i >= 0; i--)
    {
      Destroy(animationArt.GetChild(i).gameObject);
    }

    // What it does: Chooses between a Spine-based visual prefab or a simple Sprite visual for this variant.
    // What it's used for: Allows different boomba types to use either animated or static visuals with the same spawn pipeline.
    if (variant.visualPrefab != null)
    {
      var sr = spriteArt.GetComponent<SpriteRenderer>();
      if (sr) sr.sprite = null;

      // 👉 Spine / animated variant
      animationArt.gameObject.SetActive(true);

      GameObject visual = Instantiate(variant.visualPrefab, animationArt);
      visual.transform.localPosition = Vector3.zero;
      visual.transform.localRotation = Quaternion.identity;
      visual.transform.localScale = Vector3.one * variant.ImageScale;
    }
    else
    {
      // 👉 Sprite variant
      spriteArt.gameObject.SetActive(true);

      SpriteRenderer sr = spriteArt.GetComponent<SpriteRenderer>();
      sr.sprite = variant.sprite;
      sr.transform.localScale = Vector3.one * variant.ImageScale;
    }

    BoombaProperties props = boomba.GetComponent<BoombaProperties>();
    if (props != null)
    {
      // What it does: Detects if this variant is the highest-level one and flags it.
      // What it's used for: Lets other systems treat the final variant differently (e.g., endgame behavior).
      bool isLast = boombaVariants.Count > 0 &&
              object.ReferenceEquals(variant, boombaVariants[boombaVariants.Count - 1]);
      props.IsLastVariant = isLast;

      props.SetValue(variant.value);
    }
    
    return boomba;
  }

  // What it does: Deactivates all boombas and then restarts the initial spawn coroutine.
  // What it's used for: Provides a simple way to reset the board for restarts or testing.
  public void RestartGame()
  {
    foreach (GameObject boomba in boombaPool.GetAllBoombas())
    {
      boomba.SetActive(false);
    }

    InitialSpawnCompleted = false;
    StartCoroutine(SpawnBoombasWithDelay());
  }

  // What it does: Stops all current spawn coroutines and starts a fresh initial wave.
  // What it's used for: Called when you want to fully reset spawning behavior from external code.
  public void ResetAndStartInitialSpawn()
  {
    StopAllCoroutines();
    InitialSpawnCompleted = false;
    StartCoroutine(SpawnBoombasWithDelay());
  }
}
