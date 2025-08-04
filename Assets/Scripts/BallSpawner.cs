using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Represents a specific type of ball with size, sprite, and value
[System.Serializable]
public class BallVariant
{
  public Sprite sprite;
  [Range(0.3f, 3.0f)]
  public float size;
  public int value;
}

public class BallSpawner : MonoBehaviour
{
  [Header("Dependencies")]
  public BallPool ballPool;

  [Header("Spawn Settings")]
  public int minBalls = 3;
  public int maxBalls = 7;
  private Vector2 spawnAreaMin = new Vector2(-2.5f, 3f);
  private Vector2 spawnAreaMax = new Vector2(2.5f, 3f);
  public float spawnDelay = 0.8f;

  [Header("Ball Variants")]
  public List<BallVariant> ballVariants = new List<BallVariant>();

  public void Initialize()
  {
    if (ballPool == null)
    {
      Debug.LogError("BallPool is not assigned in BallSpawner!");
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
    StartCoroutine(SpawnBallsWithDelay());
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
  IEnumerator SpawnBallsWithDelay()
  {
    if (ballPool == null)
    {
      Debug.LogError("BallPool is not assigned in BallSpawner!");
      yield break;
    }

    if (ballVariants == null || ballVariants.Count == 0)
    {
      Debug.LogError("No ball variants assigned!");
      yield break;
    }

    int ballCount = Random.Range(minBalls, maxBalls + 1);

    for (int i = 0; i < ballCount; i++)
    {
      Vector2 randomPos = new Vector2(
        Random.Range(spawnAreaMin.x, spawnAreaMax.x),
        Random.Range(spawnAreaMin.y, spawnAreaMax.y)
      );

      int limit = Mathf.CeilToInt(ballVariants.Count / 2f);
      int variantIndex = Random.Range(0, limit);
      BallVariant variant = ballVariants[variantIndex];

      GameObject ball = ballPool.GetBall(randomPos);
      ball.transform.localScale = Vector3.one * variant.size;

      SpriteRenderer renderer = ball.GetComponent<SpriteRenderer>();
      if (renderer != null)
      {
        renderer.sprite = variant.sprite;
      }

      BallProperties props = ball.GetComponent<BallProperties>();
      if (props != null)
      {
        props.SetValue(variant.value);
      }

      yield return new WaitForSeconds(spawnDelay);
    }
  }

  // Allows spawning a ball with a specific value (e.g. for merges)
  public GameObject SpawnBallWithValue(Vector2 position, int value)
  {
    BallVariant variant = ballVariants.Find(v => v.value == value);

    if (variant == null)
    {
      Debug.Log($"No variant found with value: {value}");
      return null;
    }

    GameObject ball = ballPool.GetBall(position);
    ball.transform.localScale = Vector3.one * variant.size;

    SpriteRenderer renderer = ball.GetComponent<SpriteRenderer>();
    if (renderer != null)
    {
      renderer.sprite = variant.sprite;
    }

    BallProperties props = ball.GetComponent<BallProperties>();
    if (props != null)
    {
      props.SetValue(variant.value);
    }

    return ball;
  }

  // Clears existing balls and restarts spawning
  public void RestartGame()
  {
    foreach (GameObject ball in ballPool.GetAllBalls())
    {
      ball.SetActive(false);
    }

    StartCoroutine(SpawnBallsWithDelay());
  }
}
