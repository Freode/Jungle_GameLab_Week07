using UnityEngine;

[CreateAssetMenu(fileName = "PeopleTechEffect", menuName = "Scriptable Objects/Tech Effect/People Tech Effect")]
public class PeopleTechEffect : BaseTechEffect
{
    [SerializeField] private AreaType targetArea;

    public override void ApplyTechEffect()
    {
        // 변경: Gold를 구매하는 경우를 제외하고는 Gold 영역의 Worker를 사용
        // Gold를 구매하는 경우는 무직(Normal)을 사용 (주석 처리됨)
        GameObject obj = null;
        
        if (targetArea == AreaType.Gold)
        {
            // Gold 직업은 구매 불가능하게 함 (또는 다른 로직 사용)
            Debug.LogWarning("Gold 영역 직업은 직접 구매할 수 없습니다.");
            return;
        }
        else
        {
            // 다른 직업들은 Gold Worker를 1명 사용
            obj = PeopleManager.Instance.SelectOnePerson(AreaType.Gold);
        }
        
        if (obj == null) return;
        
        switch (targetArea)
        {
            case AreaType.Normal:
                Debug.Log("Normal 영역의 사람을 선택했습니다: " + obj.name);
                break;
            case AreaType.Mine:
                PeopleManager.Instance.MoveToArea(obj, AreaType.Mine, JobType.Miner);
                break;
            case AreaType.Carrier:
                PeopleManager.Instance.CheckUnlockArea();
                PeopleManager.Instance.MoveToArea(obj, AreaType.Carrier, JobType.Carrier);
                break;
            case AreaType.Architect:
                PeopleManager.Instance.MoveToArea(obj, AreaType.Architect, JobType.Architect);
                break;
            case AreaType.StoneCarving:
                PeopleManager.Instance.MoveToArea(obj, AreaType.StoneCarving, JobType.Carver);
                break;
            case AreaType.Gold:
                PeopleManager.Instance.MoveToArea(obj, AreaType.Gold, JobType.Worker);
                break;
            case AreaType.Prison:
                Debug.Log("Prison 영역의 사람을 선택했습니다: " + obj.name);
                break;
            case AreaType.Barrack:
                PeopleManager.Instance.MoveToArea(obj, AreaType.Barrack, JobType.Guard);
                break;
            case AreaType.Brewery:
                PeopleManager.Instance.MoveToArea(obj, AreaType.Brewery, JobType.Brewer);
                break;
            case AreaType.Temple:
                PeopleManager.Instance.MoveToArea(obj, AreaType.Temple, JobType.Priest);
                break;
            case AreaType.Special:
                PeopleManager.Instance.MoveToArea(obj, AreaType.Special, JobType.God);
                break;
            default:
                Debug.LogWarning("알 수 없는 영역 타입입니다: " + targetArea);
                break;
        }
    }
}
