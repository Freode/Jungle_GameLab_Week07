using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class ImageSpriteData
{
    [Header("Image Reference")]
    public Image targetImage;
    
    [Header("Sprite Settings")]
    public Sprite originalSprite;
    public Sprite changedSprite;
    
    [Header("Critical Sprite Settings")]
    public Sprite criticalSprite;
    
    [Header("Individual Settings")]
    public float changeDuration = 0.5f;
    public bool useGlobalDuration = true;
    
    [Header("Critical Duration Settings")]
    public float criticalChangeDuration = 1.0f;
    public bool useGlobalCriticalDuration = true;

    [HideInInspector]
    public bool isChanging = false;
    
    // 캐시된 Transform (최적화)
    [System.NonSerialized]
    public Transform cachedTransform;
    
    // 초기화 메서드
    public void Initialize(float globalDuration, float globalCriticalDuration)
    {
        if (targetImage != null)
        {
            cachedTransform = targetImage.transform;
            if (originalSprite == null)
                originalSprite = targetImage.sprite;
            if (useGlobalDuration)
                changeDuration = globalDuration;
            if (useGlobalCriticalDuration)
                criticalChangeDuration = globalCriticalDuration;
        }
    }
    
    // 유효성 검사
    public bool IsValid => targetImage != null && changedSprite != null;
}

