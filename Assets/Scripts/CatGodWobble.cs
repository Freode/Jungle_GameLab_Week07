using UnityEngine;

/// <summary>
/// 드래그 중 좌우 흔들림(달랑달랑) 효과 전담.
/// - DragHandler에서 OnDrag마다 Nudge(horizontalVelocity) 호출
/// - Lift 시작/종료에 맞춰 OnDragStart/OnDragEnd 호출
/// 회전은 localRotation(Z축), 바운스는 localPosition Y를 약간 보정
/// </summary>
public class CatGodWobble : MonoBehaviour
{
    [Header("대상 트랜스폼(미지정 시 this)")]
    [SerializeField] private Transform target;

    [Header("회전 세팅")]
    [SerializeField] private float maxAngleDeg = 12f;          // 최대 기울기
    [SerializeField] private float torqueScale = 0.0025f;      // 입력→각속도 변환 스케일
    [SerializeField] private float angularDamping = 6f;        // 감쇠(클수록 빨리 멈춤)
    [SerializeField] private float springReturn = 80f;         // 원점 복귀 스프링 강도

    [Header("바운스(상하) 세팅")]
    [SerializeField] private float bobAmplitude = 0.05f;       // Y 바운스 크기
    [SerializeField] private float bobSpeedScale = 0.004f;     // 속도→바운스 속도 스케일

    [Header("드래그 중 강화 옵션")]
    [SerializeField] private float dragDampingMultiplier = 0.8f; // 드래그 중 감쇠 약하게(더 흔들림)
    [SerializeField] private float dragSpringMultiplier  = 0.8f;  // 드래그 중 복귀 힘 약하게

    private float _angle;            // 현재 각도(deg)
    private float _angularVel;       // 각속도(deg/s)
    private float _bobPhase;         // 바운스 위상
    private Vector3 _baseLocalPos;   // 원래 로컬 위치
    private bool _dragging;

    private void Awake()
    {
        if (target == null) target = transform;
        _baseLocalPos = target.localPosition;
    }

    public void OnDragStart()
    {
        _dragging = true;
    }

    public void OnDragEnd()
    {
        _dragging = false;
    }

    /// <summary>
    /// 좌우 입력(속도)을 받아 흔들림에 토크를 가한다.
    /// vX는 "좌우 이동 속도" 개념이면 됨(월드/스크린 기준 무관, 일관되게만).
    /// </summary>
    public void Nudge(float horizontalVelocity)
    {
        _angularVel += -horizontalVelocity * torqueScale; // 좌우 반응 방향성
        // 바운스 위상은 속도 크기에 비례해 가속
        _bobPhase += Mathf.Abs(horizontalVelocity) * bobSpeedScale * Time.deltaTime;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // 드래그 상태에 따른 파라미터 보정
        float damp = angularDamping * (_dragging ? dragDampingMultiplier : 1f);
        float spring = springReturn * (_dragging ? dragSpringMultiplier : 1f);

        // 스프링(각도 원점 복귀) + 감쇠(마찰)
        float springAccel = -spring * _angle;        // 각 가속
        float damping     = -damp * _angularVel;

        _angularVel += (springAccel + damping) * dt;
        _angle += _angularVel * dt;

        // 최대 각도 제한
        _angle = Mathf.Clamp(_angle, -maxAngleDeg, maxAngleDeg);

        // 회전 적용(Z축)
        target.localRotation = Quaternion.Euler(0f, 0f, _angle);

        // 바운스 적용(Y 위치 살짝 위아래)
        float bob = Mathf.Sin(_bobPhase) * bobAmplitude;
        target.localPosition = new Vector3(_baseLocalPos.x, _baseLocalPos.y + bob, _baseLocalPos.z);

        // 드래그가 완전히 끝나고 거의 정지하면 위치/회전 복귀 스냅
        if (!_dragging && Mathf.Abs(_angle) < 0.05f && Mathf.Abs(_angularVel) < 0.05f)
        {
            _angle = 0f;
            _angularVel = 0f;
            _bobPhase = 0f;
            target.localRotation = Quaternion.identity;
            target.localPosition = _baseLocalPos;
        }
    }
}
