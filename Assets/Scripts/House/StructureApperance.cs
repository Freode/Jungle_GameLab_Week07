using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;


public class StructureApperance : MonoBehaviour
{
    private enum StructureType
    {
        House,
        Transport,
        Forge,
        Pyramid
    }

    public AreaType areaType;
    public Sprite areaIcon;                     // 건물 아이콘
    public LevelAppearance[] levelAppearances;
    public bool isClearStructure = false;
    public GameObject gameOverButton; // 게임 오버 버튼
    public GameObject InfoUI;
    [SerializeField] StructureType structureType;

    public Queue<bool> levelUpQueue = new Queue<bool>();
    public bool IsLevelUpPending => levelUpQueue.Count > 0;
    public GameObject levelUpQueueUI;
    public ParticleSystem levelUpParticle;

    [Header("건물 이름 표시")]
    public TextMeshProUGUI structureNameText;   // 건물 이름을 표시할 TMP 텍스트
    public float nameYOffset = 1.5f;            // 건물 위 이름 표시 Y 오프셋

    private SpriteRenderer spriteRenderer;
    private int currentLevel = 0;
    private int finalLevel = 0;
    private int appliedAppearanceLevel = -1;
    private int currentLevelIndex = 0;
    private bool isFinalLevelUp = false;
    
    public static event System.Action<RectTransform> OnFirstLevelUpButtonShown;
    public static event System.Action OnLevelUpButtonPressed;
    private bool hasShownLevelUpButtonOnce = false;


    void Start()
    {
        // 에디터에서 값 변경 시 실시간으로 적용
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        finalLevel = levelAppearances[levelAppearances.Length - 1].level;

        // 초기 레벨 0의 스프라이트 적용
        if (levelAppearances.Length > 0 && levelAppearances[0].level == 0)
        {
            spriteRenderer.sprite = levelAppearances[0].sprite;
            transform.localScale = levelAppearances[0].scale;
            appliedAppearanceLevel = 0;
            // currentLevelIndex를 1로 설정 (레벨 0은 이미 적용했으므로 다음은 인덱스 1)
            currentLevelIndex = 1;
        }

        // 건물 이름 텍스트 초기화
        UpdateStructureNameDisplay();
    }

    void Update()
    {
        CheckLevelUpQueue();
    }

    // 레벨에 따른 외형 변경
    public void UpdateApperanceByLevel(int level)
    {
        currentLevel = level;
        // 클리어 구조체를 모두 완성한 경우
        if (isClearStructure && level >= finalLevel)
        {
            // --- Logger Code ---
            string context = $"Timestamp: {System.DateTime.Now}";
            GameLogger.Instance.Log("pyramid_completion", context);
            // --- End Logger Code ---

            isFinalLevelUp = true;

            // GameManager.instance.SetIsGameOver(true);
        }

        for (int i = levelAppearances.Length - 1; i >= 0; i--)
        {
            if (level < levelAppearances[i].level)
                continue;

            // Check if we are applying a sprite from a new, higher level tier
            if (appliedAppearanceLevel < levelAppearances[i].level)
            {
                if (appliedAppearanceLevel == -1)
                {
                    if (structureType == StructureType.House || structureType == StructureType.Pyramid)
                    {
                        appliedAppearanceLevel = levelAppearances[i].level;
                        currentLevelIndex++;
                        continue;
                    }
                }

                levelUpQueue.Enqueue(true);

                appliedAppearanceLevel = levelAppearances[i].level;
            }


            break;
        }

        // 레벨 변경 시 이름 표시 업데이트
        UpdateStructureNameDisplay();
    }

    void CheckLevelUpQueue()
    {
        if (!levelUpQueueUI.activeSelf && levelUpQueue.Count > 0)
        {
            levelUpQueueUI.SetActive(true);

            // ★ 최초로 버튼이 등장한 시점에 한 번만 이벤트 발생
            if (!hasShownLevelUpButtonOnce)
            {
                hasShownLevelUpButtonOnce = true;
                // 버튼(또는 아이콘) 월드 좌표 전달
                var btnRect = levelUpQueueUI.GetComponent<RectTransform>();
                OnFirstLevelUpButtonShown?.Invoke(btnRect);
            }
        }
        else if (levelUpQueue.Count == 0)
        {
            levelUpQueueUI.SetActive(false);
        }

    }

    public void LevelUpStructure()
    {
        if (levelUpQueue.Count == 0) return;
        
        // 업그레이드 버튼이 실제로 눌려 소비되는 시점에 알림
        OnLevelUpButtonPressed?.Invoke();

        GameLogger.Instance.click.AddInteractClick();
        ApplyLevelUpEffect();
        spriteRenderer.sprite = levelAppearances[currentLevelIndex].sprite;
        transform.localScale = levelAppearances[currentLevelIndex].scale;
        levelUpQueue.Dequeue();
        levelUpQueueUI.SetActive(false);
        currentLevelIndex++;
        GameManager.instance.AddCurrentGoldAmount(0); // To trigger UI refresh

        if (isFinalLevelUp)
        {
            if (gameOverButton != null)
            {
                gameOverButton.SetActive(true);
            }
            isFinalLevelUp = false; // Reset flag
        }

        // play particle at transform position
        levelUpParticle.transform.position = transform.position;
        levelUpParticle.Play();

        // 건물 이름 업데이트
        UpdateStructureNameDisplay();
    }

    // 마우스 올려 놓기
    // private void OnMouseEnter()
    // {
    //     InfoUI.TryGetComponent(out TechInfo techInfo);
    //     if (techInfo == null) return;

    //     techInfo.OnActiveInfo(areaType, currentLevel, finalLevel, areaIcon, new Vector3(1920f, 0f, 0f));
    // }

    // // 마우스가 빠져 나감
    // private void OnMouseExit()
    // {
    //     InfoUI.TryGetComponent(out TechInfo techInfo);
    //     if (techInfo == null) return;

    //     techInfo.OnInactiveInfo();
    // }

    // 다음 진화 레벨을 반환하는 메서드
    public int GetNextEvolutionLevel()
    {
        // 현재 레벨보다 높은 다음 진화 단계를 찾음
        for (int i = 0; i < levelAppearances.Length; i++)
        {
            if (levelAppearances[i].level > currentLevel)
            {
                return levelAppearances[i].level;
            }
        }
        // 다음 진화 단계가 없으면 최종 레벨 반환
        return finalLevel;
    }

    // 레벨 업 시, 적용되는 효과 발동
    private void ApplyLevelUpEffect()
    {
        if (levelAppearances[currentLevelIndex].effects.Count == 0)
            return;

        foreach (var effect in levelAppearances[currentLevelIndex].effects)
        {
            string content = effect.ApplyTechEffect();
        }

    }

    // 건물 이름 표시 업데이트
    private void UpdateStructureNameDisplay()
    {
        if (structureNameText == null)
            return;

        // 레벨이 1 이상일 때만 이름 표시
        if (currentLevel >= 1)
        {
            string structureName = FuncSystem.GetStructureName(areaType, currentLevel);
            
            // "???" 이면 표시하지 않음
            if (structureName != "???")
            {
                structureNameText.text = structureName;
                structureNameText.gameObject.SetActive(true);
            }
            else
            {
                structureNameText.gameObject.SetActive(false);
            }
        }
        else
        {
            structureNameText.gameObject.SetActive(false);
        }
    }
}