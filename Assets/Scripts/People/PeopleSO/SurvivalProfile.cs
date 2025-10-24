using UnityEngine;

[CreateAssetMenu(menuName = "People/Survival Profile")]
public class SurvivalProfile : ScriptableObject
{
    [Tooltip("X축: 나이, Y축: 생존 확률 (0=0%, 1=100%)")]
    public AnimationCurve survivalChanceByAge;

    // 특정 나이의 생존 확률을 반환하는 함수
    public float GetSurvivalChance(int age)
    {
        // 커브에서 확률을 평가하여 반환 (0과 1 사이 값)
        return Mathf.Clamp01(survivalChanceByAge.Evaluate(age));
    }
}