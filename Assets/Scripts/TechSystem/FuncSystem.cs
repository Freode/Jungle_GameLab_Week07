using System.Collections.Generic;
using System.Xml.Linq;

public static class FuncSystem
{
    // 숫자 형식 변경
    public static string Format(decimal number)
    {
        // 음수 처리
        bool isNegative = number < 0;
        decimal absNumber = isNegative ? -number : number;
        
        string formatted = absNumber switch
        {
            // 100경 (Quintillion) 이상
            >= 1_000_000_000_000_000_000 => (absNumber / 1_000_000_000_000_000_000).ToString("F2") + "Qi",
            // 1000조 (Quadrillion) 이상
            >= 1_000_000_000_000_000 => (absNumber / 1_000_000_000_000_000).ToString("F2") + "Qa",
            // 1조 (Trillion) 이상
            >= 1_000_000_000_000 => (absNumber / 1_000_000_000_000).ToString("F2") + "T",
            // 10억 (Billion) 이상
            >= 1_000_000_000 => (absNumber / 1_000_000_000).ToString("F2") + "B",
            // 100만 (Million) 이상
            >= 1_000_000 => (absNumber / 1_000_000).ToString("F2") + "M",
            // 1천 (Kilo) 이상
            >= 1_000 => (absNumber / 1_000).ToString("F2") + "K",
            // 1천 미만
            _ => ((long)absNumber).ToString()
        };
        
        return isNegative ? "-" + formatted : formatted;
    }

    // 랜덤으로 숫자 반환
    public static long RandomLongRange(long min, long max)
    {
        decimal dt = (decimal)UnityEngine.Random.value;
        return (long)(min + (max - min) * dt);
    }

    // 구조물 이름 가져오기
    public static string GetStructureName(AreaType areaType, int currentLevel)
    {
        string name = string.Empty;
        switch(areaType)
        {
            case AreaType.Mine:
                name = (currentLevel != 0) ? "광산" : "???";
                break; ;

            case AreaType.Gold:
                name = "움집";
                break;

            case AreaType.StoneCarving:
                name = (currentLevel != 0) ? "세공소" : "???";
                break;

            case AreaType.Carrier:
                name = (currentLevel != 0) ? "운반소" : "???";
                break;

            case AreaType.Architect:
                name = (currentLevel != 0) ? "건축소" : "???";
                break;

            case AreaType.Pyramid:
                name = "피라미드";
                break;

            case AreaType.Barrack:
                name = (currentLevel != 0) ? "병영" : "???";
                break;

            case AreaType.Temple:
                name = (currentLevel != 0) ? "신전" : "???";
                break;

            case AreaType.Brewery:
                name = (currentLevel != 0) ? "양조장" : "???";
                break;

            case AreaType.Special:
                name = (currentLevel != 0) ? "특별구역" : "???";
                break;

            default:
                name = "없음";
                break;
        }

        return name;
    }

    // 구조물 효과 반환
    public static string GetStructureDescription(AreaType areaType, long linearAmount, long periodAmount, int currentLevel, int maxLevel)
    {
        string description = string.Empty;
        switch (areaType)
        {
            case AreaType.Mine:
                if (currentLevel != 0)
                    description = $"클릭당 금 : +{Format(linearAmount)}\n" +
                        $"초당 금 : +{Format(periodAmount)}\n";
                else
                    description = "버려진 땅";
                break;

            case AreaType.Gold:
                description = $"클릭당 금 : +{Format(linearAmount)}\n" +
                    $"초당 금 : +{Format(periodAmount)}\n" +
                    $"무직 생성 주기 : {GameManager.instance.GetRespawnTime().ToString("F3")}초\n";
                break;

            case AreaType.StoneCarving:
                if (currentLevel != 0)
                    description = $"클릭당 금 : +{Format(linearAmount)}\n" +
                        $"초당 금 : +{Format(periodAmount)}\n";
                else
                    description = "버려진 땅";

                break;

            case AreaType.Carrier:
                if (currentLevel != 0)
                    description = $"클릭당 금 : +{Format(linearAmount)}\n" +
                        $"초당 금 : +{Format(periodAmount)}\n";
                else
                    description = "버려진 땅";
                break;

            case AreaType.Architect:
                if (currentLevel != 0)
                    description = $"클릭당 금 : +{Format(linearAmount)}\n" +
                        $"초당 금 : +{Format(periodAmount)}\n";
                else
                    description = "버려진 땅";
                break;

            case AreaType.Pyramid:
                description = $"피라미드 진척도 : {currentLevel}/{maxLevel}\n";
                break;

            case AreaType.Special:
                if (currentLevel != 0)
                    description = $"클릭당 금 : +{Format(linearAmount)}\n" +
                        $"초당 금 : +{Format(periodAmount)}\n";
                else
                    description = "특별한 땅";
                break;

            case AreaType.Temple:
                if (currentLevel != 0)
                    description = $"클릭당 금 : +{Format(linearAmount)}\n" +
                        $"초당 금 : +{Format(periodAmount)}\n";
                else
                    description = "버려진 땅";
                break;

            case AreaType.Barrack:
                if (currentLevel != 0)

                    description = $"자동 클릭 주기 : +{GameManager.instance.GetAutoClickInterval():F2}s\n" +
                        $"1초당 자동 클릭으로 획득하는 금 : +{Format(GameManager.instance.curAutoGoldAmount)}\n";
                else
                    description = "버려진 땅";
                break;

            case AreaType.Brewery:
                if (currentLevel != 0)
                    description = $"클릭당 금 : +{Format(linearAmount)}\n" +
                        $"초당 금 : +{Format(periodAmount)}\n";
                else
                    description = "버려진 땅";
                break;

            default:
                description = "없음";
                break;
        }

        return description;
    }
}
