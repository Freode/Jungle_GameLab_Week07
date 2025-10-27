
using UnityEngine;
using System.Collections.Generic;

public static class CatGodManager
{
    private static CatGodData catGodData;
    private static HashSet<CatGodType> spawnedCatGods = new HashSet<CatGodType>();
    public static void TrySpawnCatGod(CatGodType catGodType)
    {
        if (spawnedCatGods.Contains(catGodType))
        {
            return;
        }

        if (catGodData == null)
        {
            catGodData = Resources.Load<CatGodData>("CatGodData");
            if (catGodData == null)
            {
                Debug.LogError("CatGodData를 Resources 폴더에서 찾을 수 없습니다.");
                return;
            }
        }

        ObjectType objectType = catGodData.GetObjectType(catGodType);
        if (objectType == ObjectType.None)
        {
            Debug.LogWarning($"{catGodType}에 해당하는 ObjectType이 CatGodData에 없습니다.");
            return;
        }

        GameObject catGod = ObjectPooler.Instance.SpawnObject(objectType);

        if (catGod == null)
        {
            Debug.LogWarning("고양이 신 소환에 실패했습니다.");
            return;
        }

        // CatGod을 Normal 영역에 배회하도록 설정
        PeopleManager.Instance.MoveToArea(catGod, AreaType.Special, JobType.None);

        spawnedCatGods.Add(catGodType);

        Debug.Log($"{catGodType} 고양이 신이 소환되었습니다!");
    }
}
