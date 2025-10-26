using UnityEngine;

[CreateAssetMenu(fileName = "PeopleTechEffect", menuName = "Scriptable Objects/Tech Effect/People Tech Effect")]
public class PeopleTechEffect : BaseTechEffect
{
    [SerializeField] private AreaType targetArea;

    public override void ApplyTechEffect()
    {
        GameObject obj = PeopleManager.Instance.SelectOnePerson(AreaType.Normal);
        
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
            default:
                Debug.LogWarning("알 수 없는 영역 타입입니다: " + targetArea);
                break;
        }
    }
}
