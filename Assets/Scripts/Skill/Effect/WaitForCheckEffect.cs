using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// WaitForCheckEffect는 특정 조건(ToBoolCheck)이 충족될 때까지 기다리는 효과 클래스입니다.
/// 주로 연출이나 순차적 실행 제어를 위한 대기 상태 처리에 사용되며,
/// 조건이 만족되면 true를 반환하여 다음 로직으로 넘어갈 수 있게 합니다.
/// </summary>
public class WaitForCheckEffect : Effect
{
    private ToBoolCheck _checker;

    public WaitForCheckEffect(ToBoolCheck checker = null) : base()
    {
        _checker = checker;
        IsInitialized = checker != null;
    }

    public override SkillResult Initialize(params object[] objects)
    {
        try
        {
            if (objects.Length == 0)
                return Failure("WaitForCheckEffect.Initialize: No parameters provided");

            if (objects[0] is ToBoolCheck checker)
            {
                _checker = checker;
                IsInitialized = true;
                return Success("WaitForCheckEffect initialized with checker");
            }

            return Failure($"WaitForCheckEffect.Initialize: Expected ToBoolCheck, got {objects[0]?.GetType()?.Name ?? "null"}");
        }
        catch (Exception ex)
        {
            return Failure("WaitForCheckEffect.Initialize failed", ex);
        }
    }

    public override bool Apply(Actor source, Actor target)
    {
        if (!IsInitialized)
        {
            Debug.LogError("WaitForCheckEffect: Not initialized before Apply");
            return false;
        }

        if (_checker == null)
        {
            Debug.LogError("WaitForCheckEffect: Checker is null");
            return false;
        }

        return _checker._ischeck;
    }

    public override void Clear()
    {
        // WaitForCheckEffect는 상태를 변경하지 않으므로 Clear에서 특별한 작업 없음
    }
}