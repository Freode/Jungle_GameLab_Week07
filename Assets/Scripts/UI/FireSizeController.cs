using UnityEngine;
using System.Collections.Generic; // List를 사용하기 위해 추가

/// <summary>
/// 권위 단계(레벨) 방송을 수신하여, 모든 자손 Fire UI의 RectTransform 크기를
/// 미리 설정된 값으로 변경하는 컨트롤러입니다. FireUI 오브젝트에 붙여서 사용합니다.
/// </summary>
public class FireSizeController : MonoBehaviour
{
    // [System.Serializable]을 붙이면 인스펙터 창에 노출됩니다.
    [System.Serializable]
    public class LevelSizeData
    {
        [Tooltip("이 설정을 적용할 권위 레벨입니다. (예: 0, 1, 2...)")]
        public int level;
        [Tooltip("해당 레벨에서 불꽃 UI들이 가질 Width와 Height 값입니다.")]
        public Vector2 fireSize = new Vector2(200, 200); // 기본값 설정
    }

    [Header("구독할 방송 채널")]
    [Tooltip("AuthorityManager가 사용하는 레벨/색상 변경 채널 에셋을 연결하세요.")]
    public AuthorityLevelChangeEventChannelSO authorityLevelChannel;

    [Header("레벨별 불꽃 크기 설정")]
    [Tooltip("각 권위 레벨에 따라 적용할 불꽃의 크기를 설정합니다.")]
    public List<LevelSizeData> levelSizeSettings;

    // 오브젝트 활성화 시 방송 구독
    private void OnEnable()
    {
        if (authorityLevelChannel != null)
        {
            authorityLevelChannel.OnEventRaised += OnAuthorityLevelChanged;
        }
    }

    // 오브젝트 비활성화 시 구독 해제 (메모리 누수 방지)
    private void OnDisable()
    {
        if (authorityLevelChannel != null)
        {
            authorityLevelChannel.OnEventRaised -= OnAuthorityLevelChanged;
        }
    }

    /// <summary>
    /// 방송을 수신했을 때 호출되는 메인 함수
    /// </summary>
    private void OnAuthorityLevelChanged(int newLevel, Color color)
    {
        // ★★★ 핵심 수정 로직 시작 ★★★

        // 1. 만약 새로운 레벨이 0이라면?
        if (newLevel == 0)
        {
            // 모든 불꽃 컨테이너(Top, Bottom...)를 꺼버립니다.
            Debug.Log($"<color=grey>FireUI: 레벨 0 방송 수신! 모든 불꽃을 끕니다.</color>");
            SetChildrenActive(false);
            return; // 여기서 함수를 종료합니다.
        }

        // 2. 레벨이 0이 아니라면, 일단 모든 불꽃 컨테이너를 다시 켭니다.
        SetChildrenActive(true);

        // 3. 기존 로직을 그대로 수행하여 크기를 조절합니다.
        LevelSizeData targetSetting = levelSizeSettings.Find(setting => setting.level == newLevel);

        if (targetSetting != null)
        {
            Debug.Log($"<color=yellow>FireUI: 레벨 {newLevel} 방송 수신! 크기를 {targetSetting.fireSize}로 변경합니다.</color>");
            ApplySizeToGrandchildren(targetSetting.fireSize);
        }
        else
        {
            Debug.LogWarning($"FireUI: 레벨 {newLevel}에 대한 크기 설정이 없습니다. 크기를 변경하지 않습니다.");
        }
    }
    
    /// <summary>
    /// 이 오브젝트의 모든 직계 자식(Top, Bottom, Left, Right)을 켜거나 끕니다.
    /// </summary>
    private void SetChildrenActive(bool isActive)
    {
        foreach (Transform directChild in transform)
        {
            // 자식의 활성화 상태가 이미 원하는 상태와 같다면 굳이 변경하지 않습니다. (최적화)
            if (directChild.gameObject.activeSelf != isActive)
            {
                directChild.gameObject.SetActive(isActive);
            }
        }
    }

    /// <summary>
    /// 이 오브젝트의 모든 '손주' 오브젝트를 찾아 RectTransform 크기를 변경합니다.
    /// </summary>
    private void ApplySizeToGrandchildren(Vector2 newSize)
    {
        // 1. 나의 직계 자식들을 순회합니다 (Top, Bottom, Left, Right)
        foreach (Transform directChild in transform)
        {
            // 2. 그 자식들의 자식들(나의 손주들)을 순회합니다 (개별 Fire UI들)
            foreach (Transform grandchild in directChild)
            {
                // 3. 손주에게 RectTransform 컴포넌트가 있는지 확인하고, 있다면 크기를 변경합니다.
                if (grandchild.TryGetComponent<RectTransform>(out RectTransform rectTransform))
                {
                    rectTransform.sizeDelta = newSize;
                }
            }
        }
    }
}