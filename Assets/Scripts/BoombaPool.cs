using System.Collections.Generic;
using UnityEngine;

public class BoombaPool : MonoBehaviour
{
  public GameObject boombaPrefab;   // Prefab to instantiate when creating new boombas
  public int poolSize = 20;       // Initial number of pooled boombas

  private List<GameObject> boombaPool;

  public void Initialize()
  {
    boombaPool = new List<GameObject>();
    for (int i = 0; i < poolSize; i++)
    {
      GameObject boomba = Instantiate(boombaPrefab);
      boomba.SetActive(false);
      boombaPool.Add(boomba);
    }
  }


  void Awake()
  {
    Initialize(); // still runs during real game play
  }


  // Retrieves an inactive boomba from the pool and repositions it
  public GameObject GetBoomba(Vector2 position)
  {
    foreach (GameObject boomba in boombaPool)
    {
      if (boomba != null && !boomba.activeInHierarchy)
      {
        boomba.transform.position = position;
        boomba.SetActive(true);
        return boomba;
      }
    }

    // If none available, create a new one
    GameObject newBoomba = Instantiate(boombaPrefab);
    newBoomba.transform.position = position;
    newBoomba.SetActive(true);
    boombaPool.Add(newBoomba);
    return newBoomba;
  }

  // Returns all boombas in the pool (used for resetting or restarts)
  public List<GameObject> GetAllBoombas()
  {
    return boombaPool;
  }
}
