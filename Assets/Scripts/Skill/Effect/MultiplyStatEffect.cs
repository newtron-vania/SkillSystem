using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MultiplyStatEffect는 일정 시간 동안 지정된 스탯 항목들을 배수로 증폭시키는 효과 클래스입니다.
/// 효과가 활성화되면 스탯을 수정하며, 시간이 종료되면 원래의 스탯으로 되돌립니다.
/// </summary>
public class MultiplyStatEffect : Effect
{
    private Stat _originalStat;
    private List<float> _multiplyValues;
    private float _duration;
    private bool isActive = false;

    public MultiplyStatEffect(Stat originalStat = null, List<float> multiplyValues = null, float duration = 0f)
    {
        _originalStat = originalStat?.DeepCopy();
        _multiplyValues = multiplyValues;
        _duration = duration;
        IsInitialized = originalStat != null && multiplyValues != null && duration > 0;
    }

    public override SkillResult Initialize(params object[] objects)
    {
        try
        {
            if (objects.Length < 3)
                return Failure("MultiplyStatEffect.Initialize: Expected 3 parameters (Stat, List<float>, float)");

            if (objects[0] is Stat stat && 
                objects[1] is List<float> values && 
                objects[2] is float duration)
            {
                _originalStat = stat.DeepCopy();
                _multiplyValues = values;
                _duration = duration;
                IsInitialized = true;
                return Success($"MultiplyStatEffect initialized with duration: {_duration}");
            }

            return Failure($"MultiplyStatEffect.Initialize: Invalid parameter types");
        }
        catch (Exception ex)
        {
            return Failure("MultiplyStatEffect.Initialize failed", ex);
        }
    }

    public override bool Apply(Actor source, Actor target)
    {
        if (!IsInitialized)
        {
            Debug.LogError("MultiplyStatEffect: Not initialized before Apply");
            return false;
        }

        if (source?.stat == null)
        {
            Debug.LogError("MultiplyStatEffect: Source or source.stat is null");
            return false;
        }

        if (!isActive)
        {
            source.stat.ApplyStatModification(_multiplyValues);
            isActive = true;
        }

        if (_duration <= 0)
        {
            source.stat = _originalStat;
            Clear();
            return true;
        }
        
        _duration -= Time.deltaTime;
        return false;
    }

    public override void Clear()
    {
        isActive = false;
    }
}