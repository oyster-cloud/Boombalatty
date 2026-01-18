using System.Collections;
using UnityEngine;
using Spine;
using Spine.Unity;

public class BoombaSpineIdleRandomizer : MonoBehaviour
{
  [SerializeField] private SkeletonAnimation skeletonAnimation;

  [SpineAnimation] public string idleAnimation = "still";
  public string[] wiggleAnimations = { "arm_wave" };

  [Header("Timing")]
  public float minIdleTime = 1.5f;
  public float maxIdleTime = 4f;

  private Coroutine routine;

  void Awake()
  {
    if (!skeletonAnimation)
      skeletonAnimation = GetComponent<SkeletonAnimation>();
  }

  void OnEnable()
  {
    if (!skeletonAnimation) return;

    skeletonAnimation.Initialize(true);

    // Start in idle (loop)
    skeletonAnimation.AnimationState.SetAnimation(0, idleAnimation, true);

    if (routine != null) StopCoroutine(routine);
    routine = StartCoroutine(IdleRoutine());
  }

  void OnDisable()
  {
    if (routine != null)
    {
      StopCoroutine(routine);
      routine = null;
    }
  }

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
