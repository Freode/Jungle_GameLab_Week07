using UnityEngine;

public class PeopleSpawner : MonoBehaviour
{
    [Header("Default Profile (optional)")]
    public PeopleProfile defaultProfile;

    [Header("Spawn Settings")]
    public Transform defaultParent;

    // 1) 프로필 기반 스폰
    public PeopleActor SpawnFromProfile(Vector3 pos, Quaternion rot, PeopleProfile profile = null, Transform parent = null)
    {
        var actorGO = ObjectPooler.Instance.SpawnObject(ObjectType.People, pos, rot, parent ? parent : defaultParent);
        if (actorGO == null) return null;

        var actor = actorGO.GetComponent<PeopleActor>();
        if (actor == null)
        {
            actor = actorGO.AddComponent<PeopleActor>(); // 안전망
        }

        var pf = profile ? profile : defaultProfile;
        if (pf == null)
        {
            // 프로필이 없으면 최소 기본값이라도
            actor.Apply(new PeopleValue { age = 20, loyalty = 50, name = "NPC" });
        }
        else
        {
            actor.Apply(pf.Generate());
        }

        return actor;
    }

    // 2) 직접 값 지정 스폰(오버라이드)
    public PeopleActor SpawnWith(Vector3 pos, Quaternion rot, int age, int loyalty, string name, Transform parent = null)
    {
        var actorGO = ObjectPooler.Instance.SpawnObject(ObjectType.People, pos, rot, parent ? parent : defaultParent);
        if (actorGO == null) return null;

        var actor = actorGO.GetComponent<PeopleActor>() ?? actorGO.AddComponent<PeopleActor>();
        actor.Apply(new PeopleValue { age = age, loyalty = loyalty, name = name, job = JobType.None });
        return actor;
    }
}
