using UnityEngine;

public class ManualTransposerController : MonoBehaviour
{
    private Animator animator;
    private bool isCurrentlyCarrying = false; // 현재 광석을 들고 있는지 기억

    void Start()
    {
        // 시작할 때 자신의 Animator 컴포넌트를 찾아서 연결합니다.
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // --- 걷기 제어 (W 키) ---
        // W 키를 누르면 IsWalking을 true로 설정합니다.
        if (Input.GetKeyDown(KeyCode.W))
        {
            animator.SetBool("IsWalking", true);
        }
        // W 키에서 손을 떼면 IsWalking을 false로 설정합니다.
        if (Input.GetKeyUp(KeyCode.W))
        {
            animator.SetBool("IsWalking", false);
        }

        // --- 광석 들기 제어 (T 키) ---
        // T 키를 누르면 IsCarrying 상태를 뒤집습니다 (토글).
        if (Input.GetKeyDown(KeyCode.T))
        {
            // 현재 상태를 반대로 바꿉니다 (false -> true, true -> false).
            isCurrentlyCarrying = !isCurrentlyCarrying;

            // 바뀐 상태를 Animator에게 알려줍니다.
            animator.SetBool("IsCarrying", isCurrentlyCarrying);
        }
    }
}