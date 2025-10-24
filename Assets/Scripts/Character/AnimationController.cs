using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [Header("Animation Components")]
    [SerializeField] private Animator animator;
    
    [Header("Animation States")]
    [SerializeField] private bool isWalking;
    [SerializeField] private bool isRunning;
    [SerializeField] private bool isSwimming;
    [SerializeField] private bool isDoing;
    [SerializeField] private bool isAxe;
    [SerializeField] private bool isMining;
    [SerializeField] private bool isHammering;
    [SerializeField] private bool isDigging;
    [SerializeField] private bool isCarrying;
    [SerializeField] private bool isDropping;
    [SerializeField] private bool isHanging;
    [SerializeField] private bool isDead;
    
    [Header("Animation Parameters")]
    public string walkParameter = "IsWalking";
    public string runParameter = "IsRunning";
    public string swimParameter = "IsSwimming";
    public string doParameter = "IsDoing";
    public string axeParameter = "IsAxe";
    public string miningParameter = "IsMining";
    public string hammerParameter = "IsHamering";
    public string digParameter = "IsDigging";
    public string carryParameter = "IsCarrying";
    public string droppingTrigger = "IsDropping";
    public string hangingParameter = "IsHanging";
    public string deathParameter = "Death";
    
    void Start()
    {
        // Animator 컴포넌트가 할당되지 않았다면 자동으로 찾기
        if (animator == null)
            animator = GetComponent<Animator>();
            
        // Animator가 없다면 경고 출력
        if (animator == null)
            Debug.LogError("Animator component not found on " + gameObject.name);
    }
    
    void Update()
    {
        // 현재 상태를 Animator에 반영
        UpdateAnimatorParameters();
    }
    
    #region Public Animation Control Methods
    
    /// <summary>
    /// 걷기 애니메이션 설정
    /// </summary>
    public void SetWalking(bool value)
    {
        isWalking = value;
        if (animator != null)
            animator.SetBool(walkParameter, value);
    }
    
    /// <summary>
    /// 달리기 애니메이션 설정
    /// </summary>
    public void SetRunning(bool value)
    {
        isRunning = value;
        if (animator != null)
            animator.SetBool(runParameter, value);
    }
    
    /// <summary>
    /// 수영 애니메이션 설정
    /// </summary>
    public void SetSwimming(bool value)
    {
        isSwimming = value;
        if (animator != null)
            animator.SetBool(swimParameter, value);
    }
    
    /// <summary>
    /// 떨어뜨리기 애니메이션 트리거
    /// </summary>
    public void TriggerDropping()
    {
        if (animator != null)
            animator.SetTrigger(droppingTrigger);
    }
    
    /// <summary>
    /// 매달리기 애니메이션 설정
    /// </summary>
    public void SetHanging(bool value)
    {
        isHanging = value;
        if (animator != null)
            animator.SetBool(hangingParameter, value);
    }
    
    /// <summary>
    /// 일반 행동 애니메이션 설정
    /// </summary>
    public void SetDoing(bool value)
    {
        isDoing = value;
        if (animator != null)
            animator.SetBool(doParameter, value);
    }
    
    /// <summary>
    /// 도끼 사용 애니메이션 설정
    /// </summary>
    public void SetAxe(bool value)
    {
        isAxe = value;
        if (animator != null)
            animator.SetBool(axeParameter, value);
    }
    
    /// <summary>
    /// 채굴 애니메이션 설정
    /// </summary>
    public void SetMining(bool value)
    {
        isMining = value;
        if (animator != null)
            animator.SetBool(miningParameter, value);
    }
    
    /// <summary>
    /// 망치질 애니메이션 설정
    /// </summary>
    public void SetHammering(bool value)
    {
        isHammering = value;
        if (animator != null)
            animator.SetBool(hammerParameter, value);
    }
    
    /// <summary>
    /// 파기 애니메이션 설정
    /// </summary>
    public void SetDigging(bool value)
    {
        isDigging = value;
        if (animator != null)
            animator.SetBool(digParameter, value);
    }
    
    /// <summary>
    /// 운반 애니메이션 설정
    /// </summary>
    public void SetCarrying(bool value)
    {
        isCarrying = value;
        if (animator != null)
            animator.SetBool(carryParameter, value);
    }
    
    /// <summary>
    /// 죽음 애니메이션 설정
    /// </summary>
    public void SetDeath(bool value)
    {
        isDead = value;
        if (value)
            ResetAllStates();
        if (animator != null)
            animator.SetBool(deathParameter, value);
    }
    
    /// <summary>
    /// 모든 상태 초기화 (Idle 상태로 돌아가기)
    /// </summary>
    public void ResetAllStates()
    {
        isWalking = false;
        isRunning = false;
        isSwimming = false;
        isDoing = false;
        isAxe = false;
        isMining = false;
        isHammering = false;
        isDigging = false;
        isDropping = false;
        isCarrying = false;
        isHanging = false;
        isDead = false;
        
        if (animator != null)
        {
            animator.SetBool(walkParameter, false);
            animator.SetBool(runParameter, false);
            animator.SetBool(swimParameter, false);
            animator.SetBool(doParameter, false);
            animator.SetBool(axeParameter, false);
            animator.SetBool(miningParameter, false);
            animator.SetBool(hammerParameter, false);
            animator.SetBool(digParameter, false);
            animator.SetBool(carryParameter, false);
            animator.SetBool(droppingTrigger, false);
            animator.SetBool(hangingParameter, false);
            animator.SetBool(deathParameter, false);
        }
    }
    
    #endregion
    
    #region Animation State Getters
    
    public bool IsWalking => isWalking;
    public bool IsRunning => isRunning;
    public bool IsSwimming => isSwimming;
    public bool IsDoing => isDoing;
    public bool IsAxe => isAxe;
    public bool IsMining => isMining;
    public bool IsHammering => isHammering;
    public bool IsDigging => isDigging;
    public bool IsCarrying => isCarrying;
    public bool IsDropping => isDropping;
    public bool IsHanging => isHanging;
    public bool IsDead => isDead;
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Animator 매개변수를 현재 상태와 동기화
    /// </summary>
    private void UpdateAnimatorParameters()
    {
        if (animator == null) return;
        
        // 각 상태를 Animator에 전달
        animator.SetBool(walkParameter, isWalking);
        animator.SetBool(runParameter, isRunning);
        animator.SetBool(swimParameter, isSwimming);
        animator.SetBool(doParameter, isDoing);
        animator.SetBool(axeParameter, isAxe);
        animator.SetBool(miningParameter, isMining);
        animator.SetBool(hammerParameter, isHammering);
        animator.SetBool(digParameter, isDigging);
        animator.SetBool(carryParameter, isCarrying);
        animator.SetBool(droppingTrigger, isDropping);
        animator.SetBool(hangingParameter, isHanging);
        animator.SetBool(deathParameter, isDead);
    }
    
    /// <summary>
    /// 현재 재생 중인 애니메이션 상태 이름 반환
    /// </summary>
    public string GetCurrentStateName()
    {
        if (animator == null) return "None";
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName("Entry") ? "Entry" : 
               stateInfo.IsName("Idle") ? "Idle" :
               stateInfo.IsName("walk") ? "Walk" :
               stateInfo.IsName("run") ? "Run" :
               stateInfo.IsName("carry") ? "Carry" :
               stateInfo.IsName("swimming") ? "Swimming" :
               stateInfo.IsName("doing") ? "Doing" :
               stateInfo.IsName("axe") ? "Axe" :
               stateInfo.IsName("mining") ? "Mining" :
               stateInfo.IsName("hammering") ? "Hammering" :
               stateInfo.IsName("dig") ? "Dig" :
               stateInfo.IsName("dropping") ? "Dropping" :
               stateInfo.IsName("hanging") ? "Hanging" :
               stateInfo.IsName("death") ? "Death" : "Unknown";
    }
    
    /// <summary>
    /// 애니메이션이 완료되었는지 확인
    /// </summary>
    public bool IsAnimationComplete()
    {
        if (animator == null) return true;
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.normalizedTime >= 1.0f && !animator.IsInTransition(0);
    }
    
    /// <summary>
    /// 애니메이션 디버깅 정보 출력
    /// </summary>
    public void DebugAnimationInfo()
    {
        if (animator == null)
        {
            Debug.LogError("Animator가 없습니다!");
            return;
        }
        
        Debug.Log($"현재 애니메이션 상태: {GetCurrentStateName()}");
        Debug.Log($"걷기: {isWalking}, 달리기: {isRunning}, 수영: {isSwimming}");
        Debug.Log($"행동: {isDoing}, 운반: {isCarrying}, 떨어뜨리기: {isDropping},매달리기: {isHanging}");
        Debug.Log($"도끼: {isAxe}, 채굴: {isMining}, 망치: {isHammering}, 파기: {isDigging}");
        Debug.Log($"죽음: {isDead}");
        
        // Animator Parameters 확인
        Debug.Log("사용 가능한 파라미터들:");
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            Debug.Log($"- {param.name} ({param.type})");
        }
    }
    
    #endregion
}
