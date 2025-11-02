using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CatGodRightClickSitHandler : MonoBehaviour
{
    [Header("호버 안내 텍스트")]
    [SerializeField] private string tipSit = "우클릭: 앉기";
    [SerializeField] private string tipRelease = "우클릭: 풀기";

    private CatGodMover _mover;
    private CatGodHoverTip _hoverTip;
    private Camera _cam;
    private Collider2D _col;

    private bool _isHovering;

    private void Awake()
    {
        _mover = GetComponent<CatGodMover>();
        _hoverTip = GetComponent<CatGodHoverTip>();
        _col = GetComponent<Collider2D>();
        _cam = Camera.main;

        if (_mover == null)
            Debug.LogError("[CatGodRightClickSitHandler] CatGodMover가 필요합니다.");
        if (_col == null)
            Debug.LogError("[CatGodRightClickSitHandler] Collider2D가 필요합니다.");
    }

    private void Update()
    {
        if (_mover == null || _col == null || _cam == null) return;

        Vector2 mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
        bool nowHovering = _col.OverlapPoint(mousePos);

        // 호버 시작/종료 감지
        if (nowHovering && !_isHovering)
        {
            _isHovering = true;
            OnHoverEnter();
        }
        else if (!nowHovering && _isHovering)
        {
            _isHovering = false;
            OnHoverExit();
        }

        // 호버 중일 때만 우클릭 처리
        if (_isHovering && Input.GetMouseButtonDown(1))
        {
            // Lift 중에는 앉기 금지 (이제 ResumeBlocked은 무시)
            if (_mover.IsLifted()) return;

            if (!_mover.IsManualSit)
                _mover.EnableManualSit();
            else
                _mover.DisableManualSit();

            UpdateTip(); // 즉시 문구 갱신
        }
    }

    private void OnHoverEnter()
    {
        UpdateTip();
        if (_hoverTip != null)
            _hoverTip.Show();
    }

    private void OnHoverExit()
    {
        if (_hoverTip != null)
            _hoverTip.Hide();
    }

    private void UpdateTip()
    {
        if (_hoverTip == null) return;
        _hoverTip.SetTipText(_mover != null && _mover.IsManualSit ? tipRelease : tipSit);
    }
}
