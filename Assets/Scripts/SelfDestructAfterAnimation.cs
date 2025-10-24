// 파일 이름: SelfDestructAfterAnimation.cs
using UnityEngine;

/// <summary>
/// 이 스크립트가 붙어있는 오브젝트의 Animator가 가진 애니메이션이
/// 재생 완료되면, 스스로를 파괴(소멸)시킵니다.
/// </summary>
[RequireComponent(typeof(Animator))]
public class SelfDestructAfterAnimation : MonoBehaviour
{
    void Start()
    {
        // 자신의 애니메이터에게서 현재 재생 중인 애니메이션의 길이를 알아냅니다.
        float animationLength = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;
        
        // 그 길이만큼의 시간이 지난 후에 자신을 파괴하라는 유언을 남깁니다.
        Destroy(gameObject, animationLength);
    }
}