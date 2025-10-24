// 파일 이름: CitizenNameTag.cs (개정안)
using UnityEngine;
using TMPro;

public class CitizenNameTag : MonoBehaviour
{
    [Header("이름표 UI 요소")]
    public GameObject nameTagObject;
    public TextMeshProUGUI nameText;

    [Header("구독할 방송 채널")]
    public NameTagEventChannelSO onNameTagStateChangeChannel;

    private PeopleActor selfActor;

    void Awake()
    {
        selfActor = GetComponent<PeopleActor>();
    }

    // ★★★ 수정: 게임 시작 시 '왕의 인장'을 확인하도록 변경 ★★★
    void Start()
    {
        // 임무 시작 시, 이 백성이 이미 이름을 하사받은 몸인지 확인합니다.
        // (예: 저장된 게임을 불러왔을 경우)
        if (selfActor.HasReceivedRoyalName)
        {
            ShowNameTag();
        }
        else
        {
            HideNameTag();
        }
    }

    private void OnEnable()
    {
        // 임무 시작 시, 이 백성이 이미 이름을 하사받은 몸인지 확인합니다.
        // (예: 저장된 게임을 불러왔을 경우)
        if (selfActor.HasReceivedRoyalName)
        {
            ShowNameTag();
        }
        else
        {
            HideNameTag();
        }

        if (onNameTagStateChangeChannel != null)
        {
            onNameTagStateChangeChannel.OnEventRaised += HandleNameTagEvent;
        }
    }

    private void OnDisable()
    {
        HideNameTag();
        if (onNameTagStateChangeChannel != null)
        {
            onNameTagStateChangeChannel.OnEventRaised -= HandleNameTagEvent;
        }
    }

    private void HandleNameTagEvent(GameObject targetCitizen, bool shouldShow)
    {
        if (targetCitizen != this.gameObject) return;
        
        if (shouldShow)
        {
            ShowNameTag();
        }
        else
        {
            HideNameTag();
        }
    }

    // 이름표를 켜고 이름을 갱신하는 임무
    private void ShowNameTag()
    {
        if (nameTagObject == null || nameText == null || selfActor == null) return;
        nameText.text = selfActor.DisplayName;
        nameTagObject.SetActive(true);
    }

    // 이름표를 끄는 임무
    private void HideNameTag()
    {
        if (nameTagObject != null)
        {
            nameTagObject.SetActive(false);
        }
    }
}