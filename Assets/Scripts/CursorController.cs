using UnityEngine;
using System.Collections.Generic;

public class CursorController : MonoBehaviour
{
    public enum CursorState
    {
        Normal,
        Whip,
        // 필요에 따라 상태 추가 가능
    }

    [System.Serializable]
    public struct CursorData
    {
        public CursorState state;
        [Header("Normal Cursor (Mouse Not Pressed)")]
        public Texture2D normalTexture;
        public Vector2 normalHotspot;
        [Header("Left Click Cursor")]
        public Texture2D leftClickTexture;
        public Vector2 leftClickHotspot;
        [Header("Right Click Cursor")]
        public Texture2D rightClickTexture;
        public Vector2 rightClickHotspot;
    }

    [Header("Cursor State List")]
    [SerializeField] private CursorState currentState = CursorState.Normal;
    [SerializeField] private CursorData[] cursorStates;

    private Dictionary<CursorState, CursorData> cursorDict;
    private bool isLeftMousePressed;
    private bool isRightMousePressed;
    private CursorState previousState;

    // 성능을 위한 캐시된 참조값들
    private static readonly Vector2 DefaultHotspot = Vector2.zero;
    private const CursorMode DefaultCursorMode = CursorMode.Auto;

    public CursorState CurrentState => currentState;

    void Awake()
    {
        InitializeCursorDictionary();
        UpdateCursor();
    }

    void Update()
    {
        HandleMouseInput();
        HandleKeyboardInput();
    }

    /// <summary>
    /// O(1) 조회를 위한 커서 상태 딕셔너리 초기화
    /// </summary>
    private void InitializeCursorDictionary()
    {
        if (cursorStates == null || cursorStates.Length == 0)
        {
            Debug.LogWarning("[CursorController] 정의된 커서 상태가 없습니다!");
            return;
        }

        cursorDict = new Dictionary<CursorState, CursorData>(cursorStates.Length);
        foreach (var data in cursorStates)
        {
            if (cursorDict.ContainsKey(data.state))
            {
                Debug.LogWarning($"[CursorController] 중복된 커서 상태: {data.state}");
                continue;
            }
            cursorDict[data.state] = data;
        }
    }

    /// <summary>
    /// 좌클릭, 우클릭 입력을 효율적으로 처리
    /// </summary>
    private void HandleMouseInput()
    {
        bool currentLeftPressed = Input.GetMouseButton(0);
        bool currentRightPressed = Input.GetMouseButton(1);
        
        // 마우스 상태가 변경되면 커서 업데이트
        if (currentLeftPressed != isLeftMousePressed || currentRightPressed != isRightMousePressed)
        {
            isLeftMousePressed = currentLeftPressed;
            isRightMousePressed = currentRightPressed;
            UpdateCursor();
        }
    }

