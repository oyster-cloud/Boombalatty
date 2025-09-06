using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Represents a specific type of ball with size, sprite, and value
[System.Serializable]
public class AnimalVariant
{
  public Sprite sprite;
  [Range(0.3f, 5.0f)]
  public float size;
  public int value;
  [Range(0.1f, 2f)]
  public float ImageScale = 1f;
}

public class AnimalSpawner : MonoBehaviour
{
  [Header("Dependencies")]
  public AnimalPool ballPool;

  [Header("Testing")]
  public bool requireInitialSpawnCompleted = false; // test-only hook

  [Header("Spawn Settings")]
  public int minAnimals = 3;
  public int maxAnimals = 7;
  public Vector2 spawnAreaMin = new Vector2(-2.5f, 3f);
  public Vector2 spawnAreaMax = new Vector2(2.5f, 3f);
  public float spawnDelay = 0.8f;

  [Header("Animal Variants")]
  public List<AnimalVariant> ballVariants = new List<AnimalVariant>();

  /// True once the initial delayed spawn wave completes.
  public bool InitialSpawnCompleted { get; private set; } = false;

  /// Fired once when the initial wave finishes.
  public event System.Action OnInitialSpawnComplete;

  public void Initialize()
  {
    if (ballPool == null)
    {
      Debug.LogError("AnimalPool is not assigned in AnimalSpawner!");
      return;
    }

    if (ballVariants == null || ballVariants.Count == 0)
    {
      Debug.LogError("No ball variants assigned!");
      return;
    }
  }

  void Start()
  {
    // Spawn balls when game starts
    InitialSpawnCompleted = false;
    StartCoroutine(SpawnAnimalsWithDelay());
  }

  void Update()
  {
    // Press 'R' to restart (for testing)
    if (Input.GetKeyDown(KeyCode.R))
    {
      RestartGame();
    }
  }

  // Coroutine to spawn a series of balls with delay
  IEnumerator SpawnAnimalsWithDelay()
  {
    if (ballPool == null)
    {
      Debug.LogError("AnimalPool is not assigned in AnimalSpawner!");
      yield break;
    }

    if (ballVariants == null || ballVariants.Count == 0)
    {
      Debug.LogError("No ball variants assigned!");
      yield break;
    }

    int ballCount = Random.Range(minAnimals, maxAnimals + 1);

    for (int i = 0; i < ballCount; i++)
    {
      Vector2 randomPos = new Vector2(
        Random.Range(spawnAreaMin.x, spawnAreaMax.x),
        Random.Range(spawnAreaMin.y, spawnAreaMax.y)
      );

      int limit = Mathf.CeilToInt(ballVariants.Count / 2f);
      int variantIndex = Random.Range(0, limit);

      AnimalVariant variant = ballVariants[variantIndex];

      SpawnAnimalWithValue(randomPos, variant.value);

      yield return new WaitForSeconds(spawnDelay);
    }

    // ✅ mark complete and notify
    InitialSpawnCompleted = true;
    OnInitialSpawnComplete?.Invoke();
  }

  // Allows spawning a ball with a specific value (e.g. for merges)
  public virtual GameObject SpawnAnimalWithValue(Vector2 position, int value)
  {
    AnimalVariant variant = ballVariants.Find(v => v.value == value);

    if (variant == null)
    {
      Debug.Log($"No variant found with value: {value}");
      return null;
    }

    GameObject ball = ballPool.GetAnimal(position);
    ball.transform.localScale = Vector3.one * variant.size;

    SpriteRenderer renderer = ball.GetComponentInChildren<SpriteRenderer>();
    // Debug.Log($"renderer ==> : {renderer}");
    // renderer.sprite = variant.sprite;
    if (renderer != null)
    {
      renderer.sprite = variant.sprite;

      // Optional: scale only the image, not the collider/physics
      renderer.transform.localScale = Vector3.one * variant.ImageScale; // e.g., 0.8f
    }

    AnimalProperties props = ball.GetComponent<AnimalProperties>();
    if (props != null)
    {
      bool isLast = ballVariants.Count > 0 && object.ReferenceEquals(variant, ballVariants[6]);
      props.IsLastVariant = isLast;

      props.SetValue(variant.value);
    }

    return ball;
  }

  // Clears existing balls and restarts spawning
  public void RestartGame()
  {
    foreach (GameObject ball in ballPool.GetAllAnimals())
    {
      ball.SetActive(false);
    }

    InitialSpawnCompleted = false;
    StartCoroutine(SpawnAnimalsWithDelay());
  }
}
