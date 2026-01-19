using UnityEngine;

// What it does: Holds static references to key gameplay services accessible from anywhere.
// What it's used for: Provides a simple, lightweight service locator for spawners and ceiling logic without full dependency injection.
public static class Services
{
  // What it does: Reference to the active BoombaSpawner in the scene.
// What it's used for: Lets other systems (like GameManager or merge logic) spawn boombas without manual wiring.
  public static BoombaSpawner BoombaSpawner { get; internal set; }

  // What it does: Reference to the active SnackTouchSpawner in the scene.
// What it's used for: Allows UI or GameManager to reset or control snack spawning globally.
  public static SnackTouchSpawner SnackTouchSpawner { get; internal set; }

  // What it does: Reference to the active ceiling trigger that ends the game when crossed.
// What it's used for: Used to reset or re-arm ceiling detection as part of restart logic.
  public static EndGameWhenBoombaCrossesCeiling Ceiling { get; internal set; } 
}
