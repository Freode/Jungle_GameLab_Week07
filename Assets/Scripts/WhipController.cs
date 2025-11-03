// 파일 이름: WhipController.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 마우스 왼쪽 클릭으로 지정된 범위의 시민들에게 채찍질을 가하는 컨트롤러입니다.
/// </summary>
public class WhipController : MonoBehaviour
{
    [Header("Whip Settings")]
    [Tooltip("채찍이 직접 닿는 안쪽 범위입니다.")]
    public float innerRadius = 2.0f;
    [Tooltip("채찍의 충격이 미치는 바깥쪽 범위입니다.")]
    public float outerRadius = 4.0f;

    [Header("Punishment Settings")]
    [Tooltip("안쪽 범위의 시민들이 잃을 충성심입니다.")]
    public int innerLoyaltyPenalty = 5;
    [Tooltip("바깥쪽 범위의 시민들이 잃을 충성심입니다.")]
    public int outerLoyaltyPenalty = 1;
    [Header("Visual Effects")]
    [Tooltip("채찍이 떨어질 때 생성할 폭발 효과 프리팹입니다.")]
    public GameObject whipExplosionPrefab;
    
    // 처음 감소 로그를 한 번만 찍기 위한 플래그
    private bool _firstLoyaltyLogged = false;
    

    // 마우스 클릭을 감지하여 처벌을 실행합니다.
    void Update()
    {
        if (ClickModeManager.Instance.CurrentMode != ClickMode.Whip) return;

        // 마우스 왼쪽 버튼 클릭을 감지합니다.
        if (Input.GetMouseButtonDown(0))
        {
            // UI 위에 마우스가 있는 경우 처벌을 실행하지 않습니다.
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // UI 위가 아닐 때만 처벌을 실행합니다.
            ExecutePunishment();
        }
    }

    // 처벌을 실행하는 메서드
    void ExecutePunishment()
    {
        Vector2 whipPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 시각 효과 생성
        if (whipExplosionPrefab != null)
        {
            GameObject explosion = Instantiate(whipExplosionPrefab, whipPoint, Quaternion.identity);
            Destroy(explosion, 2f); // 2초 후 자동 삭제
        }

        // 영향 받는 시민 탐지
        Collider2D[] allAffectedColliders = Physics2D.OverlapCircleAll(whipPoint, outerRadius);
        Collider2D[] directHitColliders = Physics2D.OverlapCircleAll(whipPoint, innerRadius);

        // 권위 계산을 위한 변수 초기화
        int directHitCount = 0;
        int nearMissCount = 0;

        // 중복 처벌 방지를 위한 시민 목록
        HashSet<GameObject> punishedCitizens = new HashSet<GameObject>();

        // 안쪽 범위(직접 타격) 처벌 및 카운트
        foreach (var citizenCollider in directHitColliders)
        {
            // PeopleActor 컴포넌트가 있는지 확인
            if (citizenCollider.TryGetComponent<PeopleActor>(out PeopleActor actor))
            {
                directHitCount++;
                punishedCitizens.Add(actor.gameObject);

                // 직접 타격 처리
                HandleDirectHit(actor.gameObject);
            }
        }

        // 바깥쪽 범위 처벌 및 카운트
        foreach (var citizenCollider in allAffectedColliders)
        {
            // 이미 직접 타격으로 처벌된 시민은 제외
            if (punishedCitizens.Contains(citizenCollider.gameObject))
            {
                continue;
            }

            // PeopleActor 컴포넌트가 있는지 확인
            if (citizenCollider.TryGetComponent<PeopleActor>(out PeopleActor actor))
            {
                nearMissCount++;

                // 근접 타격 처리
                HandleNearMiss(actor.gameObject);
            }
        }

        // 권위 계산 및 적용
        if (AuthorityManager.instance != null)
        {
            float totalAuthorityGained = 
                (directHitCount * AuthorityManager.instance.directHitAuthorityGain) + 
                (nearMissCount * AuthorityManager.instance.nearMissAuthorityGain);
            
            // [피버 타임 비활성화] 채찍질 시 권위 증가 주석처리
            // if (totalAuthorityGained > 0)
            // {
            //     AuthorityManager.instance.IncreaseAuthorityByAmount(totalAuthorityGained);
            // }
        }
        
        // 처음 충성도 감소 발생 시 로그 기록
        if (!_firstLoyaltyLogged && (directHitCount > 0 || nearMissCount > 0))
        {
            _firstLoyaltyLogged = true;

            GameLogger.Instance?.Log(
                "Whip",
                $"FirstLoyaltyDecrease/innerHit={directHitCount}/outerHit={nearMissCount}/" +
                $"innerPenalty={innerLoyaltyPenalty}/outerPenalty={outerLoyaltyPenalty}/" +
                $"point=({whipPoint.x:F2},{whipPoint.y:F2})"
            );
        }
    }
    

    // 안쪽 범위 직접 타격 처리
    void HandleDirectHit(GameObject citizen)
    {
        PeopleActor actor = citizen.GetComponent<PeopleActor>();
        EmotionController emotion = citizen.GetComponent<EmotionController>();
        CitizenHighlighter highlighter = citizen.GetComponent<CitizenHighlighter>();

        if (actor != null && emotion != null && highlighter != null)
        {
            // 현재 클릭 모드의 배수를 적용
            float multiplier = ClickModeManager.Instance != null ? ClickModeManager.Instance.GetCurrentMultiplier() : 1f;
            int adjustedPenalty = Mathf.RoundToInt(innerLoyaltyPenalty * multiplier);
            
            actor.ChangeLoyalty(-adjustedPenalty); // 충성심 감소 (배수 적용)
            emotion.ExpressEmotion("Emotion_Angry"); // 분노 표출
            highlighter.FlashRed(); // 붉은 섬광
        }
    }

    // 바깥쪽 범위 근접 타격 처리
    void HandleNearMiss(GameObject citizen)
    {
        PeopleActor actor = citizen.GetComponent<PeopleActor>();
        EmotionController emotion = citizen.GetComponent<EmotionController>();

        if (actor != null && emotion != null)
        {
            // 현재 클릭 모드의 배수를 적용
            float multiplier = ClickModeManager.Instance != null ? ClickModeManager.Instance.GetCurrentMultiplier() : 1f;
            int adjustedPenalty = Mathf.RoundToInt(outerLoyaltyPenalty * multiplier);
            
            actor.ChangeLoyalty(-adjustedPenalty); // 충성심 감소 (배수 적용)
            emotion.ExpressEmotion("Emotion_exclamation"); 
        }
    }

    // 처벌 범위를 시각적으로 표시 (에디터 전용)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, outerRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, innerRadius);
    }
}