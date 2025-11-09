using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Represents a specific type of boomba with size, sprite, and value
[System.Serializable]
public class BoombaVariant
{
  public Sprite sprite;
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

  void OnEnable()
  {
    Services.BoombaSpawner = this;
  }

  void OnDisable()
  {
    if (Services.BoombaSpawner == this)
      Services.BoombaSpawner = null;
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

  void Start()
  {
    // Spawn boombas when game starts
    InitialSpawnCompleted = false;
    StartCoroutine(SpawnBoombasWithDelay());
  }

  void Update()
  {
    // Press 'R' to restart (for testing)
    if (Input.GetKeyDown(KeyCode.R))
    {
      RestartGame();
    }
  }

  // Coroutine to spawn a series of boombas with delay
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

  // Allows spawning a boomba with a specific value (e.g. for merges)
  public virtual GameObject SpawnBoombaWithValue(Vector2 position, int value)
  {
    BoombaVariant variant = boombaVariants.Find(v => v.value == value);

    if (variant == null)
    {
      return null;
    }

    GameObject boomba = boombaPool.GetBoomba(position);
    boomba.transform.localScale = Vector3.one * variant.size;

    SpriteRenderer renderer = boomba.GetComponentInChildren<SpriteRenderer>();
    if (renderer != null)
    {
      renderer.sprite = variant.sprite;

      // Optional: scale only the image, not the collider/physics
      renderer.transform.localScale = Vector3.one * variant.ImageScale; // e.g., 0.8f
    }

    BoombaProperties props = boomba.GetComponent<BoombaProperties>();
    if (props != null)
    {
      bool isLast = boombaVariants.Count > 0 &&
              object.ReferenceEquals(variant, boombaVariants[boombaVariants.Count - 1]);
      props.IsLastVariant = isLast;

      props.SetValue(variant.value);
    }
    
    return boomba;
  }

  // Clears existing boombas and restarts spawning
  public void RestartGame()
  {
    foreach (GameObject boomba in boombaPool.GetAllBoombas())
    {
      boomba.SetActive(false);
    }

    InitialSpawnCompleted = false;
    StartCoroutine(SpawnBoombasWithDelay());
  }

  // BoombaSpawner.cs
  public void ResetAndStartInitialSpawn()
  {
    StopAllCoroutines();
    InitialSpawnCompleted = false;
    StartCoroutine(SpawnBoombasWithDelay());
  }
}
