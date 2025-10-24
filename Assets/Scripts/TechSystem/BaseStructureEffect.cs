using UnityEngine;

// 구조물 효과 실행
[CreateAssetMenu(fileName = "BaseStructureEffect", menuName = "Scriptable Objects/Base Structure Effect")]

public abstract class BaseStructureEffect : ScriptableObject
{
    public string content;

    public abstract string ApplyTechEffect();
}
