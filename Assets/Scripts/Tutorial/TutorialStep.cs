using UnityEngine;

[CreateAssetMenu(fileName = "New Tutorial Step", menuName = "Scriptable Objects/Tutorial/Tutorial Step")]
public class TutorialStep : ScriptableObject
{
    [TextArea]
    public string description; // 튜토리얼 설명 텍스트
    public string targetId; // 하이라이트할 대상의 고유 ID
    public bool isUI; // 대상이 UI인지 게임 오브젝트인지 구분
}