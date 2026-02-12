using System.Collections.Generic;
using UnityEngine;

public class BoombaPool : MonoBehaviour
{
  public GameObject boombaPrefab;   // Prefab to instantiate when creating new boombas
  public int poolSize = 20;       // Initial number of pooled boombas

  private List<GameObject> boombaPool;

  // What it does: Pre-instantiates a set of inactive boomba objects and stores them in a list.
  // What it's used for: Sets up the object pool so the game can reuse boombas instead of constantly instantiating/destroying.
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

  // What it does: Automatically initializes the pool when the component awakens.
  // What it's used for: Ensures the BoombaPool is ready to serve objects as soon as the scene starts.
  void Awake()
  {
    Initialize(); // still runs during real game play
  }

  // What it does: Returns an inactive boomba from the pool (or creates a new one) and positions/activates it.
  // What it's used for: Called whenever the game needs a boomba instance, minimizing runtime instantiation costs.
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

  // What it does: Returns the internal list of all boombas managed by this pool.
  // What it's used for: Used by systems like restart logic to iterate over and reset all pooled boombas.
  public List<GameObject> GetAllBoombas()
  {
    return boombaPool;
  }

  /// <summary>
  /// Fills the given set with all distinct values currently present
  /// on ACTIVE boombas in this pool.
  /// </summary>
  public HashSet<int> GetActiveBoombaValues()
  {
    var result = new HashSet<int>();
    if (boombaPool == null) return result;

    foreach (var go in boombaPool)
    {
      if (!go || !go.activeInHierarchy) continue;
      var props = go.GetComponent<BoombaProperties>();
      if (props && props.Value != 0 && props.Value != 7)
        result.Add(props.Value);
    }

    return result;
  }
}
