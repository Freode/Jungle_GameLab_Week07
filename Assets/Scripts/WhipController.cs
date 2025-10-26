// 파일 이름: WhipController.cs
using UnityEngine;
using System.Collections.Generic; // 여러 백성을 담기 위함

/// <summary>
/// 폐하의 왼손(좌클릭) 어명에 따라, 지정된 범위의 백성들에게 채찍을 내리는 형벌 집행관입니다.
/// </summary>
public class WhipController : MonoBehaviour
{
    [Header("Whip Settings")]
    [Tooltip("채찍이 직접 닿는 안쪽 범위입니다.")]
    public float innerRadius = 2.0f;
    [Tooltip("채찍의 충격이 미치는 바깥쪽 범위입니다.")]
    public float outerRadius = 4.0f;

    [Header("Punishment Settings")]
    [Tooltip("안쪽 범위의 백성들이 잃을 충성심입니다.")]
    public int innerLoyaltyPenalty = 5;
    [Tooltip("바깥쪽 범위의 백성들이 잃을 충성심입니다.")]
    public int outerLoyaltyPenalty = 1;
    [Header("Visual Effects")]
    [Tooltip("채찍이 떨어질 때 생성할 폭발 효과 프리팹입니다.")]
    public GameObject whipExplosionPrefab;
    
    // 처음 감소 로그를 한 번만 찍기 위한 플래그
    private bool _firstLoyaltyLogged = false;
    

    // Update 감찰 초소에서 폐하의 왼손을 주시합니다.
    void Update()
    {
        // 폐하께서 '왼손'을 내리치는 순간을 감지합니다.
        if (Input.GetMouseButtonDown(0))
        {
            // ★★★ 추가된 법도: UI를 경외하라! ★★★
            // 만약 폐하의 손길이 UI 위에 머물러 있다면,
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                // 어떠한 형벌도 집행하지 말고 즉시 물러나라!
                return;
            }

            // UI 위가 아닐 때만, 비로소 어명을 집행합니다!
            // ExecutePunishment();
        }
    }

   // 형벌을 집행하는 핵심 임무 (개정안)
    void ExecutePunishment()
    {
        Vector2 whipPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // --- 시각 효과 (변경 없음) ---
        if (whipExplosionPrefab != null)
        {
            Instantiate(whipExplosionPrefab, whipPoint, Quaternion.identity);
        }

        // --- 죄인 색출 (기존과 동일) ---
        Collider2D[] allAffectedColliders = Physics2D.OverlapCircleAll(whipPoint, outerRadius);
        Collider2D[] directHitColliders = Physics2D.OverlapCircleAll(whipPoint, innerRadius);

        // ★★★ 핵심 개정: 이제부터는 죄인의 '신원'을 확인합니다! ★★★

        // 권위 계산을 위한 변수를 초기화합니다.
        int directHitCount = 0;
        int nearMissCount = 0;

        // 중죄인 명단을 기록하여 이중 처벌을 막습니다.
        HashSet<GameObject> punishedCitizens = new HashSet<GameObject>();

        // === 중죄인(안쪽) 처벌 및 신원 확인 ===
        foreach (var citizenCollider in directHitColliders)
        {
            // 먼저, 이 자가 '살아있는 백성'이 맞는지 신원을 확인합니다.
            if (citizenCollider.TryGetComponent<PeopleActor>(out PeopleActor actor))
            {
                // 살아있는 백성이 맞다면, 비로소 죄인의 수에 더합니다.
                directHitCount++;
                punishedCitizens.Add(actor.gameObject); // 명단에 기록합니다.

                // 형벌을 집행합니다.
                HandleDirectHit(actor.gameObject);
            }
        }

        // === 경범죄인(바깥쪽) 처벌 및 신원 확인 ===
        foreach (var citizenCollider in allAffectedColliders)
        {
            // 이미 중죄로 다스려진 자는 제외합니다.
            if (punishedCitizens.Contains(citizenCollider.gameObject))
            {
                continue;
            }

            // 이 자 또한 '살아있는 백성'이 맞는지 신원을 확인합니다.
            if (citizenCollider.TryGetComponent<PeopleActor>(out PeopleActor actor))
            {
                // 살아있는 백성이 맞다면, 경범죄인의 수에 더합니다.
                nearMissCount++;

                // 형벌을 집행합니다.
                HandleNearMiss(actor.gameObject);
            }
        }

        // === 권위 상승 보고 (이제 정확한 수로 보고합니다) ===
        if (AuthorityManager.instance != null)
        {
            float totalAuthorityGained = 
                (directHitCount * AuthorityManager.instance.directHitAuthorityGain) + 
                (nearMissCount * AuthorityManager.instance.nearMissAuthorityGain);
            
            if (totalAuthorityGained > 0)
            {
                AuthorityManager.instance.IncreaseAuthorityByAmount(totalAuthorityGained);
            }
        }
        
        // === '처음으로' 충성도 감소 발생 시 단 한 번만 로그 ===
        if (!_firstLoyaltyLogged && (directHitCount > 0 || nearMissCount > 0))
        {
            _firstLoyaltyLogged = true;

            // 로그 양식: [TimeStamp] [LogType] Message/Message/Message
            // -> GameLogger가 TimeStamp를 붙이고, LogType은 "Whip" 파일명으로 표기됨
            GameLogger.Instance?.Log(
                "Whip",
                $"FirstLoyaltyDecrease/innerHit={directHitCount}/outerHit={nearMissCount}/" +
                $"innerPenalty={innerLoyaltyPenalty}/outerPenalty={outerLoyaltyPenalty}/" +
                $"point=({whipPoint.x:F2},{whipPoint.y:F2})"
            );
        }
    }
    

    // 중죄인(안쪽)을 다스리는 절차
    void HandleDirectHit(GameObject citizen)
    {
        PeopleActor actor = citizen.GetComponent<PeopleActor>();
        EmotionController emotion = citizen.GetComponent<EmotionController>();
        CitizenHighlighter highlighter = citizen.GetComponent<CitizenHighlighter>();

        if (actor != null && emotion != null && highlighter != null)
        {
            actor.ChangeLoyalty(-innerLoyaltyPenalty); // 충성심 5 감소
            emotion.ExpressEmotion("Emotion_Angry"); // 분노 표출
            highlighter.FlashRed(); // 붉은 섬광
        }
    }

    // 경범죄인(바깥쪽)을 다스리는 절차
    void HandleNearMiss(GameObject citizen)
    {
        PeopleActor actor = citizen.GetComponent<PeopleActor>();
        EmotionController emotion = citizen.GetComponent<EmotionController>();

        if (actor != null && emotion != null)
        {
            actor.ChangeLoyalty(-outerLoyaltyPenalty); // 충성심 1 감소
            // 폐하께서 말씀하신대로 "Emotion_exclamation"를 표출합니다.
            emotion.ExpressEmotion("Emotion_exclamation"); 
        }
    }

    // 폐하께서 형벌의 범위를 시각적으로 확인하실 수 있도록 돕는 기능이옵니다.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, outerRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, innerRadius);
    }
}