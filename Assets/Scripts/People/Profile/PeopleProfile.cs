using UnityEngine;

public enum JobType { None, Worker, Miner, Carver, Carrier, Architect, Guard, Brewer, Priest, God }

public enum CarrierItem
{
    None,
    Stone,
    CarvedStone,
}



[CreateAssetMenu(menuName = "Scriptable Objects/People/People Profile", fileName = "PeopleProfile_")]
public class PeopleProfile : ScriptableObject
{
    [Header("Age")]
    public bool randomAge = true;
    [Min(0)] public int minAge = 18;
    [Min(0)] public int maxAge = 60;
    public int fixedAge = 20;

    [Header("Loyalty (0~100)")]
    public bool randomLoyalty = true;
    [Range(0, 100)] public int minLoyalty = 10;
    [Range(0, 100)] public int maxLoyalty = 90;
    [Range(0, 100)] public int fixedLoyalty = 50;

    [Header("Job")]
    public JobType defaultJob = JobType.None;

    [Header("Carring Item")]
    public CarrierItem defaultCarrier = CarrierItem.None;


    [Header("Name")]
    // public string[] firstNames = new string[]
    // {
    //     "Alex","Blake","Casey","Drew","Evan","Finn",
    //     "Jamie","Jordan","Taylor","Morgan","Quinn","Riley","Avery","Cameron","Hayden","Parker",
    //     "Reese","Rowan","Skyler","Sage","Harper","Logan","Kendall","Peyton","Remy","Charlie",
    //     "Emerson","Dakota","Phoenix","Rory","Micah","Sidney"
    // };

    // public string[] lastNames = new string[]
    // {
    //     "Smith","Johnson","Williams","Brown","Jones","Miller","Davis","Garcia","Rodriguez","Martinez",
    //     "Wilson","Anderson","Taylor","Thomas","Moore","Jackson","Martin","Thompson","White","Harris",
    //     "Clark","Lewis","Robinson","Walker","Young","Allen"
    // };
    public string[] firstNames = new string[]
    {
        // 신/왕·일반 인명 혼합, 2~3음절 위주
        "라","아몬","아톤","호루","세트","토트","프타","네이트","콘수","바스테트",
        "하토르","소벡","세크메트","케프리","아누케트",
        "메네스","나르메르","세티","람세스","투트모세","호렘헤브","페피","테티","우나스","메리트","네페르"
    };

    public string[] lastNames = new string[]
    {
        // 실제 이집트어/왕명·신명 어근에서 온 짧은 형태(2~3음절)
        "호텝",   // (hotep, 평온/만족) Amenhotep의 -hotep
        "모세",   // (mose, ~의 아이) Thutmose/Ramose의 -mose
        "라",     // (Ra, 태양신)
        "레",     // (Re, Ra의 변형)
        "아몬",   // (Amun)
        "아톤",   // (Aten)
        "케프",   // (Khepr-, 케프리 어근)
        "카",     // (ka, 생명력/영혼)
        "카프",   // (kaf/khaf, Khafre의 어감 차용)
        "카레",   // (k3-rae ~ ‘카-라’ 구성 차용)
        "호르",   // (Hor-, Horus 어근)
        "프타",   // (Ptah)
        "소벡",   // (Sobek)
        "네프",   // (Nef-, ‘아름다움/완전’ 계열 어근)
        "수트",   // (wsr/wst 어근 변형, 권위·거룩의 뉘앙스)
        "테프"    // (Tef-, Tefnut 어근)
    };    


    // Fixed 모드
    public string fixedName = "NPC";

    // 프로필에서 새 인스턴스 값 생성
    public PeopleValue Generate()
    {
        var v = new PeopleValue();
        v.age = randomAge ? RandomIntInclusive(minAge, maxAge) : Mathf.Max(0, fixedAge);
        v.loyalty = randomLoyalty ? RandomIntInclusive(minLoyalty, maxLoyalty) : Mathf.Clamp(fixedLoyalty, 0, 100);
        v.name = GenerateName();
        v.job = defaultJob;
        v.carrier = defaultCarrier;
        return v;
    }


    public string GenerateName()
    {
        if (HasAny(firstNames) && HasAny(lastNames))
            return $"{Pick(firstNames)} {Pick(lastNames)}";

        return "NPC";
    }

    // --- 유틸 ---

    private static bool HasAny(string[] arr) => arr != null && arr.Length > 0;

    private static string Pick(string[] arr) => arr[Random.Range(0, arr.Length)];

    // Unity Random.Range(int,int)는 상한 미포함 → 포함 범위 도우미
    private static int RandomIntInclusive(int min, int max)
    {
        if (min > max) (min, max) = (max, min);
        return Random.Range(min, max + 1);
    }

    private void OnValidate()
    {
        if (minAge > maxAge) (minAge, maxAge) = (maxAge, minAge);
        if (minLoyalty > maxLoyalty) (minLoyalty, maxLoyalty) = (maxLoyalty, minLoyalty);

        fixedAge = Mathf.Max(0, fixedAge);
        fixedLoyalty = Mathf.Clamp(fixedLoyalty, 0, 100);
        minLoyalty = Mathf.Clamp(minLoyalty, 0, 100);
        maxLoyalty = Mathf.Clamp(maxLoyalty, 0, 100);
    }
}

[System.Serializable]
public class PeopleValue
{
    public int age;
    public int loyalty; // 0~100
    public string name;
    public JobType job;
    public CarrierItem carrier;
}
