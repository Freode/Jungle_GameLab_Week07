using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// AreaType별 rewardCooldown 설정을 담는 클래스 (초기값 설정용)
[System.Serializable]
public class AreaConfig
{
    public AreaType areaType;
    public float rewardCooldown = 0.5f;
    public int goldCollectionSteps = 3;
    public float goldCollectionDelay = 0.5f;
}

[CreateAssetMenu(fileName = "GameConfig", menuName = "Scriptable Objects/GameConfig", order = 1)]
public class GameConfig : ScriptableObject
{
    [Header("Default Reward Cooldown")]
    [Tooltip("개별 영역 설정이 없을 때 사용될 기본 값입니다.")]
    public float defaultRewardCooldown = 0.5f;

    [Header("Area Specific Reward Cooldowns (Initial Values)")]
    [Tooltip("인스펙터에서 각 AreaType별 초기 rewardCooldown 값을 설정하세요.")]
    public List<AreaConfig> areaConfigs;

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

        // ScriptableObject의 초기값을 런타임 딕셔너리로 복사 및 골드 수집 설정 초기화
        foreach (var config in areaConfigs)
        {
            _runtimeRewardCooldowns[config.areaType] = config.rewardCooldown;
            _runtimeGoldCollectionSettings[config.areaType] = new RuntimeGoldCollectionSettings(config.goldCollectionSteps, config.goldCollectionDelay);
        }

        // GameConfig에 정의되지 않은 AreaType에 대해서는 default 값으로 초기화
        foreach (AreaType type in Enum.GetValues(typeof(AreaType)))
        {
            if (type == AreaType.Total) continue;

            if (!_runtimeRewardCooldowns.ContainsKey(type))
            {
                _runtimeRewardCooldowns[type] = defaultRewardCooldown;
            }
            if (!_runtimeGoldCollectionSettings.ContainsKey(type))
            {
                _runtimeGoldCollectionSettings[type] = new RuntimeGoldCollectionSettings(defaultGoldCollectionSteps, defaultGoldCollectionDelay);
            }
        }
    }

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
}
