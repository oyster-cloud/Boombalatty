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
  [SerializeField] int startingBoombaCount = 3; // Starting number of boombas
  [SerializeField] int boombaCountIncrement = 1; // How much to increase per whale completion
  [SerializeField] int maxBoombaCount = 10; // Cap for boomba count
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
  private int _currentBoombaCount; // Tracks the current difficulty level
  private bool _hasLoadedSavedState = false; // Track if we've loaded from save

  public int CurrentBoombaCount => _currentBoombaCount; // Expose for saving
  public int StartingBoombaCount => startingBoombaCount; // Expose for loading

  private bool _canStartSpawning = false;

  void OnEnable()
  {
    Services.BoombaSpawner = this;
    SplashScreen.OnSplashComplete += HandleSplashComplete;
  }

  void OnDisable()
  {
    if (Services.BoombaSpawner == this)
      Services.BoombaSpawner = null;
    
    SplashScreen.OnSplashComplete -= HandleSplashComplete;
  }

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

  void HandleSplashComplete()
  {
    _canStartSpawning = true;
    Debug.Log("Splash complete - spawning enabled");
  }

  void Start()
  {
    Initialize();
    
    // Only auto-start if GameManager hasn't loaded saved state
    if (!_hasLoadedSavedState)
    {
      _currentBoombaCount = startingBoombaCount;
      InitialSpawnCompleted = false;
      StartCoroutine(WaitForSplashThenSpawn());
    }
  }

  IEnumerator WaitForSplashThenSpawn()
  {
    // Wait until splash completes
    while (!_canStartSpawning)
      yield return null;
    
    Debug.Log("Starting spawn after splash");
    StartCoroutine(SpawnBoombasWithDelay());
  }

  IEnumerator SpawnBoombasWithDelay()
  {
    // Spawn exact count - no range
    int boombaCount = _currentBoombaCount;
    
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

    InitialSpawnCompleted = true;
    OnInitialSpawnComplete?.Invoke();
  }

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

  private void SetupBoombaVisuals(GameObject boomba, BoombaVariant variant)
  {
    Transform art = boomba.transform.Find("VisualRoot");
    if (art == null) 
    {
      Debug.LogError("Boomba prefab missing child: VisualRoot");
      return;
    }

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

    spriteArt.gameObject.SetActive(false);
    animationArt.gameObject.SetActive(false);

    for (int i = animationArt.childCount - 1; i >= 0; i--)
      Destroy(animationArt.GetChild(i).gameObject);

    if (variant.visualPrefab != null)
    {
      SetupAnimatedVisual(animationArt, variant);
    }
    else
    {
      SetupSpriteVisual(spriteArt, variant);
    }
  }

  private void SetupAnimatedVisual(Transform animationArt, BoombaVariant variant)
  {
    animationArt.gameObject.SetActive(true);
    
    GameObject visual = Instantiate(variant.visualPrefab, animationArt);
    visual.transform.localPosition = Vector3.zero;
    visual.transform.localRotation = Quaternion.identity;
    visual.transform.localScale = Vector3.one * variant.ImageScale;
  }

  private void SetupSpriteVisual(Transform spriteArt, BoombaVariant variant)
  {
    spriteArt.gameObject.SetActive(true);
    
    SpriteRenderer sr = spriteArt.GetComponent<SpriteRenderer>();
    sr.sprite = variant.sprite;
    sr.transform.localScale = Vector3.one * variant.ImageScale;
  }

  private void SetupBoombaProperties(GameObject boomba, BoombaVariant variant)
  {
    BoombaProperties props = boomba.GetComponent<BoombaProperties>();
    if (props == null) return;

    bool isLast = boombaVariants.Count > 0 && 
                  object.ReferenceEquals(variant, boombaVariants[boombaVariants.Count - 1]);
    
    props.IsLastVariant = isLast;
    props.SetValue(variant.value);
  }

  // What it does: Loads saved state and starts spawning with that difficulty after splash completes.
  // What it's used for: Called by GameManager on startup to restore progress.
  public void LoadSavedStateAndStart(int savedBoombaCount)
  {
    _currentBoombaCount = Mathf.Clamp(savedBoombaCount, startingBoombaCount, maxBoombaCount);
    _hasLoadedSavedState = true;
    InitialSpawnCompleted = false;
    
    // Also wait for splash before spawning
    StartCoroutine(WaitForSplashThenSpawn());
    
    Debug.Log($"BoombaSpawner loaded saved state: {_currentBoombaCount} boombas - waiting for splash");
  }

  // What it does: Resets spawning and starts a new wave, optionally increasing difficulty.
  // What it's used for: Called by GameManager on restart. Pass true to increase difficulty after whale completion.
  public void ResetAndStartInitialSpawn(bool increaseDifficulty = false)
  {
    if (increaseDifficulty)
    {
      _currentBoombaCount = Mathf.Min(_currentBoombaCount + boombaCountIncrement, maxBoombaCount);
      Debug.Log($"Difficulty increased! Boomba count: {_currentBoombaCount}");
    }

    StopAllCoroutines();
    InitialSpawnCompleted = false;
    StartCoroutine(SpawnBoombasWithDelay());
  }

  // What it does: Resets difficulty and boomba count to starting values.
  // What it's used for: Called when player resets progress or after game over.
  public void ResetToStartingDifficulty()
  {
    _currentBoombaCount = startingBoombaCount;
    _hasLoadedSavedState = false;
  }
}