public class ButtonImageChanger : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private Button targetButton;
    
    [Header("Multiple Image Settings")]
    [SerializeField] private List<ImageSpriteData> imageDataList = new List<ImageSpriteData>();
    
    [Header("Global Settings")]
    [SerializeField] private float globalChangeDuration = 0.5f;
    [SerializeField] private bool changeAllSimultaneously = true;
    
    [Header("Global Critical Settings")]
    [SerializeField] private float globalCriticalChangeDuration = 1.0f;
    
    [Header("Gold Area People Count Settings")]
    [SerializeField] private bool useGoldAreaCount = true;
    [Tooltip("Gold 영역 사람 수에 따라 이미지를 순차적으로 활성화합니다")]
    
    [Header("Critical Settings")]
    [SerializeField] private bool enableCriticalImages = true;
    [Tooltip("크리티컬 발생 시 크리티컬 이미지로 변경")]
    [SerializeField] private ClickThrottle clickThrottle;
    [Tooltip("크리티컬 감지를 위한 ClickThrottle 참조")]

        [Header("Effect UI Settings")]
        [SerializeField] private GameObject effectUI;
        [SerializeField] private float effectUIDuration = 1.0f;
    
    [Header("Event Broadcasting")]
    public VoidEventChannelSO onGoldButtonClickChannel;
    // 최적화를 위한 캐시
    private readonly List<Coroutine> activeCoroutines = new List<Coroutine>();
    private readonly List<ImageSpriteData> validImageData = new List<ImageSpriteData>();
    private readonly List<ImageSpriteData> activeImageData = new List<ImageSpriteData>();
    
    // 이벤트 구독 상태 추적
    private bool isSubscribedToGoldAreaEvents = false;
    
    // 마지막으로 확인한 Gold 영역 사람 수 (불필요한 업데이트 방지)
    private int lastGoldAreaCount = -1;
    
    // 크리티컬 감지를 위한 변수들
    private bool isCriticalMode = false;
    private Coroutine criticalResetCoroutine;
    
    private void Start()
    {
        InitializeComponents();
        InitializeImageData();
        SetupEventListeners();
    }
    
    private void InitializeComponents()
    {
        // 컴포넌트 자동 할당
        if (targetButton == null)
            targetButton = GetComponent<Button>();
            
        // ClickThrottle 자동 할당 (같은 게임오브젝트에서 찾기)
        if (clickThrottle == null && enableCriticalImages)
            clickThrottle = GetComponent<ClickThrottle>();
    }
    
    private void InitializeImageData()
    {
        // 이미지 데이터 초기화 및 유효한 데이터만 캐시
        validImageData.Clear();
        
        for (int i = 0; i < imageDataList.Count; i++)
        {
            var data = imageDataList[i];
            data.Initialize(globalChangeDuration, globalCriticalChangeDuration);
            
            if (data.IsValid)
            {
                validImageData.Add(data);
            }
        }
    }
    
    private void SetupEventListeners()
    {
        // 버튼 이벤트 연결
        if (targetButton != null)
            targetButton.onClick.AddListener(OnButtonClick);
            
        // Gold 영역 사람 수 추적 시작
        if (useGoldAreaCount)
        {
            SubscribeToGoldAreaEvents();
        }
        
        // 크리티컬 이벤트 구독
        if (enableCriticalImages)
        {
            ClickThrottle.OnCriticalHit += OnCriticalHit;
        }
    }
    
    private void SubscribeToGoldAreaEvents()
    {
        if (isSubscribedToGoldAreaEvents || PeopleManager.Instance == null) return;
        
        PeopleManager.Instance.OnAreaPeopleCountChanged += UpdateImagesByGoldAreaCount;
        isSubscribedToGoldAreaEvents = true;
        UpdateImagesByGoldAreaCount(); // 초기 상태 설정
    }
    
    private void UnsubscribeFromGoldAreaEvents()
    {
        if (!isSubscribedToGoldAreaEvents || PeopleManager.Instance == null) return;
        
        PeopleManager.Instance.OnAreaPeopleCountChanged -= UpdateImagesByGoldAreaCount;
        isSubscribedToGoldAreaEvents = false;
    }
    
    private void OnDestroy()
    {
        // 실행 중인 코루틴 정리
        StopAllActiveCoroutines();
        
        // 이벤트 해제
        if (targetButton != null)
            targetButton.onClick.RemoveListener(OnButtonClick);
            
        UnsubscribeFromGoldAreaEvents();
        
        // 크리티컬 이벤트 해제
        ClickThrottle.OnCriticalHit -= OnCriticalHit;
    }
    
    // 크리티컬 이벤트 핸들러
    private void OnCriticalHit()
    {
        if (enableCriticalImages)
        {
            TriggerCriticalMode();
        }
    }
    
    private void StopAllActiveCoroutines()
    {
        for (int i = 0; i < activeCoroutines.Count; i++)
        {
            if (activeCoroutines[i] != null)
                StopCoroutine(activeCoroutines[i]);
        }
        activeCoroutines.Clear();
        
        // 모든 이미지의 변경 상태 초기화
        for (int i = 0; i < validImageData.Count; i++)
        {
            validImageData[i].isChanging = false;
        }
    }
    
    public void OnButtonClick()
    {
        if (onGoldButtonClickChannel != null)
        {
            onGoldButtonClickChannel.RaiseEvent();
        }
        // 이미 실행 중인 애니메이션이 있다면 중단
        if (activeCoroutines.Count > 0)
        {
            StopAllActiveCoroutines();
        }

        // 광부가 있을 때 Effect UI 활성화
        if (effectUI != null && PeopleManager.Instance != null && PeopleManager.Instance.Count(AreaType.Gold) > 0)
        {
            StartCoroutine(ShowEffectUICoroutine());
        }

        if (useGoldAreaCount)
        {
            // Gold 영역 모드: 활성화된 이미지만 변경
            UpdateActiveImageDataCache();
            if (changeAllSimultaneously)
            {
                var coroutine = StartCoroutine(ChangeImagesSimultaneously(activeImageData));
                activeCoroutines.Add(coroutine);
            }
            else
            {
                var coroutine = StartCoroutine(ChangeImagesSequentially(activeImageData));
                activeCoroutines.Add(coroutine);
            }
        }
        else
        {
            // 기본 모드: 유효한 모든 이미지 변경
            if (changeAllSimultaneously)
            {
                var coroutine = StartCoroutine(ChangeImagesSimultaneously(validImageData));
                activeCoroutines.Add(coroutine);
            }
            else
            {
                var coroutine = StartCoroutine(ChangeImagesSequentially(validImageData));
                activeCoroutines.Add(coroutine);
            }
        }

    }

    // Effect UI를 일정 시간 활성화하는 코루틴
    private IEnumerator ShowEffectUICoroutine()
    {
        effectUI.SetActive(true);
        yield return new WaitForSeconds(effectUIDuration);
        effectUI.SetActive(false);
    }
    
    // 크리티컬 모드 활성화
    public void TriggerCriticalMode()
    {
        if (!enableCriticalImages) return;
        
        isCriticalMode = true;
        
        // 기존 크리티컬 리셋 코루틴이 있다면 중단
        if (criticalResetCoroutine != null)
        {
            StopCoroutine(criticalResetCoroutine);
        }
        
        // 크리티컬 모드 자동 해제 (일정 시간 후)
        criticalResetCoroutine = StartCoroutine(ResetCriticalModeAfterDelay());
    }
    
    // 크리티컬 모드 해제
    private IEnumerator ResetCriticalModeAfterDelay()
    {
        yield return new WaitForSeconds(globalCriticalChangeDuration + 0.5f);
        isCriticalMode = false;
        criticalResetCoroutine = null;
    }
    
    private void UpdateActiveImageDataCache()
    {
        activeImageData.Clear();
        for (int i = 0; i < validImageData.Count; i++)
        {
            var data = validImageData[i];
            if (data.targetImage != null && data.targetImage.gameObject.activeInHierarchy)
            {
                activeImageData.Add(data);
            }
        }
    }
    
    private IEnumerator ChangeImagesSimultaneously(List<ImageSpriteData> targetImages)
    {
        var runningCoroutines = new List<Coroutine>();
        
        // 모든 유효한 이미지를 동시에 변경 시작
        for (int i = 0; i < targetImages.Count; i++)
        {
            var data = targetImages[i];
            if (!data.isChanging)
            {
                var coroutine = StartCoroutine(ChangeImageTemporarily(data));
                runningCoroutines.Add(coroutine);
                activeCoroutines.Add(coroutine);
            }
        }
        
        // 모든 코루틴이 완료될 때까지 대기
        for (int i = 0; i < runningCoroutines.Count; i++)
        {
            yield return runningCoroutines[i];
            activeCoroutines.Remove(runningCoroutines[i]);
        }
    }
    
    private IEnumerator ChangeImagesSequentially(List<ImageSpriteData> targetImages)
    {
        // 순차적으로 변경
        for (int i = 0; i < targetImages.Count; i++)
        {
            var data = targetImages[i];
            if (!data.isChanging)
            {
                var coroutine = StartCoroutine(ChangeImageTemporarily(data));
                activeCoroutines.Add(coroutine);
                yield return coroutine;
                activeCoroutines.Remove(coroutine);
            }
        }
    }
    
    private IEnumerator ChangeImageTemporarily(ImageSpriteData data)
    {
        if (!data.IsValid || data.isChanging)
            yield break;
        
        data.isChanging = true;
        
        // 원본 스프라이트 백업 (필요한 경우)
        if (data.originalSprite == null)
            data.originalSprite = data.targetImage.sprite;
        
        // 사용할 스프라이트 결정 (크리티컬 모드인지 확인)
        Sprite spriteToUse;
        if (isCriticalMode && data.criticalSprite != null)
        {
            spriteToUse = data.criticalSprite;
        }
        else
        {
            spriteToUse = data.changedSprite;
        }
        
        // 스프라이트 변경
        data.targetImage.sprite = spriteToUse;
        
        // 지정된 시간만큼 대기 (크리티컬 모드인지에 따라 다른 지속 시간 사용)
        float duration;
        if (isCriticalMode && data.criticalSprite != null)
        {
            duration = data.useGlobalCriticalDuration ? globalCriticalChangeDuration : data.criticalChangeDuration;
        }
        else
        {
            duration = data.useGlobalDuration ? globalChangeDuration : data.changeDuration;
        }
        yield return new WaitForSeconds(duration);
        
        // 원본 스프라이트로 복원
        if (data.targetImage != null && data.originalSprite != null)
        {
            data.targetImage.sprite = data.originalSprite;
        }
        
        data.isChanging = false;
    }
    
    // Gold 영역 사람 수에 따른 이미지 활성화 업데이트
    private void UpdateImagesByGoldAreaCount()
    {
        if (!useGoldAreaCount || PeopleManager.Instance == null) return;
        
        int goldAreaPeopleCount = PeopleManager.Instance.Count(AreaType.Gold);
        
        // 값이 변경되지 않았다면 업데이트 생략 (최적화)
        if (goldAreaPeopleCount == lastGoldAreaCount) return;
        lastGoldAreaCount = goldAreaPeopleCount;
        
        // 캐시된 유효한 이미지 데이터 사용
        int maxCount = Mathf.Min(goldAreaPeopleCount, validImageData.Count);
        
        // 모든 이미지를 먼저 비활성화
        for (int i = 0; i < validImageData.Count; i++)
        {
            validImageData[i].targetImage.gameObject.SetActive(false);
        }
        
        // 사람 수만큼 이미지를 순차적으로 활성화
        for (int i = 0; i < maxCount; i++)
        {
            validImageData[i].targetImage.gameObject.SetActive(true);
        }
    }
    
    // 외부에서 호출 가능한 메서드들 (최적화됨)
    public void SetGlobalChangeDuration(float duration)
    {
        globalChangeDuration = duration;
        
        // 글로벌 duration을 사용하는 데이터들 업데이트
        for (int i = 0; i < validImageData.Count; i++)
        {
            if (validImageData[i].useGlobalDuration)
            {
                validImageData[i].changeDuration = globalChangeDuration;
            }
        }
    }
    
    public void SetGlobalCriticalChangeDuration(float duration)
    {
        globalCriticalChangeDuration = duration;
        
        // 글로벌 크리티컬 duration을 사용하는 데이터들 업데이트
        for (int i = 0; i < validImageData.Count; i++)
        {
            if (validImageData[i].useGlobalCriticalDuration)
            {
                validImageData[i].criticalChangeDuration = globalCriticalChangeDuration;
            }
        }
    }
    
    public void AddImageData(Image image, Sprite changedSprite, float duration = -1f)
    {
        var newData = new ImageSpriteData
        {
            targetImage = image,
            originalSprite = image?.sprite,
            changedSprite = changedSprite,
            changeDuration = duration > 0 ? duration : globalChangeDuration,
            useGlobalDuration = duration <= 0
        };
        
        imageDataList.Add(newData);
        newData.Initialize(globalChangeDuration, globalCriticalChangeDuration);
        
        if (newData.IsValid)
        {
            validImageData.Add(newData);
        }
    }
    
    public void AddImageData(Image image, Sprite changedSprite, Sprite criticalSprite, float duration = -1f)
    {
        var newData = new ImageSpriteData
        {
            targetImage = image,
            originalSprite = image?.sprite,
            changedSprite = changedSprite,
            criticalSprite = criticalSprite,
            changeDuration = duration > 0 ? duration : globalChangeDuration,
            useGlobalDuration = duration <= 0
        };
        
        imageDataList.Add(newData);
        newData.Initialize(globalChangeDuration, globalCriticalChangeDuration);
        
        if (newData.IsValid)
        {
            validImageData.Add(newData);
        }
    }
    
    public void AddImageData(Image image, Sprite changedSprite, Sprite criticalSprite, float duration = -1f, float criticalDuration = -1f)
    {
        var newData = new ImageSpriteData
        {
            targetImage = image,
            originalSprite = image?.sprite,
            changedSprite = changedSprite,
            criticalSprite = criticalSprite,
            changeDuration = duration > 0 ? duration : globalChangeDuration,
            criticalChangeDuration = criticalDuration > 0 ? criticalDuration : globalCriticalChangeDuration,
            useGlobalDuration = duration <= 0,
            useGlobalCriticalDuration = criticalDuration <= 0
        };
        
        imageDataList.Add(newData);
        newData.Initialize(globalChangeDuration, globalCriticalChangeDuration);
        
        if (newData.IsValid)
        {
            validImageData.Add(newData);
        }
    }
    
    public void RemoveImageData(Image image)
    {
        for (int i = imageDataList.Count - 1; i >= 0; i--)
        {
            if (imageDataList[i].targetImage == image)
            {
                validImageData.Remove(imageDataList[i]);
                imageDataList.RemoveAt(i);
                break;
            }
        }
    }
    
    public void ClearAllImageData()
    {
        StopAllActiveCoroutines();
        imageDataList.Clear();
        validImageData.Clear();
        activeImageData.Clear();
    }
    
    public void SetChangeMode(bool simultaneous)
    {
        changeAllSimultaneously = simultaneous;
    }
    
    public void ChangeSpecificImage(int index)
    {
        if (index >= 0 && index < validImageData.Count && !validImageData[index].isChanging)
        {
            var coroutine = StartCoroutine(ChangeImageTemporarily(validImageData[index]));
            activeCoroutines.Add(coroutine);
        }
    }
    
    public void ChangeSpecificImage(Image targetImage)
    {
        for (int i = 0; i < validImageData.Count; i++)
        {
            if (validImageData[i].targetImage == targetImage && !validImageData[i].isChanging)
            {
                var coroutine = StartCoroutine(ChangeImageTemporarily(validImageData[i]));
                activeCoroutines.Add(coroutine);
                break;
            }
        }
    }
    
    public void SetTargetButton(Button button)
    {
        // 기존 버튼 이벤트 해제
        if (targetButton != null)
            targetButton.onClick.RemoveListener(OnButtonClick);
        
        // 새 버튼 설정 및 이벤트 연결
        targetButton = button;
        if (targetButton != null)
            targetButton.onClick.AddListener(OnButtonClick);
    }
    
    public void SetUseGoldAreaCount(bool enabled)
    {
        if (useGoldAreaCount == enabled) return; // 이미 같은 상태면 무시
        
        useGoldAreaCount = enabled;
        
        if (enabled)
        {
            SubscribeToGoldAreaEvents();
        }
        else
        {
            UnsubscribeFromGoldAreaEvents();
            // Gold 영역 모드 해제 시 모든 유효한 이미지 활성화
            for (int i = 0; i < validImageData.Count; i++)
            {
                validImageData[i].targetImage.gameObject.SetActive(true);
            }
            lastGoldAreaCount = -1; // 캐시 초기화
        }
    }
    
    // 크리티컬 관련 외부 제어 메서드들
    public void SetEnableCriticalImages(bool enabled)
    {
        if (enableCriticalImages == enabled) return; // 이미 같은 상태면 무시
        
        enableCriticalImages = enabled;
        
        if (enabled)
        {
            ClickThrottle.OnCriticalHit += OnCriticalHit;
        }
        else
        {
            ClickThrottle.OnCriticalHit -= OnCriticalHit;
            isCriticalMode = false;
            if (criticalResetCoroutine != null)
            {
                StopCoroutine(criticalResetCoroutine);
                criticalResetCoroutine = null;
            }
        }
    }
    
    public void SetClickThrottle(ClickThrottle throttle)
    {
        clickThrottle = throttle;
    }
    
    public void ForceCriticalMode(bool enabled)
    {
        if (!enableCriticalImages) return;
        
        isCriticalMode = enabled;
        
        if (enabled && criticalResetCoroutine != null)
        {
            StopCoroutine(criticalResetCoroutine);
            criticalResetCoroutine = null;
        }
    }
    
    public void AddCriticalSpriteToImageData(int index, Sprite criticalSprite)
    {
        if (index >= 0 && index < imageDataList.Count)
        {
            imageDataList[index].criticalSprite = criticalSprite;
        }
    }
    
    // 디버그/정보 제공용 메서드들
    public int GetActiveCoroutineCount() => activeCoroutines.Count;
    public int GetValidImageCount() => validImageData.Count;
    public bool IsGoldAreaModeEnabled() => useGoldAreaCount;
    public bool IsCriticalModeEnabled() => enableCriticalImages;
    public bool IsCriticalMode() => isCriticalMode;
    public bool IsAnyImageChanging()
    {
        for (int i = 0; i < validImageData.Count; i++)
        {
            if (validImageData[i].isChanging) return true;
        }
        return false;
    }
}
