using UnityEngine;

[CreateAssetMenu(fileName = "BaseTechEffect", menuName = "Scriptable Objects/BaseTechEffect")]
public abstract class BaseTechEffect : ScriptableObject
{
    // 테크 효과 실행 함수
    public abstract void ApplyTechEffect();

}
