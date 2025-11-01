using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CatGodRightClickSitHandler : MonoBehaviour
{
    [Header("호버 안내 텍스트")]
    [SerializeField] private string tipSit = "우클릭: 앉기";
    [SerializeField] private string tipRelease = "우클릭: 풀기";

    private CatGodMover _mover;
    private CatGodHoverTip _hoverTip;

    private void Awake()
    {
        _mover = GetComponent<CatGodMover>();
        _hoverTip = GetComponent<CatGodHoverTip>();

        if (_mover == null)
            Debug.LogError("[CatGodRightClickSitHandler] CatGodMover가 필요합니다.");
    }

    // 마우스가 고양이 위에 있는 동안: 항상 현재 상태에 맞춰 텍스트 갱신
    private void OnMouseOver()
    {
        if (_mover == null) return;

        // 호버 중에는 계속 현재 상태에 맞는 문구 유지
        UpdateTip();

        // 우클릭 토글
        if (Input.GetMouseButtonDown(1))
        {
            // 들고 있거나 드롭 쿨다운 중이면 무시 (원치 않으면 이 줄 제거)
            if (_mover.IsLifted() || _mover.IsResumeBlocked) return;

            if (!_mover.IsManualSit) _mover.EnableManualSit();
            else _mover.DisableManualSit();

            // 토글 직후 문구 갱신 (호버 중이면 그대로 표시됨)
            UpdateTip();
        }
    }

    private void UpdateTip()
    {
        if (_hoverTip == null) return;
        // 수동 앉기 중이면 "풀기", 아니면 "앉기" 안내
        _hoverTip.SetTipText(_mover != null && _mover.IsManualSit ? tipRelease : tipSit);
    }
}
