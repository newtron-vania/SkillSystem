using System;
using System.Collections.Generic;
using UnityEngine;

// TriggerManager: 트리거의 활성/비활성 상태를 중앙에서 관리합니다.
public class TriggerManager : Singleton<TriggerManager>
{
    private readonly Dictionary<Define.GameTrigger, bool> activeTriggers = new();

    public override void Awake()
    {
        base.Awake();

        // 모든 트리거 초기화 (기본값: 비활성화)
        foreach (Define.GameTrigger trigger in Enum.GetValues(typeof(Define.GameTrigger))) activeTriggers[trigger] = false;
    }

    /// 특정 트리거를 활성화합니다.
    public void ActivateTrigger(Define.GameTrigger trigger)
    {
        if (activeTriggers.ContainsKey(trigger))
        {
            activeTriggers[trigger] = true;
            Debug.Log($"[INFO] TriggerManager::ActivateTrigger({trigger}) - {trigger} 활성화됨");
        }
    }

    /// 특정 트리거를 비활성화합니다.
    public void DeactivateTrigger(Define.GameTrigger trigger)
    {
        if (activeTriggers.ContainsKey(trigger))
        {
            activeTriggers[trigger] = false;
            Debug.Log($"[INFO] TriggerManager::DeactivateTrigger({trigger}) - {trigger} 비활성화됨");
        }
    }

    /// 특정 트리거가 활성화되어 있는지 확인합니다.
    public bool IsTriggerActive(Define.GameTrigger trigger)
    {
        return activeTriggers.ContainsKey(trigger) && activeTriggers[trigger];
    }

    /// 활성화된 모든 트리거를 가져옵니다.
    public List<Define.GameTrigger> GetActiveTriggers()
    {
        var activeList = new List<Define.GameTrigger>();
        foreach (var trigger in activeTriggers)
            if (trigger.Value)
                activeList.Add(trigger.Key);

        return activeList;
    }
}