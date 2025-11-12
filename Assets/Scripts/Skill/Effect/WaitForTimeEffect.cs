using System;
using UnityEngine;

public class WaitForTimeEffect : Effect
{
    private float _duration;
    private float _remainingTime;

    public WaitForTimeEffect(float duration = 0f)
    {
        _duration = duration;
        _remainingTime = duration;
        IsInitialized = duration > 0;
    }

    public override SkillResult Initialize(params object[] objects)
    {
        try
        {
            if (objects.Length == 0)
                return Failure("WaitForTimeEffect.Initialize: No parameters provided");

            if (objects[0] is float duration)
            {
                _duration = duration;
                _remainingTime = duration;
                IsInitialized = duration > 0;
                return Success($"WaitForTimeEffect initialized with duration: {_duration}");
            }

            return Failure($"WaitForTimeEffect.Initialize: Expected float, got {objects[0]?.GetType()?.Name ?? "null"}");
        }
        catch (Exception ex)
        {
            return Failure("WaitForTimeEffect.Initialize failed", ex);
        }
    }

    public override bool Apply(Actor source, Actor target)
    {
        if (!IsInitialized)
        {
            Debug.LogError("WaitForTimeEffect: Not initialized before Apply");
            return true; // End immediately if not initialized
        }

        _remainingTime -= Time.deltaTime;
        return _remainingTime <= 0f;
    }

    public override void Clear()
    {
        _remainingTime = _duration; // 다음 사용을 위해 리셋
    }
}