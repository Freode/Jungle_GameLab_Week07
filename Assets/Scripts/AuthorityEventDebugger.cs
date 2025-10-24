using UnityEngine;

/// <summary>
/// AuthorityLevelChangeEventChannelSO 채널에서 보내는 방송을 수신하여
/// 콘솔에 로그를 출력하는 디버깅용 스크립트입니다.
/// </summary>
public class AuthorityEventDebugger : MonoBehaviour
{
    [Header("구독할 방송 채널")]
    [Tooltip("AuthorityManager가 사용하는 레벨/색상 변경 채널 에셋을 연결하세요.")]
    public AuthorityLevelChangeEventChannelSO authorityLevelChannel;

    // 이 오브젝트가 활성화될 때 방송 구독을 시작합니다.
    private void OnEnable()
    {
        if (authorityLevelChannel != null)
        {
            authorityLevelChannel.OnEventRaised += HandleAuthorityLevelChange;
        }
    }

    // 이 오브젝트가 비활성화될 때 방송 구독을 해제합니다. (메모리 누수 방지)
    private void OnDisable()
    {
        if (authorityLevelChannel != null)
        {
            authorityLevelChannel.OnEventRaised -= HandleAuthorityLevelChange;
        }
    }

    /// <summary>
    /// 방송이 수신되었을 때 호출될 함수입니다.
    /// </summary>
    /// <param name="level">수신된 권위 레벨</param>
    /// <param name="color">수신된 현재 색상</param>
    private void HandleAuthorityLevelChange(int level, Color color)
    {
        // 수신된 색상을 HEX 코드로 변환 (로그에 색을 입히기 위함)
        string colorHex = ColorUtility.ToHtmlStringRGB(color);

        // 콘솔 창에 불구경 로그를 출력합니다!
        Debug.Log($"<color=orange>🔥 방송 수신 (불구경 중) 🔥</color>\n" +
                  $"<b>현재 권위 단계:</b> {GetLevelName(level)}\n" +
                  $"<b>수신된 색상 코드:</b> <color=#{colorHex}>■■■</color> (#{colorHex})");
    }

    // 레벨 숫자를 보기 좋은 이름으로 바꿔주는 도우미 함수
    private string GetLevelName(int level)
    {
        switch (level)
        {
            case 0: return "<b>0단계 (시작)</b>";
            case 1: return "<b>1단계 (성장)</b>";
            case 2: return "<b>2단계 (핵심)</b>";
            case 3: return "<b>3단계 (권위)</b>";
            case 4: return "<b>4단계 (정점)</b>";
            case 5: return "<b>5단계 (초월)</b>";
            case 6: return "<color=red><b>6단계 (피버타임!!!)</b></color>";
            default: return $"<b>{level}단계 (알 수 없음)</b>";
        }
    }
}