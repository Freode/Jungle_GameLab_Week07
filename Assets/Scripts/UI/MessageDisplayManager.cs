using System.Collections;
using UnityEngine;
using TMPro;

public class MessageDisplayManager : MonoBehaviour
{
    public static MessageDisplayManager instance { get; private set; }

    [SerializeField] private TextMeshProUGUI messageText; // 메시지를 표시할 TextMeshProUGUI 컴포넌트
    [SerializeField] private float displayDuration = 3f; // 메시지 기본 표시 시간
    [SerializeField] private float fadeDuration = 0.5f; // 메시지 페이드 아웃 시간

    private Coroutine currentMessageCoroutine; // 현재 실행 중인 메시지 코루틴

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        if (messageText != null)
        {
            messageText.gameObject.SetActive(false); // 시작 시 비활성화
        }
    }

    /// <summary>
    /// 화면 중앙 상단에 메시지를 표시합니다.
    /// </summary>
    /// <param name="message">표시할 메시지 텍스트</param>
    /// <param name="color">메시지 색상</param>
    /// <param name="duration">메시지 표시 시간 (페이드 아웃 시간 포함)</param>
    public void ShowMessage(string message, Color color, float duration)
    {
        if (messageText == null) return;

        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
        }

        messageText.text = message;
        messageText.color = new Color(color.r, color.g, color.b, 1f); // 알파값 1로 시작
        messageText.gameObject.SetActive(true);

        currentMessageCoroutine = StartCoroutine(DisplayAndFadeCoroutine(duration));
    }

    private IEnumerator DisplayAndFadeCoroutine(float duration)
    {
        // 지정된 시간 동안 메시지 표시
        yield return new WaitForSeconds(duration - fadeDuration);

        // 페이드 아웃
        float timer = 0f;
        Color startColor = messageText.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (timer < fadeDuration)
        {
            messageText.color = Color.Lerp(startColor, endColor, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        messageText.gameObject.SetActive(false);
        currentMessageCoroutine = null;
    }
}
