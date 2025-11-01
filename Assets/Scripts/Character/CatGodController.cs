using UnityEngine;
using System.Collections;

public class CatGodController : MonoBehaviour
{
    private CatGodMover mover;

    private void Start()
    {
        mover = GetComponent<CatGodMover>();
        if (mover == null)
        {
            Debug.LogError("CatGodMover 컴포넌트를 찾을 수 없습니다. 고양이 신 프리팹에 CatGodMover 컴포넌트를 추가해주세요.");
            return;
        }
        StartCoroutine(StateMachine());
    }

    private IEnumerator StateMachine()
    {
        while (true)
        {
            // Lift/쿨다운/수동앉기 동안 대기
            yield return new WaitWhile(() => mover != null && (mover.IsLifted() || mover.IsResumeBlocked || mover.IsManualSit));

            mover.StartWandering();
            yield return InterruptibleDelay(Random.Range(5f, 6f));
            if (mover.IsLifted() || mover.IsResumeBlocked || mover.IsManualSit) continue;

            yield return new WaitWhile(() => mover.IsLifted() || mover.IsResumeBlocked || mover.IsManualSit);
            mover.Stop();
            yield return InterruptibleDelay(1f);
            if (mover.IsLifted() || mover.IsResumeBlocked || mover.IsManualSit) continue;

            yield return new WaitWhile(() => mover.IsLifted() || mover.IsResumeBlocked || mover.IsManualSit);
            mover.StartSitting(Random.Range(2f, 4f));
            yield return new WaitUntil(() => !mover.IsSitting() || mover.IsManualSit); // 수동앉기 전환 시 통과
        }
    }

    private IEnumerator InterruptibleDelay(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            if (mover == null || mover.IsLifted() || mover.IsResumeBlocked || mover.IsManualSit)
                yield break;
            t += Time.deltaTime;
            yield return null;
        }
    }
}