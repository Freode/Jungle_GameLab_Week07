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
            // Walking
            mover.StartWandering();
            yield return new WaitForSeconds(Random.Range(5f, 6f));

            // Idle
            mover.Stop();
            yield return new WaitForSeconds(1f);

            // Sitting
            mover.StartSitting(Random.Range(2f, 4f));
            yield return new WaitUntil(() => !mover.IsSitting());
        }
    }
}