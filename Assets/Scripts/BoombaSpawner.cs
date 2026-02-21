using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Represents a specific type of wired boomba with size, sprite, and value
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

  // What it does: Validates setup and starts the initial spawn coroutine on game start.
  // What it's used for: Kicks off the first wave of boombas when the scene begins.
  void Start()
  {
    Initialize();
    InitialSpawnCompleted = false;
    StartCoroutine(SpawnBoombasWithDelay());
  }

  // What it does: Spawns a randomized batch of boombas over time within the configured spawn area.
  // What it's used for: Creates the initial wave of falling boombas and signals when that wave is done.
  IEnumerator SpawnBoombasWithDelay()
  {
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

    // Mark complete and notify
    InitialSpawnCompleted = true;
    OnInitialSpawnComplete?.Invoke();
  }

  // What it does: Spawns (or reuses from the pool) a boomba of a specific value at the given position.
  // What it's used for: Used both by initial spawning and by merge logic to create correctly configured boombas.
  public virtual GameObject SpawnBoombaWithValue(Vector2 position, int value)
  {
    BoombaVariant variant = boombaVariants.Find(v => v.value == value);
    if (variant == null) return null;

    GameObject boomba = boombaPool.GetBoomba(position);
    boomba.transform.position = position;
    boomba.transform.localScale = Vector3.one * variant.size;

    SetupBoombaVisuals(boomba, variant);
    SetupBoombaProperties(boomba, variant);
    
    return boomba;
  }

  // What it does: Configures the visual rendering pipeline for a spawned boomba.
  // What it's used for: Sets up either sprite or animated visuals and assigns sorting order.
  private void SetupBoombaVisuals(GameObject boomba, BoombaVariant variant)
  {
    Transform art = boomba.transform.Find("VisualRoot");
    if (art == null) 
    {
      Debug.LogError("Boomba prefab missing child: VisualRoot");
      return;
    }

    // Assign unique sorting order
    var sortingGroup = art.GetComponent<UnityEngine.Rendering.SortingGroup>();
    if (sortingGroup != null)
      sortingGroup.sortingOrder = _nextSortingOrder++;

    Transform spriteArt = art.Find("SpriteArt");
    Transform animationArt = art.Find("AnimationArt");
    
    if (spriteArt == null || animationArt == null)
    {
      Debug.LogError("Boomba prefab missing SpriteArt or AnimationArt under VisualRoot");
      return;
    }

    // Reset both visual types (important for pooling)
    spriteArt.gameObject.SetActive(false);
    animationArt.gameObject.SetActive(false);

    // Clear previous animation visuals
    for (int i = animationArt.childCount - 1; i >= 0; i--)
      Destroy(animationArt.GetChild(i).gameObject);

    // Setup new visuals based on variant type
    if (variant.visualPrefab != null)
    {
      SetupAnimatedVisual(animationArt, variant);
    }
    else
    {
      SetupSpriteVisual(spriteArt, variant);
    }
  }

  // What it does: Instantiates and configures a Spine-based animated visual for a boomba.
  // What it's used for: Handles the setup of complex animated boombas.
  private void SetupAnimatedVisual(Transform animationArt, BoombaVariant variant)
  {
    animationArt.gameObject.SetActive(true);
    
    GameObject visual = Instantiate(variant.visualPrefab, animationArt);
    visual.transform.localPosition = Vector3.zero;
    visual.transform.localRotation = Quaternion.identity;
    visual.transform.localScale = Vector3.one * variant.ImageScale;
  }

  // What it does: Configures a simple sprite-based visual for a boomba.
  // What it's used for: Handles the setup of static sprite boombas.
  private void SetupSpriteVisual(Transform spriteArt, BoombaVariant variant)
  {
    spriteArt.gameObject.SetActive(true);
    
    SpriteRenderer sr = spriteArt.GetComponent<SpriteRenderer>();
    sr.sprite = variant.sprite;
    sr.transform.localScale = Vector3.one * variant.ImageScale;
  }

  // What it does: Sets the value and last-variant flag on a boomba's properties component.
  // What it's used for: Initializes the data/state that other systems read from the boomba.
  private void SetupBoombaProperties(GameObject boomba, BoombaVariant variant)
  {
    BoombaProperties props = boomba.GetComponent<BoombaProperties>();
    if (props == null) return;

    bool isLast = boombaVariants.Count > 0 && 
                  object.ReferenceEquals(variant, boombaVariants[boombaVariants.Count - 1]);
    
    props.IsLastVariant = isLast;
    props.SetValue(variant.value);
  }

  // What it does: Stops all spawn coroutines, clears the board, and starts a fresh initial wave.
  // What it's used for: Called by GameManager.Restart() to reset just the spawning system (not the full game).
  public void ResetAndStartInitialSpawn()
  {
    // Board already cleared by BoardCleaner
    StopAllCoroutines();
    InitialSpawnCompleted = false;
    StartCoroutine(SpawnBoombasWithDelay());
  }
}