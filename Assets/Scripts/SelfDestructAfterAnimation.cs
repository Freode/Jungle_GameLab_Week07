// File name: SelfDestructAfterAnimation.cs
using System.Collections;
using UnityEngine;

/// <summary>
/// Returns the object to the pool (or destroys it) when the animator finishes playing.
/// </summary>
[RequireComponent(typeof(Animator))]
public class SelfDestructAfterAnimation : MonoBehaviour
{
    [SerializeField] private float additionalDelay = 0f;

    private Animator cachedAnimator;
    private Coroutine returnRoutine;

    void Awake()
    {
        cachedAnimator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
        }

        if (cachedAnimator != null)
        {
            cachedAnimator.Play(0, 0, 0f);
            cachedAnimator.Update(0f);
        }

        returnRoutine = StartCoroutine(ReturnAfterAnimation());
    }

    void OnDisable()
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }
    }

    private IEnumerator ReturnAfterAnimation()
    {
        float waitTime = 0.5f;

        if (cachedAnimator != null)
        {
            float clipLength = 0f;
            var clipInfos = cachedAnimator.GetCurrentAnimatorClipInfo(0);
            if (clipInfos.Length > 0 && clipInfos[0].clip != null)
            {
                clipLength = clipInfos[0].clip.length;
            }
            else
            {
                clipLength = cachedAnimator.GetCurrentAnimatorStateInfo(0).length;
            }

            float speed = Mathf.Approximately(cachedAnimator.speed, 0f) ? 1f : Mathf.Abs(cachedAnimator.speed);
            waitTime = Mathf.Max(0.05f, clipLength / speed);
        }

        yield return new WaitForSeconds(waitTime + Mathf.Max(0f, additionalDelay));

        if (TryGetComponent<ObjectPoolInfo>(out var poolInfo))
        {
            poolInfo.ReturnToPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
