using System.Collections.Generic;
using UnityEngine;

public class AnimalPool : MonoBehaviour
{
  public GameObject ballPrefab;   // Prefab to instantiate when creating new balls
  public int poolSize = 20;       // Initial number of pooled balls

  private List<GameObject> ballPool;

  public void Initialize()
  {
    ballPool = new List<GameObject>();
    for (int i = 0; i < poolSize; i++)
    {
      GameObject ball = Instantiate(ballPrefab);
      ball.SetActive(false);
      ballPool.Add(ball);
    }
  }


  void Awake()
  {
    Initialize(); // still runs during real game play
  }


  // Retrieves an inactive ball from the pool and repositions it
  public GameObject GetAnimal(Vector2 position)
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
    GameObject newAnimal = Instantiate(ballPrefab);
    newAnimal.transform.position = position;
    newAnimal.SetActive(true);
    ballPool.Add(newAnimal);
    return newAnimal;
  }

  // Returns all balls in the pool (used for resetting or restarts)
  public List<GameObject> GetAllAnimals()
  {
    return ballPool;
  }
}
