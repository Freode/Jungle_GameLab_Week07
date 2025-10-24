using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Tutorial Sequence", menuName = "Scriptable Objects/Tutorial/Tutorial Sequence")]
public class TutorialSequence : ScriptableObject
{
    public List<TutorialStep> steps; // 튜토리얼 단계들의 리스트
}