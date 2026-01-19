// using UnityEngine;

// /// <summary>
// /// Spawns a held (static) boomba and doesn’t spawn the next one
// /// until the last dropped boomba collides with something.
// /// </summary>
// // What it does: Manages a “held” boomba the player can drop with a click, and waits for landing before spawning the next.
// // What it's used for: Implements the core input loop of the game—spawn one boomba above, let the player drop it, then repeat.
// public class BoombaTouchSpawner : MonoBehaviour
// {
//   [Header("Dependencies")]
//   public BoombaSpawner boombaSpawner;

//   [Header("Testing")]
//   public bool requireInitialSpawnCompleted = false; // test-only hook

//   [Header("Spawn Settings")]
//   public float spawnY = 5f;

//   private GameObject currentBoomba;       // held (static) boomba waiting to drop
//   private bool waitingForLanding = false; // true after releasing a boomba, until it collides
//   private bool canSpawnHeld = false;

//   // What it does: Subscribes to the BoombaSpawner’s initial spawn completion event.
//   // What it's used for: Delays enabling touch spawning until the initial random wave of boombas has finished spawning.
//   void OnEnable()
//   {
//     if (boombaSpawner != null)
//       boombaSpawner.OnInitialSpawnComplete += HandleInitialSpawnComplete;
//   }

//   // What it does: Unsubscribes from the BoombaSpawner’s event when this component is disabled.
//   // What it's used for: Prevents dangling event subscriptions if this spawner is turned off or destroyed.
//   void OnDisable()
//   {
//     if (boombaSpawner != null)
//       boombaSpawner.OnInitialSpawnComplete -= HandleInitialSpawnComplete;
//   }

//   // What it does: Marks that the spawner is now allowed to create the first held boomba.
//   // What it's used for: Used as the callback from BoombaSpawner to “arm” this touch spawner.
//   private void HandleInitialSpawnComplete()
//   {
//     canSpawnHeld = true;
//   }

//   // What it does: Handles the main touch/click loop—spawns a held boomba, then releases it on click once it exists.
//   // What it's used for: Runs every frame to manage when to create a new held boomba and when to drop it based on player input.
//   void Update()
//   {
//     // If event hasn’t fired yet, do nothing
//     if (!canSpawnHeld) return;

//     // If we’re allowed to spawn and there isn't a held boomba yet, make one
//     if (!waitingForLanding && currentBoomba == null)
//     {
//       currentBoomba = SpawnHeldBoomba();
//       return; // don’t process click on same frame
//     }

//     // Release on click
//     if (currentBoomba != null && Input.GetMouseButtonDown(0))
//     {
//       ReleaseCurrentBoomba();
//     }
//   }

//   // What it does: Spawns a new boomba at a fixed Y position and freezes it so it hovers until released.
//   // What it's used for: Creates the “held” boomba above the playfield that the player will later drop.
//   private GameObject SpawnHeldBoomba()
//   {
//     // Safety checks
//     if (boombaSpawner == null || boombaSpawner.boombaVariants == null || boombaSpawner.boombaVariants.Count == 0)
//     {
//       Debug.LogError("BoombaTouchSpawner: BoombaSpawner or variants not set.");
//       return null;
//     }

//     // Pick one of the first 3 variants (or fewer if not available)
//     int idx = Random.Range(0, Mathf.Min(3, boombaSpawner.boombaVariants.Count));
//     int value = boombaSpawner.boombaVariants[idx].value;

//     // Hold position: fixed Y, centered X (clamped to bounds just in case)
//     float startX = 0f;
//     float clampedX = Mathf.Clamp(startX, boombaSpawner.spawnAreaMin.x, boombaSpawner.spawnAreaMax.x);
//     Vector2 pos = new Vector2(clampedX, spawnY);

//     // Spawn via BoombaSpawner
//     GameObject boomba = boombaSpawner.SpawnBoombaWithValue(pos, value);
//     if (boomba == null) return null;

//     // Freeze so it doesn't fall yet
//     var rb = boomba.GetComponent<Rigidbody2D>();
//     if (rb != null) rb.bodyType = RigidbodyType2D.Static;

//     return boomba;
//   }

//   // What it does: Drops the currently held boomba at the click’s X position and arms a landing callback.
//   // What it's used for: Moves the held boomba to the clicked X location, unfreezes it, and waits until it lands before spawning another.
//   public void ReleaseCurrentBoomba()
//   {
//     if (currentBoomba == null) return;

//     // Move X to click/touch once (no tracking)
//     if (Camera.main != null)
//     {
//       Vector2 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//       float clampedX = Mathf.Clamp(world.x, boombaSpawner.spawnAreaMin.x, boombaSpawner.spawnAreaMax.x);
//       currentBoomba.transform.position = new Vector2(clampedX, spawnY);
//     }

//     var rb = currentBoomba.GetComponent<Rigidbody2D>();
//     if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;

//     // Add a one-shot landing reporter so we know when to spawn the next boomba
//     var reporter = currentBoomba.AddComponent<BoombaLandingReporter>();
//     reporter.Init(OnBoombaLanded);

//     // We’re now waiting for that collision before making a new boomba
//     waitingForLanding = true;
//     currentBoomba = null;
//   }

//   // What it does: Resets the waiting flag once the dropped boomba has collided with something.
//   // What it's used for: Signals that it’s safe to spawn another held boomba on the next Update.
//   private void OnBoombaLanded()
//   {
//     waitingForLanding = false; // next frame Update() will spawn a new held boomba
//   }

//   // What it does: Spawns a held boomba immediately for testing and returns the instance.
//   // What it's used for: Used by automated tests or debug tools to force creation of the held boomba without touch input.
//   public GameObject ForceSpawnHeldBoombaForTest()
//   {
//     if (currentBoomba == null)
//     {
//       currentBoomba = SpawnHeldBoomba();
//     }
//     return currentBoomba;
//   }
// }
