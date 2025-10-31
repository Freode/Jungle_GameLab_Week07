using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// AreaType별 rewardCooldown 설정을 담는 클래스 (초기값 설정용)
[System.Serializable]
public class AreaConfig
{
    public AreaType areaType;

    [Header("Cooldown")]
    public float rewardCooldown = 0.5f;

    [Header("Dynamic Steps (Power-based)")]
    [Tooltip("해당 Area의 기본 노력치(클수록 기본 스텝이 큼). 예: 100")]
    public float baseEffort = 100f;

    [Tooltip("동적으로 계산된 스텝의 하한/상한")]
    public int minSteps = 1;
    public int maxSteps = 6;

    [Header("Legacy (optional)")]
    public int goldCollectionSteps = 3;
    public float goldCollectionDelay = 0.5f;
}


[CreateAssetMenu(fileName = "GameConfig", menuName = "Scriptable Objects/GameConfig", order = 1)]
public class GameConfig : ScriptableObject
{
    [Header("Default Reward Cooldown")]
    public float defaultRewardCooldown = 0.5f;

    [Header("Area Specific Reward Cooldowns (Initial Values)")]
    public List<AreaConfig> areaConfigs;

    [Header("Global Power→Step Mapping")]
    [Tooltip("steps = ceil( baseEffort / max(1, power * powerToStepScale) ) 를 기본으로 사용")]
    public float powerToStepScale = 1.0f;

    [Tooltip("동적 스텝 계산 후 전체에 곱하는 후처리 스케일(미세 튜닝용, 1=무효)")]
    public float stepPostScale = 1.0f;

    // 런타임에 변경될 수 있는 값들을 저장하는 딕셔너리 (인스펙터에 노출되지 않음)
    [System.NonSerialized]
    private Dictionary<AreaType, float> _runtimeRewardCooldowns;

    [Header("Default Gold Collection Settings")]
    [Tooltip("기본 골드 수집 횟수 (AreaType별 설정이 없을 때 사용) ")]
    public int defaultGoldCollectionSteps = 3;
    [Tooltip("기본 골드 수집 간 지연 시간 (AreaType별 설정이 없을 때 사용)")]
    public float defaultGoldCollectionDelay = 0.5f;

    // 런타임에 변경될 수 있는 골드 수집 설정 저장 (인스펙터에 노출되지 않음)
    [System.NonSerialized]
    private Dictionary<AreaType, RuntimeGoldCollectionSettings> _runtimeGoldCollectionSettings;

    // baseEffort 런타임 오버라이드 저장(인스펙터 비노출)
    [System.NonSerialized]
    private Dictionary<AreaType, float> _runtimeBaseEfforts;


    // 런타임 골드 수집 설정을 담는 내부 클래스
    private class RuntimeGoldCollectionSettings
    {
        public int steps;
        public float delay;

        public RuntimeGoldCollectionSettings(int steps, float delay)
        {
            this.steps = steps;
            this.delay = delay;
        }
    }

