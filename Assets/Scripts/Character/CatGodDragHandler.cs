using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CatGodDragHandler : MonoBehaviour
{
    [SerializeField] private int dragSortingOrder = 1000;

    // 선택: HoverTip 직접 제어(있으면 연결)
    [Header("Optional Hover Tip References")]
    [SerializeField] private GameObject tipRoot;          // 툴팁 루트(없으면 자동 탐색 시도)
    [SerializeField] private CanvasGroup tipCanvasGroup;  // 있으면 페이드 없이 alpha 0/1만 제어

    private Camera _cam;
    private CatGodMover _mover;
    private SpriteRenderer _sr;
    private Collider2D _col;

    // CatGodHoverTip 스크립트를 쓰고 있다면 자동으로 잡아서 Show/Hide 호출
    private CatGodHoverTip _hoverTip;

    private bool _dragging;
    private Vector3 _grabOffsetWS;
    private float _zCache;
    private int _originalSortingOrder;

    private void Awake()
    {
        _cam   = Camera.main;
        _mover = GetComponent<CatGodMover>();
        _sr    = GetComponent<SpriteRenderer>();
        _col   = GetComponent<Collider2D>();
        _hoverTip = GetComponent<CatGodHoverTip>();

        if (_mover == null)
            Debug.LogError("[CatGodDragHandler] CatGodMover가 필요합니다.");
        if (_cam == null)
            Debug.LogError("[CatGodDragHandler] Main Camera를 찾지 못했습니다.");

        // tipRoot / tipCanvasGroup 자동 탐색(선택)
        if (tipRoot == null)
        {
            var canvas = GetComponentInChildren<Canvas>(true);
            if (canvas != null) tipRoot = canvas.gameObject;
        }
        if (tipRoot != null && tipCanvasGroup == null)
            tipCanvasGroup = tipRoot.GetComponent<CanvasGroup>();
    }

    private void OnMouseDown()
    {
        if (_cam == null || _mover == null) return;

        _dragging = true;
        _zCache = transform.position.z;

        Vector3 mouseWS = ScreenToWorldOnZ(Input.mousePosition, _zCache);
        _grabOffsetWS = transform.position - mouseWS;

        _mover.OnLiftStart();

        // 드래그 시작 시 툴팁 강제 숨김
        HideTip();

        if (_sr != null)
        {
            _originalSortingOrder = _sr.sortingOrder;
            _sr.sortingOrder = dragSortingOrder;
        }
    }

    private void OnMouseUp()
    {
        if (!_dragging) return;
        _dragging = false;

        if (_sr != null) _sr.sortingOrder = _originalSortingOrder;

        _mover.OnLiftEnd();

        // 드롭 후, 여전히 고양이 위에 마우스가 있으면 다시 표시
        if (IsMouseOverSelf()) ShowTip();
        else HideTip();
    }

    private void OnMouseDrag()
    {
        if (!_dragging || _cam == null || _mover == null) return;

        Vector3 mouseWS = ScreenToWorldOnZ(Input.mousePosition, _zCache);
        Vector3 target  = mouseWS + _grabOffsetWS;

        // 드래그 중에는 항상 숨김 유지(보수적)
        HideTip();

        // 영역 밖이면 즉시 드래그 해제
        if (!_mover.IsInsideArea(target))
        {
            CancelDragBecauseOutOfBounds();
            return;
        }

        // 영역 안에서만 좌표 갱신
        transform.position = target;
    }

    private void CancelDragBecauseOutOfBounds()
    {
        _dragging = false;

        if (_sr != null)
            _sr.sortingOrder = _originalSortingOrder;

        // 드롭 처리(자동 idle 쿨다운 포함)
        _mover.OnLiftEnd();

        // 바깥으로 나가면서 해제된 경우는 호버 복구하지 않음
        HideTip();
    }

    private Vector3 ScreenToWorldOnZ(Vector3 screenPos, float z)
    {
        var sp = new Vector3(screenPos.x, screenPos.y, Mathf.Abs(_cam.transform.position.z - z));
        Vector3 ws = _cam.ScreenToWorldPoint(sp);
        ws.z = z;
        return ws;
    }

    // === Tooltip 제어 유틸 ===
    private void ShowTip()
    {
        if (_hoverTip != null)
        {
            _hoverTip.Show();
            return;
        }

        if (tipCanvasGroup != null)
        {
            if (tipRoot != null && !tipRoot.activeSelf) tipRoot.SetActive(true);
            tipCanvasGroup.alpha = 1f;
            return;
        }

        if (tipRoot != null) tipRoot.SetActive(true);
    }

    private void HideTip()
    {
        if (_hoverTip != null)
        {
            _hoverTip.Hide();
            return;
        }

        if (tipCanvasGroup != null)
        {
            tipCanvasGroup.alpha = 0f;
            // 완전히 숨길 때 비활성화까지 원하면:
            if (tipRoot != null) tipRoot.SetActive(false);
            return;
        }

        if (tipRoot != null) tipRoot.SetActive(false);
    }

    private bool IsMouseOverSelf()
    {
        if (_cam == null || _col == null) return false;

        var mp = Input.mousePosition;
        var wp = _cam.ScreenToWorldPoint(new Vector3(mp.x, mp.y, Mathf.Abs(_cam.transform.position.z - transform.position.z)));
        return _col.OverlapPoint(wp);
    }
}
