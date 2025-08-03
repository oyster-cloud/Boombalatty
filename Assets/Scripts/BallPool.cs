using System.Collections.Generic;
using UnityEngine;

public class BallPool : MonoBehaviour
{
  public GameObject ballPrefab;   // Prefab to instantiate when creating new balls
  public int poolSize = 20;       // Initial number of pooled balls

  private List<GameObject> ballPool;

  void Awake()
  {
    // Pre-instantiate balls and disable them
    ballPool = new List<GameObject>();
    for (int i = 0; i < poolSize; i++)
    {
      GameObject ball = Instantiate(ballPrefab);
      ball.SetActive(false);
      ballPool.Add(ball);
    }
  }

  // Retrieves an inactive ball from the pool and repositions it
  public GameObject GetBall(Vector2 position)
  {
    foreach (GameObject ball in ballPool)
    {
      if (ball != null && !ball.activeInHierarchy)
      {
        ball.transform.position = position;
        ball.SetActive(true);
        return ball;
      }
    }

    // If none available, create a new one
    GameObject newBall = Instantiate(ballPrefab);
    newBall.transform.position = position;
    newBall.SetActive(true);
    ballPool.Add(newBall);
    return newBall;
  }

  // Returns all balls in the pool (used for resetting or restarts)
  public List<GameObject> GetAllBalls()
  {
    return ballPool;
  }
}
