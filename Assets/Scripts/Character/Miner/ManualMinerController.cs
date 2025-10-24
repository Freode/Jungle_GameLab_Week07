using UnityEngine;

public class ManualMinerController : MonoBehaviour
{
    private Animator animator;
    private bool isCurrentlyMining = false; // 현재 광질 중인지 기억하는 변수

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // --- 걷기 제어 (W 키) ---
        // (광질 중이 아닐 때만 걷도록 조건을 추가하면 더 좋습니다)
        if (!isCurrentlyMining)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                animator.SetBool("IsWalking", true);
            }
            if (Input.GetKeyUp(KeyCode.W))
            {
                animator.SetBool("IsWalking", false);
            }
        }

        // --- 광질 제어 (M 키) ---
        // M 키를 누르면 광질 상태를 뒤집는다 (토글)
        if (Input.GetKeyDown(KeyCode.M))
        {
            // 현재 상태를 반대로 바꿈 (false -> true, true -> false)
            isCurrentlyMining = !isCurrentlyMining;

            // 바뀐 상태를 Animator에게 알려줌
            animator.SetBool("IsMining", isCurrentlyMining);

            // 만약 광질을 시작했다면, 걷기 상태는 강제로 꺼준다.
            if (isCurrentlyMining)
            {
                animator.SetBool("IsWalking", false);
            }
        }
    }
}