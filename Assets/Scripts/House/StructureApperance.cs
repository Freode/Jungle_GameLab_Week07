using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


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
    public GameObject InfoUI;
    [SerializeField] StructureType structureType;

    public Queue<bool> levelUpQueue = new Queue<bool>();
    public bool IsLevelUpPending => levelUpQueue.Count > 0;
    public GameObject levelUpQueueUI;
    public ParticleSystem levelUpParticle;

    private SpriteRenderer spriteRenderer;
    private int currentLevel = 0;
    private int finalLevel = 0;
    private int appliedAppearanceLevel = -1;
    private int currentLevelIndex = 0;

    void Start()
    {
        // 에디터에서 값 변경 시 실시간으로 적용
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        finalLevel = levelAppearances[levelAppearances.Length - 1].level;
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

            // GameManager.instance.SetIsGameOver(true);
        }

        for (int i = levelAppearances.Length - 1; i >= 0; i--)
        {
            if (level < levelAppearances[i].level)
                continue;

            Debug.Log(this);
            Debug.Log(areaType + ":" + level + ", " + appliedAppearanceLevel + ", " + levelAppearances[i].level);
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
    }

    void CheckLevelUpQueue()
    {
        // ui 가 active false 상태이고, queue 의 count 가 0 이상일때
        if (!levelUpQueueUI.activeSelf && levelUpQueue.Count > 0)
        {
            levelUpQueueUI.SetActive(true);
        }
        else if (levelUpQueue.Count == 0)
        {
            levelUpQueueUI.SetActive(false);
        }
    }

    public void LevelUpStructure()
    {
        if (levelUpQueue.Count == 0) return;

        GameLogger.Instance.click.AddInteractClick();
        ApplyLevelUpEffect();
        spriteRenderer.sprite = levelAppearances[currentLevelIndex].sprite;
        transform.localScale = levelAppearances[currentLevelIndex].scale;
        levelUpQueue.Dequeue();
        levelUpQueueUI.SetActive(false);
        currentLevelIndex++;
        GameManager.instance.AddCurrentGoldAmount(0); // To trigger UI refresh
        
        // play particle at transform position
        levelUpParticle.transform.position = transform.position;
        levelUpParticle.Play();
    }

    // 마우스 올려 놓기
    private void OnMouseEnter()
    {
        InfoUI.TryGetComponent(out TechInfo techInfo);
        if (techInfo == null) return;

        techInfo.OnActiveInfo(areaType, currentLevel, finalLevel, areaIcon, new Vector3(1920f, 0f, 0f));
    }

    // 마우스가 빠져 나감
    private void OnMouseExit()
    {
        InfoUI.TryGetComponent(out TechInfo techInfo);
        if (techInfo == null) return;

        techInfo.OnInactiveInfo();
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
}