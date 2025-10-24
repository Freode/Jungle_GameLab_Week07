using UnityEngine;

[ExecuteInEditMode]
public class HouseLevelController : MonoBehaviour
{
    public int currentLevel = 1;

    // ★★★ 이름 변경 및 scale 변수 추가 ★★★
    // 레벨별 외형 정보(스프라이트, 스케일)를 담을 배열입니다.
    public LevelAppearance[] levelAppearances;

    private SpriteRenderer spriteRenderer;

    void OnValidate()
    {
        // 에디터에서 값 변경 시 실시간으로 적용
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        UpdateAppearance();
    }

    void Start()
    {
        // 게임 시작 시 적용
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        UpdateAppearance();
    }

    public void SetLevel(int newLevel)
    {
        currentLevel = newLevel;
        UpdateAppearance();
    }

    // ★★★ 함수 이름 변경 및 스케일 적용 로직 추가 ★★★
    void UpdateAppearance()
    {
        if (spriteRenderer == null) return;
        if (levelAppearances == null || levelAppearances.Length == 0) return;

        LevelAppearance appearanceToSet = null;

        // 현재 레벨에 맞는 외형 정보를 찾습니다.
        foreach (var appearance in levelAppearances)
        {
            if (currentLevel >= appearance.level)
            {
                appearanceToSet = appearance;
            }
        }
        
        if (appearanceToSet != null)
        {
            // 찾은 정보로 스프라이트와 스케일을 모두 변경합니다.
            spriteRenderer.sprite = appearanceToSet.sprite;
            transform.localScale = appearanceToSet.scale; // 이 줄이 추가되었습니다!
        }
    }
}