    // 게임 시작 시 한 번 호출하여 런타임 상태를 초기화합니다.
    public void InitializeRuntimeState()
    {
        _runtimeRewardCooldowns = new Dictionary<AreaType, float>();
        _runtimeGoldCollectionSettings = new Dictionary<AreaType, RuntimeGoldCollectionSettings>();
        _runtimeBaseEfforts = new Dictionary<AreaType, float>(); // ★ 추가

        foreach (var config in areaConfigs)
        {
            if (config == null) continue;

            _runtimeRewardCooldowns[config.areaType] = config.rewardCooldown;
            _runtimeGoldCollectionSettings[config.areaType] =
                new RuntimeGoldCollectionSettings(config.goldCollectionSteps, config.goldCollectionDelay);

            // baseEffort 초기값 복사(1 이상 보장)
            _runtimeBaseEfforts[config.areaType] = Mathf.Max(1f, config.baseEffort);
        }

        foreach (AreaType type in Enum.GetValues(typeof(AreaType)))
        {
            if (type == AreaType.Total) continue;

            if (!_runtimeRewardCooldowns.ContainsKey(type))
                _runtimeRewardCooldowns[type] = defaultRewardCooldown;

            if (!_runtimeGoldCollectionSettings.ContainsKey(type))
                _runtimeGoldCollectionSettings[type] =
                    new RuntimeGoldCollectionSettings(defaultGoldCollectionSteps, defaultGoldCollectionDelay);

            // areaConfigs에 없던 타입에 대한 기본 baseEffort (원하면 값 조정)
            if (!_runtimeBaseEfforts.ContainsKey(type))
                _runtimeBaseEfforts[type] = 100f;
        }
    }

    
    /// <summary>
    /// 원의 파워에 따라 동적 스텝 계산.
    /// 기본식: steps = clamp( ceil( baseEffort / max(1, power * powerToStepScale) ) * stepPostScale, minSteps, maxSteps )
    /// </summary>
    public int GetDynamicSteps(AreaType areaType, float currentPower)
    {
        EnsureRuntimeInited(); // ★ 추가

        // ★ 런타임 오버라이드된 baseEffort 우선 사용
        float effort = GetBaseEffort(areaType); // >= 1 보장

        // min/max는 설정 존재 시 그 값을, 없으면 합리적 기본치 사용
        AreaConfig cfg = FindAreaConfig(areaType); // ★ 헬퍼
        int minSteps = (cfg != null) ? Mathf.Max(1, cfg.minSteps) : Mathf.Max(1, defaultGoldCollectionSteps);
        int maxSteps = (cfg != null) ? Mathf.Max(minSteps, cfg.maxSteps) : Mathf.Max(minSteps, 6);

        float denom = Mathf.Max(1f, currentPower * Mathf.Max(0.0001f, powerToStepScale));
        float raw   = effort / denom;
        raw        *= Mathf.Max(0.0001f, stepPostScale);

        int steps = Mathf.CeilToInt(raw);
        steps = Mathf.Clamp(steps, minSteps, maxSteps);
        return steps;
    }

    #region Area별 가중치
    
    // ★ 런타임 딕셔너리 보정
    private void EnsureRuntimeInited()
    {
        if (_runtimeRewardCooldowns == null ||
            _runtimeGoldCollectionSettings == null ||
            _runtimeBaseEfforts == null)
        {
            InitializeRuntimeState();
        }
    }

    // ★ areaConfigs에서 AreaConfig 찾기
    private AreaConfig FindAreaConfig(AreaType areaType)
    {
        if (areaConfigs == null) return null;
        for (int i = 0; i < areaConfigs.Count; i++)
        {
            var c = areaConfigs[i];
            if (c != null && c.areaType == areaType) return c;
        }
        return null;
    }

    // ===== baseEffort 런타임 오버라이드 API =====
    public float GetBaseEffort(AreaType areaType)
    {
        EnsureRuntimeInited();
        if (_runtimeBaseEfforts.TryGetValue(areaType, out float v))
            return Mathf.Max(1f, v);

        var cfg = FindAreaConfig(areaType);
        return Mathf.Max(1f, (cfg != null) ? cfg.baseEffort : 100f);
    }

    public void SetBaseEffort(AreaType areaType, float newEffort)
    {
        EnsureRuntimeInited();
        _runtimeBaseEfforts[areaType] = Mathf.Max(1f, newEffort);
    }

    public void ClearBaseEffortOverride(AreaType areaType)
    {
        EnsureRuntimeInited();
        var cfg = FindAreaConfig(areaType);
        float fallback = Mathf.Max(1f, (cfg != null) ? cfg.baseEffort : 100f);
        _runtimeBaseEfforts[areaType] = fallback;
    }
    
    #endregion

    #region 쿨다운 시간 (현재 3초로 고정)
    
