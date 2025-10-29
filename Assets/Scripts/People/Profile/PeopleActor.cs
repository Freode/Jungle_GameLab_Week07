using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class PeopleActor : MonoBehaviour
{
    [Header("Death Settings")]
    public GameObject skullPrefab; // 죽었을 때 생성할 해골 프리팹
    public PeopleActorEventChannelSO OnActorDiedChannel; // 죽음을 알릴 방송 채널
    private EmotionController emotionController;
    private Mover mover; // ★★★ 추가: 자신의 수족을 관장할 Mover 장군 ★★★
    private Draggable draggable; // 드래그 가능한 컴포넌트
    private bool isDying = false;
    private bool _isImmortal = false;           // 무적
    private bool _isSpecial = false;            // 특수 객체

    [Header("Runtime Values")]
    [SerializeField] private int id;               // ★ 세션 내 고유 ID
    [SerializeField] private int age;
    [SerializeField, Range(0, 100)] private int loyalty;
    [SerializeField] private string displayName;
    [SerializeField] private JobType job;
    [SerializeField] private CarrierItem carrierItem;

    public int Id => id;
    public int Age => age;
    public int Loyalty => loyalty;
    public string DisplayName => displayName;
    public JobType Job => job;
    public CarrierItem CarrierItem => carrierItem;
    public bool HasReceivedRoyalName { get; private set; } = false;


    void Awake() // Awake 함수가 없다면 새로 만드시고, 있다면 내용을 추가하시옵소서.
    {
        // 임무 시작 시, 자신의 몸에 붙어있는 감정 관리인을 찾아냅니다.
        emotionController = GetComponent<EmotionController>();
        mover = GetComponent<Mover>();
        draggable = GetComponent<Draggable>();
    }

    // ★ '죽음'을 명하는 함수
    public void Die()
    {
        if (isDying) return; // 이미 죽음이 예고되었다면 무시

        isDying = true;
        float delay = Random.Range(0f, 10f); // 0~10초 사이의 랜덤한 시간
        StartCoroutine(DieAfterDelay(delay));
    }

    // ★ 지정된 시간 뒤에 죽음을 실행하는 코루틴
    private IEnumerator DieAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 1. 해골 생성 및 정보 전달
        if (skullPrefab != null)
        {
            Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y, -9f);
            GameObject skullObj = Instantiate(skullPrefab, spawnPosition, Quaternion.identity);
            DraggableSkull skull = skullObj.GetComponent<DraggableSkull>();
            if (skull != null)
            {
                skull.Initialize(this); // 해골에게 자신의 정보를 넘겨줌
            }
        }

        // 2. "내가 죽었노라!" 라고 방송
        if (OnActorDiedChannel != null)
        {
            PeopleManager.Instance.DespawnPerson(this.gameObject);
            OnActorDiedChannel.RaiseEvent(this);
        }
    }

    void OnEnable()
    {
        // Reset the state of the draggable component to ensure it's clean
        if (draggable != null)
        {
            draggable.ResetState();
        }

        // 스폰될 때마다 새 ID 부여
        id = RuntimeIdGenerator.Next();

        _isImmortal = false;
        _isSpecial = false;
    }

    public void Apply(PeopleValue v)
    {
        if (v == null) return;
        age = Mathf.Max(0, v.age);
        loyalty = Mathf.Clamp(v.loyalty, 0, 100);
        displayName = string.IsNullOrWhiteSpace(v.name) ? "NPC" : v.name;
        job = v.job;
        carrierItem = v.carrier;
        HasReceivedRoyalName = false;
    }

    public void ApplyJop(JobType _job)
    {
        job = _job;
        return;
    }

    public void SetCarrierItem(CarrierItem _carrierItem)
    {
        carrierItem = _carrierItem;
    }

    public void ChangeName(string newName)
    {
        // 유효성 체크
        if (string.IsNullOrWhiteSpace(newName)) return;

        // 이미 같은 이름이면 변경 없음
        if (displayName == newName) return;

        // 변경 전 이름 기억
        string oldName = string.IsNullOrWhiteSpace(displayName) ? "NPC" : displayName;

        // 실제 변경
        displayName = newName;

        // 첫 변경일 때만 로그 1회 기록
        if (!HasReceivedRoyalName)
        {
            // 사진 양식: [TimeStamp] [LogType] Message/Message/Message
            // → LogType = "Citizen", Message는 / 로 구분
            GameLogger.Instance?.Log(
                "Citizen",
                $"FirstNameChange/id={id}/from={oldName}/to={displayName}"
            );
        }

        HasReceivedRoyalName = true;
    }

    public void AddAge(int amount)
    {
        age += amount;
    }

    void OnDisable()
    {
        // 다음 스폰 시 새 ID를 받게 하려면 0으로 리셋
        id = 0;

        // 선택: 나머지 런타임 상태 리셋
        age = 0;
        loyalty = 0;
        displayName = null;
        job = JobType.None;
        carrierItem = CarrierItem.None;

        HasReceivedRoyalName = false;
        isDying = false;
    }

    /// <summary>
    /// 충성심을 지정된 양만큼 변경합니다. (음수도 가능)
    /// </summary>
    /// <param name="amount">변화시킬 충성도의 양</param>
    public void ChangeLoyalty(int amount)
    {
        // 무적 캐릭터는 데미지를 입지 않음.
        if (_isImmortal)
            return;

        loyalty += amount;
        // 충성심은 0과 100 사이를 벗어날 수 없다는 왕국의 법도를 적용합니다.
        loyalty = Mathf.Clamp(loyalty, 0, 100);

        if (loyalty <= 0 && !isDying)
        {
            isDying = true; // 이중 선고를 막기 위해 즉시 기록합니다.

            // Mover 집행관에게 "불충으로 인한 죽음을 집행하라"고 명합니다!
            if (mover != null)
            {
                mover.ExecuteDeathByDisloyalty();
            }
            else // 만약 집행관이 없다면, 기존 방식대로 처리합니다.
            {
                Die(); 
            }
        }

    }
    
    /// <summary>
    /// 충성심을 특정 값으로 즉시 설정합니다.
    /// </summary>
    /// <param name="value">설정할 충성도의 값</param>
    public void SetLoyalty(int value)
    {
        loyalty = Mathf.Clamp(value, 0, 100);
    }

    // 특수 객체로 설정
    public void SetSpecialCharacter()
    {
        _isSpecial = true;
        SetImmortal();
    }

    public bool GetSpeicalCharacter() { return  _isSpecial; }

    // 무적 설정
    public void SetImmortal()
    {
        _isImmortal = true;
    }

    public bool GetImmortal() { return _isImmortal; }
}
