using System.Collections;
using UnityEngine;
using Spine;
using Spine.Unity;

// What it does: Randomly alternates a Spine skeleton between a looping idle animation and occasional "wiggle" animations.
// What it's used for: Gives Boomba Spine characters a bit of life by triggering random idle motions without extra game logic.
public class BoombaSpineIdleRandomizer : MonoBehaviour
{
  [SerializeField] private SkeletonAnimation skeletonAnimation;

  [SpineAnimation] public string idleAnimation = "still";
  public string[] wiggleAnimations = { "arm_wave" };

  [Header("Timing")]
  public float minIdleTime = 1.5f;
  public float maxIdleTime = 4f;

  private Coroutine routine;

  // What it does: Ensures the SkeletonAnimation reference is set, defaulting to the component on the same GameObject.
  // What it's used for: Avoids manual wiring in the inspector and guarantees the script has a target skeleton to animate.
  void Awake()
  {
    if (!skeletonAnimation)
      skeletonAnimation = GetComponent<SkeletonAnimation>();
  }

  // What it does: Initializes the skeleton, starts the idle animation, and begins the coroutine that triggers random wiggles.
  // What it's used for: Automatically starts the idle/wiggle behavior whenever the Spine GameObject is enabled or reused from a pool.
  void OnEnable()
  {
    if (!skeletonAnimation) return;

    skeletonAnimation.Initialize(true);

    // Start in idle (loop)
    skeletonAnimation.AnimationState.SetAnimation(0, idleAnimation, true);

    if (routine != null) StopCoroutine(routine);
    routine = StartCoroutine(IdleRoutine());
  }

  // What it does: Stops the idle coroutine when the object is disabled and clears the handle.
  // What it's used for: Prevents coroutines from running on inactive/pooled objects and avoids duplicate routines.
  void OnDisable()
  {
    if (routine != null)
    {
      StopCoroutine(routine);
      routine = null;
    }
  }

  // What it does: Waits a random time in idle, plays a random wiggle animation once, then returns to idle in a loop.
  // What it's used for: Drives the timing pattern of idle → wiggle → idle so Spine characters feel alive without extra scene logic.
  private IEnumerator IdleRoutine()
  {
    var state = skeletonAnimation.AnimationState;

    while (true)
    {
      // Random wait while idling
      float wait = Random.Range(minIdleTime, maxIdleTime);
      yield return new WaitForSeconds(wait);

      if (wiggleAnimations == null || wiggleAnimations.Length == 0)
        continue;

      // Pick and play a wiggle once
      string wiggle = wiggleAnimations[Random.Range(0, wiggleAnimations.Length)];
      TrackEntry entry = state.SetAnimation(0, wiggle, false);

      // Wait until the wiggle completes (pool-safe)
      if (entry != null)
      {
        // Duration can be 0 if animation missing; guard.
        float dur = Mathf.Max(0.01f, entry.AnimationEnd - entry.AnimationStart);
        yield return new WaitForSeconds(dur / Mathf.Max(0.01f, entry.TimeScale));
      }

      // Return to idle (loop)
      state.SetAnimation(0, idleAnimation, true);
    }
  }
}
