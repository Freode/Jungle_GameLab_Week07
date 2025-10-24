// 파일 이름: PeopleInfoUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections;

// 인스펙터에 노출시키기 위한 데이터 묶음 클래스
[System.Serializable]
public class JobVisual
{
    public JobType job;
    public Sprite portrait; // 초상화
    public VideoClip video;  // 직업 영상
}

public class PeopleInfoUI : MonoBehaviour
{
    [Header("Event Channels")]
    public PeopleActorEventChannelSO OnPeopleSelectedChannel;
    public VoidEventChannelSO OnDeselectedChannel;
    public VoidEventChannelSO OnYearPassedChannel;
    public DraggableSkullEventChannelSO OnSkullSelectedChannel;
    public HighlightEventChannelSO OnHighlightChannel;
    public NameTagEventChannelSO OnNameTagStateChangeChannel;

    [Header("UI Components")]
    public GameObject infoPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI ageText;
    public TextMeshProUGUI jobText;
    public TextMeshProUGUI loyaltyText;
    public RawImage portraitOrVideoImage;
    public VideoPlayer videoPlayer;
    public RenderTexture videoRenderTexture;

    [Header("Name Change UI")]
    public GameObject displayGroup;     // 이름 표시 그룹
    public GameObject editGroup;        // 이름 편집 그룹
    public TMP_InputField nameInputField; // 이름 입력창
    [Tooltip("이름을 하사했을 때 상승할 충성심의 양입니다.")]
    public int nameBestowalLoyaltyBonus = 10;

    [Header("Skull Specific UI")]
    public GameObject preserveToggleObject; // 체크박스와 텍스트를 포함한 부모 오브젝트
    public Toggle preserveToggle;           // 실제 Toggle 컴포넌트

    [Header("Job Visuals Data")]
    public JobVisual[] jobVisuals;
    // ★ 유골 전용 초상화 추가
    public Sprite skullSprite;

    // 현재 UI에 정보를 표시하고 있는 Actor를 저장하는 변수
    private PeopleActor currentActor;
    private DraggableSkull currentSkull;
    private void OnEnable()
    {
        OnPeopleSelectedChannel.OnEventRaised += OnPeopleSelected;
        OnDeselectedChannel.OnEventRaised += HideUI;
        OnYearPassedChannel.OnEventRaised += OnYearPassed;
        OnSkullSelectedChannel.OnEventRaised += OnSkullSelected;
    }

    private void OnDisable()
    {
        OnPeopleSelectedChannel.OnEventRaised -= OnPeopleSelected;
        OnDeselectedChannel.OnEventRaised -= HideUI;
        OnYearPassedChannel.OnEventRaised -= OnYearPassed;
        OnSkullSelectedChannel.OnEventRaised -= OnSkullSelected;
    }
    // '1년 지남' 방송을 받으면 호출되는 함수 (수정됨)
    private void OnYearPassed()
    {
        // 정보창이 켜져있을 때만 지연 보고 코루틴을 시작시킴
        if (infoPanel.activeSelf && currentActor != null)
        {
            StartCoroutine(RefreshAgeAfterDelay());
        }
    }
    // ★ 유골이 선택되었을 때 호출될 새 함수
    private void OnSkullSelected(DraggableSkull skull)
    {
        currentActor = null; // 유골은 살아있는 Actor가 아니므로 null로 설정
        currentSkull = skull;
        infoPanel.SetActive(true);

        // 유골의 정보로 UI 텍스트 업데이트
        nameText.text = $"이름: {skull.DeceasedName} (故)";
        ageText.text = $"향년: {skull.AgeAtDeath}";
        jobText.text = $"생전 직업: {skull.JobAtDeath}";
        loyaltyText.text = $"생전 충성도: {skull.LoyaltyAtDeath}";

        // 비주얼을 해골 초상화로 고정
        videoPlayer.enabled = false;
        portraitOrVideoImage.enabled = true;
        portraitOrVideoImage.texture = skullSprite.texture;

        preserveToggleObject.SetActive(true);
        preserveToggle.isOn = currentSkull.IsPreserved;

        // 유골은 이름을 변경할 수 없으므로, 무조건 보기 모드로 설정
        displayGroup.SetActive(true);
        editGroup.SetActive(false);
    }
    
    // ★ 새로 추가된 지연 함수 (코루틴)
    private IEnumerator RefreshAgeAfterDelay()
    {
        // 폐하의 명대로 0.1초를 기다립니다.
        yield return new WaitForSeconds(0.1f);

        // 0.1초 후, 나이 담당관이 일을 마쳤을 것이므로
        // 그 때 다시 장군(currentActor)에게 최신 나이를 물어보고 보고서를 갱신합니다.
        // 혹시 그 사이에 창이 꺼졌을 경우를 대비해 한번 더 확인합니다.
        if (infoPanel.activeSelf && currentActor != null)
        {
            ageText.text = $"나이: {currentActor.Age.ToString()}";
        }
    }