    /// <summary>
    /// 키보드 입력으로 상태 변경 처리
    /// </summary>
    private void HandleKeyboardInput()
    {
        // 키가 눌리지 않았으면 조기 반환
        if (!Input.anyKeyDown) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeCursorState(CursorState.Normal);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeCursorState(CursorState.Whip);
        }
    }

    /// <summary>
    /// 검증과 함께 커서 상태 변경
    /// </summary>
    /// <param name="newState">새로운 커서 상태</param>
    public void ChangeCursorState(CursorState newState)
    {
        if (currentState == newState) return; // 불필요한 업데이트 방지
        
        if (!IsStateAvailable(newState))
        {
            Debug.LogWarning($"[CursorController] 상태 {newState}를 사용할 수 없습니다!");
            return;
        }

        previousState = currentState;
        currentState = newState;
        UpdateCursor();
    }

    /// <summary>
    /// 리스트 인덱스로 커서 상태 변경 (UI 버튼 연결용)
    /// </summary>
    /// <param name="stateIndex">커서 상태 리스트의 인덱스 (0, 1, 2...)</param>
    public void ChangeCursorStateByIndex(int stateIndex)
    {
        if (cursorStates == null || stateIndex < 0 || stateIndex >= cursorStates.Length)
        {
            Debug.LogWarning($"[CursorController] 잘못된 상태 인덱스: {stateIndex}. 유효 범위: 0-{(cursorStates?.Length - 1 ?? -1)}");
            return;
        }

        CursorState targetState = cursorStates[stateIndex].state;
        ChangeCursorState(targetState);
        
        Debug.Log($"[CursorController] 버튼을 통해 상태 변경: 인덱스 {stateIndex} → {targetState}");
    }

    /// <summary>
    /// 이전 커서 상태로 되돌리기
    /// </summary>
    public void RevertToPreviousState()
    {
        if (previousState != currentState)
        {
            ChangeCursorState(previousState);
        }
    }

    /// <summary>
    /// 현재 상태와 마우스 입력에 따라 커서 업데이트
    /// </summary>
    private void UpdateCursor()
    {
        if (cursorDict == null || !cursorDict.TryGetValue(currentState, out var data))
        {
            SetDefaultCursor();
            return;
        }

        var (texture, hotspot) = GetCursorTexture(data);
        Cursor.SetCursor(texture, hotspot, DefaultCursorMode);
    }

    /// <summary>
    /// 마우스 상태에 따라 적절한 커서 텍스처와 핫스팟 가져오기
    /// </summary>
    /// <param name="data">커서 데이터</param>
    /// <returns>텍스처와 핫스팟 튜플</returns>
    private (Texture2D texture, Vector2 hotspot) GetCursorTexture(CursorData data)
    {
        // 우클릭이 우선순위가 높음
        if (isRightMousePressed)
        {
            return (data.rightClickTexture, data.rightClickHotspot);
        }
        else if (isLeftMousePressed)
        {
            return (data.leftClickTexture, data.leftClickHotspot);
        }
        else
        {
            return (data.normalTexture, data.normalHotspot);
        }
    }

    /// <summary>
    /// 기본 시스템 커서로 설정
    /// </summary>
    private void SetDefaultCursor()
    {
        Cursor.SetCursor(null, DefaultHotspot, DefaultCursorMode);
    }

    /// <summary>
    /// 특정 상태가 사용 가능한지 확인
    /// </summary>
    /// <param name="state">확인할 상태</param>
    /// <returns>사용 가능 여부</returns>
    public bool IsStateAvailable(CursorState state)
    {
        return cursorDict?.ContainsKey(state) ?? false;
    }

    /// <summary>
    /// 사용 가능한 모든 커서 상태 가져오기
    /// </summary>
    /// <returns>사용 가능한 상태 배열</returns>
    public CursorState[] GetAvailableStates()
    {
        if (cursorDict == null) return new CursorState[0];
        
        var states = new CursorState[cursorDict.Count];
        cursorDict.Keys.CopyTo(states, 0);
        return states;
    }

    /// <summary>
    /// 현재 상태의 리스트 인덱스 가져오기
    /// </summary>
    /// <returns>현재 상태의 인덱스, 찾을 수 없으면 -1</returns>
    public int GetCurrentStateIndex()
    {
        if (cursorStates == null) return -1;
        
        for (int i = 0; i < cursorStates.Length; i++)
        {
            if (cursorStates[i].state == currentState)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// 현재 마우스 상태 확인
    /// </summary>
    /// <returns>마우스 상태 문자열</returns>
    public string GetCurrentMouseState()
    {
        if (isRightMousePressed) return "Right Click";
        if (isLeftMousePressed) return "Left Click";
        return "Normal";
    }

    /// <summary>
    /// 좌클릭 상태 확인
    /// </summary>
    public bool IsLeftMousePressed => isLeftMousePressed;

    /// <summary>
    /// 우클릭 상태 확인
    /// </summary>
    public bool IsRightMousePressed => isRightMousePressed;

    /// <summary>
    /// 상태 리스트 정보를 디버그 로그로 출력 (UI 설정 도움용)
    /// </summary>
    [ContextMenu("상태 리스트 정보 출력")]
    public void PrintStateListInfo()
    {
        if (cursorStates == null || cursorStates.Length == 0)
        {
            Debug.Log("[CursorController] 설정된 커서 상태가 없습니다.");
            return;
        }

        Debug.Log("[CursorController] 커서 상태 리스트:");
        for (int i = 0; i < cursorStates.Length; i++)
        {
            Debug.Log($"  인덱스 {i}: {cursorStates[i].state}");
        }
        Debug.Log("UI 버튼 OnClick에서 ChangeCursorStateByIndex(인덱스)를 호출하세요.");
        Debug.Log($"현재 상태: {currentState} (인덱스: {GetCurrentStateIndex()})");
        Debug.Log($"현재 마우스 상태: {GetCurrentMouseState()}");
    }

#if UNITY_EDITOR
    /// <summary>
    /// 에디터에서 커서 상태 검증
    /// </summary>
    void OnValidate()
    {
        if (cursorStates == null) return;

        // 중복 상태 확인
        var stateSet = new HashSet<CursorState>();
        foreach (var data in cursorStates)
        {
            if (!stateSet.Add(data.state))
            {
                Debug.LogWarning($"[CursorController] 중복된 상태 발견: {data.state}");
            }
        }
    }
#endif
}
