using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AutoDestroyAfterAnimation : MonoBehaviour
{
  void Start()
  {
    Animator animator = GetComponent<Animator>();
    AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
    if (clipInfo.Length > 0)
    {
      float clipLength = clipInfo[0].clip.length;
      Destroy(gameObject, clipLength);
    }
    else
    {
        // Fallback in case there's no clip info
      Destroy(gameObject, 1f);
    }
  }
}