    // 특정 AreaType의 rewardCooldown 값을 가져오는 함수
    public float GetRewardCooldown(AreaType areaType)
    {
        if (_runtimeRewardCooldowns == null) { InitializeRuntimeState(); }

        if (_runtimeRewardCooldowns.TryGetValue(areaType, out float cooldown))
        {
            return cooldown;
        }
        
        Debug.LogWarning($"No runtime rewardCooldown found for {areaType}. Returning defaultRewardCooldown.");
        return defaultRewardCooldown;
    }

    // 특정 AreaType의 rewardCooldown 값을 설정하는 함수 (런타임 변경용)
    public void SetRewardCooldown(AreaType areaType, float newCooldown)
    {
        if (_runtimeRewardCooldowns == null) { InitializeRuntimeState(); }
        _runtimeRewardCooldowns[areaType] = newCooldown;
    }



    #endregion

    #region 골드 수집 횟수 (현재 안씀)
    
    // 특정 AreaType의 골드 수집 횟수를 가져오는 함수
    public int GetGoldCollectionSteps(AreaType areaType)
    {
        if (_runtimeGoldCollectionSettings == null) { InitializeRuntimeState(); }
        if (_runtimeGoldCollectionSettings.TryGetValue(areaType, out RuntimeGoldCollectionSettings settings))
        {
            return settings.steps;
        }
        Debug.LogWarning($"No runtime gold collection steps found for {areaType}. Returning default ({defaultGoldCollectionSteps}).");
        return defaultGoldCollectionSteps;
    }

    // 특정 AreaType의 골드 수집 횟수를 설정하는 함수
    public void SetGoldCollectionSteps(AreaType areaType, int steps)
    {
        if (_runtimeGoldCollectionSettings == null) { InitializeRuntimeState(); }
        if (_runtimeGoldCollectionSettings.TryGetValue(areaType, out RuntimeGoldCollectionSettings settings))
        {
            settings.steps = steps;
        }
        else
        {
            // 해당 AreaType에 대한 설정이 없으면 default 값으로 새롭게 추가
            _runtimeGoldCollectionSettings[areaType] = new RuntimeGoldCollectionSettings(steps, defaultGoldCollectionDelay); 
        }
    }
    #endregion

    #region 골드 수집 지연 시간 (현재 0.5로 일관됨)
    // 특정 AreaType의 골드 수집 지연 시간을 가져오는 함수
    public float GetGoldCollectionDelay(AreaType areaType)
    {
        if (_runtimeGoldCollectionSettings == null) { InitializeRuntimeState(); }
        if (_runtimeGoldCollectionSettings.TryGetValue(areaType, out RuntimeGoldCollectionSettings settings))
        {
            return settings.delay;
        }
        Debug.LogWarning($"No runtime gold collection delay found for {areaType}. Returning default ({defaultGoldCollectionDelay}).");
        return defaultGoldCollectionDelay;
    }

    // 특정 AreaType의 골드 수집 지연 시간을 설정하는 함수
    public void SetGoldCollectionDelay(AreaType areaType, float delay)
    {
        if (_runtimeGoldCollectionSettings == null) { InitializeRuntimeState(); }
        if (_runtimeGoldCollectionSettings.TryGetValue(areaType, out RuntimeGoldCollectionSettings settings))
        {
            settings.delay = delay;
        }
        else
        {
            // 해당 AreaType에 대한 설정이 없으면 default 값으로 새롭게 추가
            _runtimeGoldCollectionSettings[areaType] = new RuntimeGoldCollectionSettings(defaultGoldCollectionSteps, delay); 
        }
    }

    // 전체 지역의 골드 수집 지연 시간을 설정
    public void SetAllGoldCollectionDelay(float delay)
    {
        foreach(var data in _runtimeGoldCollectionSettings)
        {
            data.Value.delay = Mathf.Max(0.1f, data.Value.delay + delay);
        }
    }

    // 현재 골드 수집 지연 시간 가져오기
    public float GetAllGoldCollectionDelay()
    {
        foreach (var data in _runtimeGoldCollectionSettings)
        {
            return data.Value.delay;
        }

        return 0.5f;
    }
    #endregion
}
