using System;
using System.Collections.Generic;

public class DeActivateTriggerEffect : Effect
{
    private List<GameTrigger> _triggers;

    public DeActivateTriggerEffect(params object[] objects)
    {
        if (objects[0] is List<GameTrigger> list) _triggers = list;
    }

    public override void Initialize(params object[] objects)
    {
        if (objects[0] is List<GameTrigger> list) _triggers = list;
    }

    public override bool Apply(Actor source, Actor target)
    {
        foreach (var trigger in _triggers) TriggerManager.Instance.DeactivateTrigger(trigger);

        return true;
    }

    public override void Clear()
    {
        // No state to clear for this effect.
    }
}