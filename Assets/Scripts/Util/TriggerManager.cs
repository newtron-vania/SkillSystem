using System;
using System.Collections.Generic;
using UnityEngine;

// GameTrigger: 게임 내 상태(스킬 사용, UI 클릭 등)를 트리거로 관리합니다.
public enum GameTrigger
{
    SkillActivated,
    SkillEditorActivated,
    UIClicked
}

// TriggerManager: 트리거의 활성/비활성 상태를 중앙에서 관리합니다.
public class TriggerManager : Singleton<TriggerManager>
{
    private readonly Dictionary<GameTrigger, bool> activeTriggers = new();

    public override void Awake()
    {
        base.Awake();

        // 모든 트리거 초기화 (기본값: 비활성화)
        foreach (GameTrigger trigger in Enum.GetValues(typeof(GameTrigger))) activeTriggers[trigger] = false;
    }

    /// 특정 트리거를 활성화합니다.
    public void ActivateTrigger(GameTrigger trigger)
    {
        if (activeTriggers.ContainsKey(trigger))
        {
            activeTriggers[trigger] = true;
            Debug.Log($"[INFO] TriggerManager::ActivateTrigger({trigger}) - {trigger} 활성화됨");
        }
    }

    /// 특정 트리거를 비활성화합니다.
    public void DeactivateTrigger(GameTrigger trigger)
    {
        if (activeTriggers.ContainsKey(trigger))
        {
            activeTriggers[trigger] = false;
            Debug.Log($"[INFO] TriggerManager::DeactivateTrigger({trigger}) - {trigger} 비활성화됨");
        }
    }

    /// 특정 트리거가 활성화되어 있는지 확인합니다.
    public bool IsTriggerActive(GameTrigger trigger)
    {
        return activeTriggers.ContainsKey(trigger) && activeTriggers[trigger];
    }

    /// 활성화된 모든 트리거를 가져옵니다.
    public List<GameTrigger> GetActiveTriggers()
    {
        var activeList = new List<GameTrigger>();
        foreach (var trigger in activeTriggers)
            if (trigger.Value)
                activeList.Add(trigger.Key);

        return activeList;
    }
}