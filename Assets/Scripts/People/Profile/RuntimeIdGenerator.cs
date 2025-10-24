// 어디서든 쓰는 세션용 ID 제너레이터
using UnityEngine;

public static class RuntimeIdGenerator
{
    private static int s_next = 1;
    private const int kMax = int.MaxValue - 1;

    // 도메인 리로드 꺼둔 상태에서도 진입 시 초기화
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init() => Reset();

    public static void Reset(int start = 1)
    {
        s_next = Mathf.Clamp(start, 1, kMax);
    }

    public static int Next()
    {
        if (s_next >= kMax) s_next = 1; // 오버플로 방지
        return s_next++;
    }
}