    // '선택됨' 방송을 받으면 호출되는 메인 함수
    private void OnPeopleSelected(PeopleActor selectedActor)
    {
        OnHighlightChannel?.RaiseEvent(selectedActor.gameObject, true);
        // 현재 선택된 actor를 클래스 변수에 저장해서 다른 함수에서도 쓸 수 있게 함
        currentActor = selectedActor;
        currentSkull = null;

        infoPanel.SetActive(true);
        preserveToggleObject.SetActive(false);

        // 모든 UI 텍스트 정보 업데이트
        nameText.text = $"이름: {currentActor.DisplayName}";
        ageText.text = $"나이: {currentActor.Age.ToString()}";
        jobText.text = $"직업: {currentActor.Job.ToString()}";
        loyaltyText.text = $"충성도: {currentActor.Loyalty.ToString()}";

        // 직업에 맞는 비주얼(초상화/비디오) 업데이트
        UpdateVisuals(currentActor.Job);

        // 이름 변경 중에 다른 사람을 선택했을 경우를 대비해, 기본 보기 모드로 전환
        ExitEditMode();
    }
    public void OnPreserveToggleChanged()
    {
        // 현재 선택된 유골이 있을 때만 작동
        if (currentSkull != null)
        {
            // 체크박스의 현재 상태(true/false)를 유골에게 전달
            currentSkull.SetPreservation(preserveToggle.isOn);
        }
    }
    
    // --- 이름 변경 관련 함수들 ---

    // '변경' 버튼을 누르면 호출 (인스펙터에서 연결)
    public void EnterEditMode()
    {
        if (currentActor == null) return; // 선택된 대상이 없으면 실행 안함

        displayGroup.SetActive(false);
        editGroup.SetActive(true);
        nameInputField.text = currentActor.DisplayName; // 입력창에 현재 이름 채워넣기
        nameInputField.Select(); // 입력창에 커서 바로 활성화
    }

    // '취소' 버튼을 누르면 호출 (인스펙터에서 연결)
    public void ExitEditMode()
    {
        displayGroup.SetActive(true);
        editGroup.SetActive(false);
    }
    
    // '확인' 버튼을 누르면 호출 (인스펙터에서 연결)
    public void ConfirmNameChange()
    {
        if (currentActor != null)
        {
            // PeopleActor에 있는 이름 변경 함수 호출
            currentActor.ChangeName(nameInputField.text);

            // UI 텍스트도 즉시 갱신
            nameText.text = $"이름: {currentActor.DisplayName}";
            OnNameTagStateChangeChannel?.RaiseEvent(currentActor.gameObject, true);

            currentActor.ChangeLoyalty(nameBestowalLoyaltyBonus);
            Debug.Log($"<color=cyan>{currentActor.DisplayName}: 이름을 하사받아 충성도가 {nameBestowalLoyaltyBonus}만큼 상승!</color>");
            EmotionController emotionCtrl = currentActor.GetComponent<EmotionController>();
            if (emotionCtrl != null)
            {
                // 감정 관리인에게 "Emotion_Love"를 표현하라고 명합니다!
                emotionCtrl.ExpressEmotion("Emotion_Love");
            }
        }
        // 기본 보기 모드로 전환
        ExitEditMode();
    }

    // 직업 비주얼을 업데이트하는 함수
    private void UpdateVisuals(JobType job)
    {
        JobVisual visualToShow = null;
        foreach (var visual in jobVisuals)
        {
            if (visual.job == job)
            {
                visualToShow = visual;
                break;
            }
        }
        
        if (visualToShow == null)
        {
            portraitOrVideoImage.enabled = false;
            videoPlayer.enabled = false;
            return;
        }

        if (visualToShow.video != null)
        {
            portraitOrVideoImage.texture = videoRenderTexture; 
            portraitOrVideoImage.enabled = true;
            videoPlayer.enabled = true;
            videoPlayer.clip = visualToShow.video;
            videoPlayer.isLooping = true;
            videoPlayer.Play();
        }
        else if (visualToShow.portrait != null)
        {
            videoPlayer.enabled = false;
            portraitOrVideoImage.enabled = true;
            portraitOrVideoImage.texture = visualToShow.portrait.texture;
        }
        else
        {
            portraitOrVideoImage.enabled = false;
            videoPlayer.enabled = false;
        }
    }

    // '선택 해제됨' 방송을 받으면 호출
    private void HideUI()
    {
        // 만약 이름 변경 중이었다면, 그것부터 취소
        if(editGroup.activeSelf)
        {
            ExitEditMode();
        }
        infoPanel.SetActive(false);
        currentActor = null; // 저장된 actor 정보 초기화
    }
}