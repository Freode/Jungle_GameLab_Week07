using UnityEngine;
using System.Collections.Generic;

public class StructuresController : MonoBehaviour
{
    public static StructuresController Instance { get; private set; }
    public List<StructureData> structureInputs;

    private Dictionary<TechData, StructureApperance> strctureDatas;
    
    private void Awake()
    {
        Instance = this;
        strctureDatas = new Dictionary<TechData, StructureApperance>();
    }

    public StructureApperance GetStructureApperance(TechData techData)
    {
        strctureDatas.TryGetValue(techData, out var apperance);
        return apperance;
    }

    // 주어진 레벨이 진화 레벨인지 확인
    public bool IsEvolutionLevel(TechData techData, int level)
    {
        if (strctureDatas.TryGetValue(techData, out var apperance))
        {
            int nextEvolution = apperance.GetNextEvolutionLevel();
            return level >= nextEvolution;
        }
        return false;
    }

    void Start()
    {
        Init();
    }

    // 초기화
    private void Init()
    {
        foreach (var inputData in structureInputs)
        {
            inputData.areaStructure.TryGetComponent(out StructureApperance apperacne);
            strctureDatas.Add(inputData.techData, apperacne);
        }

        GameManager.instance.OnModifyStructureLevel += ModifyStructureLevel;
    }

    private void OnDestroy()
    {
        GameManager.instance.OnModifyStructureLevel -= ModifyStructureLevel;
    }

    // 레벨 업에 따른 구조물 외형 변화
    private void ModifyStructureLevel(TechData targetTechData, int amount)
    {
        if(strctureDatas.ContainsKey(targetTechData))
            strctureDatas[targetTechData].UpdateApperanceByLevel(amount);
    }
}
