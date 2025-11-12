using System;
using UnityEngine;

/// <summary>
/// 에어본(공중 띄우기) 효과 클래스입니다.
/// </summary>
public class AirBonrneEffect : Effect
{
    private float _duration;

    protected override SkillResult DoInitialize(params object[] objects)
    {
        var validation = ValidateParameters(objects, 1, nameof(AirBonrneEffect));
        if (!validation.IsSuccess) return validation;

        try
        {
            _duration = SafeCast<float>(objects[0], "duration");
            return Success($"AirBonrneEffect initialized with duration: {_duration}s");
        }
        catch (Exception ex)
        {
            return Failure("Failed to initialize AirBonrneEffect", ex);
        }
    }

    protected override bool DoApply(Actor source, Actor target)
    {
        if (target == null)
        {
            Debug.LogError("Target is null for AirBonrneEffect");
            return false;
        }

        target.ApplyAirborneEffect(_duration);
        Debug.Log($"Applied airborne effect to {target.championName} for {_duration}s");
        return true;
    }

    protected override void DoClear()
    {
        _duration = 0f;
    }
}