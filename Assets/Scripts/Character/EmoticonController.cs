// 파일 이름: EmotionController.cs
using UnityEngine;
using System.Collections;

/// <summary>
/// 백성 자신의 감정 표현 오브젝트와 애니메이터를 관리합니다.
/// 이 스크립트는 외부가 아닌, 같은 오브젝트의 다른 스크립트(예: PeopleActor)에 의해 제어됩니다.
/// </summary>
public class EmotionController : MonoBehaviour
{
    [Header("Emotion Settings")]
    [Tooltip("감정 표현 아이콘이 담긴 부모 오브젝트입니다.")]
    public GameObject emotionDisplayObject;
    [Tooltip("감정 표현 애니메이션을 제어할 Animator입니다.")]
    public Animator emotionAnimator;
    [Tooltip("하나의 감정이 표시될 시간(초)입니다.")]
    public float emotionDuration = 2.0f;

    // 현재 감정 표현 코루틴을 저장하여 중복 실행을 막습니다.
    private Coroutine currentEmotionCoroutine;

    void Start()
    {
        // 시작 시에는 반드시 감정 표현을 숨깁니다.
        if (emotionDisplayObject != null)
        {
            emotionDisplayObject.SetActive(false);
        }
    }

    /// <summary>
    /// 외부(주로 PeopleActor)에서 감정 표현을 명령할 때 사용하는 함수입니다.
    /// </summary>
    /// <param name="emotionTriggerName">실행할 애니메이터의 '트리거' 이름</param>
    public void ExpressEmotion(string emotionTriggerName)
    {
        // 감정 표현에 필요한 부품이 없다면 임무를 중단합니다.
        if (emotionDisplayObject == null || emotionAnimator == null) return;
        
        // 만약 이전에 재생 중이던 감정이 있다면, 즉시 중단시킵니다.
        if (currentEmotionCoroutine != null)
        {
            StopCoroutine(currentEmotionCoroutine);
        }

        // 새로운 감정 표현 임무를 시작합니다.
        currentEmotionCoroutine = StartCoroutine(ShowEmotion(emotionTriggerName));
    }

    // 감정을 정해진 시간 동안 보여주고 숨기는 실제 임무 (코루틴)
    private IEnumerator ShowEmotion(string triggerName)
    {
        // 1. 감정 표현 오브젝트를 보이게 합니다.
        emotionDisplayObject.SetActive(true);

        // 2. 애니메이터에 어명을 내려, 해당하는 감정 애니메이션을 재생시킵니다.
        emotionAnimator.SetTrigger(triggerName);

        // 3. 정해진 시간만큼 기다립니다.
        yield return new WaitForSeconds(emotionDuration);

        // 4. 시간이 지나면 다시 감정 표현을 숨깁니다.
        emotionDisplayObject.SetActive(false);
        
        // 임무가 끝났으므로 자신을 비웁니다.
        currentEmotionCoroutine = null; 
    }
}