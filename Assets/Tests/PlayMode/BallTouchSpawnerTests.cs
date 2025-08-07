using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class BallTouchSpawnerTests
{
  private GameObject cameraObj;
  private Camera mainCamera;

  private GameObject spawnerObj;
  private BallTouchSpawner touchSpawner;
  private MockBallSpawner mockSpawner;

  [SetUp]
  public void SetUp()
  {
    // Set up Camera
    cameraObj = new GameObject("MainCamera");
    mainCamera = cameraObj.AddComponent<Camera>();
    cameraObj.tag = "MainCamera"; // Required for Camera.main to work

    // Set up BallSpawner
    GameObject ballSpawnerGO = new GameObject("BallSpawner");
    mockSpawner = ballSpawnerGO.AddComponent<MockBallSpawner>();
    mockSpawner.spawnAreaMin = new Vector2(-5, -5);
    mockSpawner.spawnAreaMax = new Vector2(5, 5);

    // Set up BallTouchSpawner
    spawnerObj = new GameObject("BallTouchSpawner");
    touchSpawner = spawnerObj.AddComponent<BallTouchSpawner>();
    touchSpawner.ballSpawner = mockSpawner;
  }

  [TearDown]
  public void TearDown()
  {
    Object.DestroyImmediate(spawnerObj);
    Object.DestroyImmediate(mockSpawner.gameObject);
    Object.DestroyImmediate(cameraObj);
  }

  [UnityTest]
  public IEnumerator Click_SpawnsBallWithinClampedArea()
  {
      Vector2 screenPos = new Vector2(Screen.width / 2f, Screen.height / 2f);

      yield return null; // Let Unity init everything

      touchSpawner.HandleTouch(screenPos); // Simulate touch

      Assert.IsTrue(mockSpawner.WasCalled, "BallSpawner was not called.");
      Vector2 pos = mockSpawner.LastSpawnPosition;

      Assert.That(pos.x, Is.InRange(mockSpawner.spawnAreaMin.x, mockSpawner.spawnAreaMax.x));
      Assert.That(pos.y, Is.InRange(mockSpawner.spawnAreaMin.y, mockSpawner.spawnAreaMax.y));
  }

  // Mock BallSpawner for testing
  private class MockBallSpawner : BallSpawner
  {
      public bool WasCalled { get; private set; } = false;
      public Vector2 LastSpawnPosition { get; private set; }

      // Override Start to prevent coroutine from running
      void Start() { }

      public override GameObject SpawnBallWithValue(Vector2 position, int value)
      {
          WasCalled = true;
          LastSpawnPosition = position;
          return new GameObject("MockBall");
      }
  }

  // Simulate mouse click (requires InputTestTools if you want deeper control)
  private static class InputSimulator
  {
    public static void ClickAt(Vector3 screenPosition)
    {
        // Unity doesn't allow setting Input.mousePosition directly
        // So you have to simulate it indirectly or use packages like InputSystem
        // This method is mostly a placeholder unless you integrate with a tool
    }
  }
}
