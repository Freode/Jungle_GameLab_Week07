using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TechUpgradeUI : MonoBehaviour
{
    [SerializeField] private Button levelUp10Button;    // 10렙 업 버튼
    [SerializeField] private Button levelUp50Button;    // 50렙 업 버튼
    [SerializeField] private TextMeshProUGUI costText;  // 비용 표시 텍스트
    [SerializeField] private TextMeshProUGUI levelText; // 레벨 표시 텍스트
    
    private TechState currentTechState;  // 현재 선택된 테크의 상태

    public void Initialize(TechState techState)
    {
        currentTechState = techState;
        UpdateUI();
        
        // 버튼에 리스너 추가
        if (levelUp10Button != null)
            levelUp10Button.onClick.AddListener(() => OnMultiLevelUpButtonClick(10));
        
        if (levelUp50Button != null)
            levelUp50Button.onClick.AddListener(() => OnMultiLevelUpButtonClick(50));
    }

    private void OnMultiLevelUpButtonClick(int levels)
    {
        if (currentTechState == null) return;

        long currentGold = GameManager.instance.GetCurrentGoldAmount();
        var (actualLevels, totalCost) = currentTechState.TryMultiLevelUp(levels, currentGold);
        
        if (actualLevels > 0)
        {
            // 골드 차감
            GameManager.instance.AddCurrentGoldAmount(-totalCost);
            
            // UI 업데이트
            UpdateUI();
            
            // 효과 적용
            foreach (var effect in currentTechState.techData.effects)
            {
                for(int i = 0; i < actualLevels; i++)
                {
                    effect.ApplyTechEffect();
                }
            }
        }
    }

    private void UpdateUI()
    {
        if (currentTechState == null) return;

        // 레벨 텍스트 업데이트
        if (levelText != null)
            levelText.text = $"Lv.{currentTechState.currentLevel}";

        // 비용 텍스트 업데이트
        if (costText != null)
            costText.text = $"Cost: {currentTechState.requaireAmount}";

        // 버튼 상태 업데이트
        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        long currentGold = GameManager.instance.GetCurrentGoldAmount();
        bool isMaxLevel = currentTechState.isMaxLevel();

        if (levelUp10Button != null)
            levelUp10Button.interactable = !isMaxLevel && currentGold >= currentTechState.requaireAmount;

        if (levelUp50Button != null)
            levelUp50Button.interactable = !isMaxLevel && currentGold >= currentTechState.requaireAmount;
    }
}